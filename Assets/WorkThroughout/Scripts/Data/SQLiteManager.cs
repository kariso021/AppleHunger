using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;



/// <summary>
/// 클라이언트 로컬 데이터 서버 관련 매니저
/// </summary>
public class SQLiteManager : MonoBehaviour
{
    private static SQLiteManager instance;
    public static SQLiteManager Instance => instance;

    private string dbName = "game_data.db";
    private string dbPath;

    public PlayerData player;
    public List<PlayerRankingData> rankings = new List<PlayerRankingData>();
    public Dictionary<int, PlayerRankingData> rankDictionary = new Dictionary<int, PlayerRankingData>();
    public PlayerRankingData myRankingData;
    public PlayerStatsData stats;
    public LoginData login;
    public List<MatchHistoryData> matches = new List<MatchHistoryData>();
    public List<PlayerItemData> items = new List<PlayerItemData>();
    public PlayerDetails playerDetails;

    // 데이터로드가 끝나면 실행될 이벤트
    public event Action OnSQLiteDataLoaded;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임이 진행하는 동안엔 삭제가 일어나면 안되므로

            player.deviceId = TransDataClass.deviceIdToApply;
            player.googleId = TransDataClass.googleIdToApply;

            StartCoroutine(InitializeDatabase());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private IEnumerator InitializeDatabase()
    {
        string rawDbPath = Path.Combine(Application.persistentDataPath, dbName).Replace("\\", "/");
        dbPath = "URI=file:" + rawDbPath;  // SQLite 연결을 위해 여전히 사용
        Debug.Log($"📂 SQLite DB 경로: {dbPath}");

        // ✅ Step 1: SQLite DB가 존재하는지 확인
        if (File.Exists(rawDbPath))
        {
            Debug.Log("✅ SQLite DB가 이미 존재합니다. 서버 요청 없이 로컬 DB 사용.");

            // ✅ Step 2: DB가 존재하면 서버에서 데이터를 받을 필요 없이 로드 후 종료
            LoadAllData();
            yield return DataSyncManager.Instance.PlayerRankingUpdated();
            saveRankDataToDictionary();
            DataSyncManager.Instance.PlayerItemsUpdated();
            yield break;
        }

        yield return StartCoroutine(CreateDatabaseAndFetchPlayerData());

        Debug.Log("데이터 찾기 끝난듯");
        LoadAllData();

        //dbPath = "URI=file:" + Path.Combine(Application.persistentDataPath, dbName);
    }
    private IEnumerator CreateDatabaseAndFetchPlayerData()
    {
        yield return StartCoroutine(CreateDatabase()); // ✅ SQLite DB 생성
    }
    private void createTables(SqliteConnection connection)
    {
        // 🔹 플레이어 테이블 (Auto-Increment 제거)
        ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS players (
                    playerId INTEGER PRIMARY KEY,  
                    deviceId TEXT UNIQUE,
                    googleId TEXT UNIQUE,
                    playerName TEXT NOT NULL UNIQUE,
                    profileIcon TEXT DEFAULT NULL,
                    boardImage TEXT DEFAULT NULL,
                    rating INTEGER DEFAULT 1200,
                    currency INTEGER DEFAULT 0,
                    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );");

        // 🔹 플레이어 스탯 테이블
        ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS playerStats (
                    playerId INTEGER PRIMARY KEY,  
                    totalGames INTEGER DEFAULT 0,
                    wins INTEGER DEFAULT 0,
                    losses INTEGER DEFAULT 0,
                    winRate REAL GENERATED ALWAYS AS (CASE WHEN totalGames = 0 THEN 0 ELSE (wins * 100.0 / totalGames) END) STORED,
                    FOREIGN KEY (playerId) REFERENCES players(playerId) ON DELETE CASCADE
                );");

        // 🔹 플레이어 아이템 테이블
        ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS playerItems (
                    itemId INTEGER PRIMARY KEY,  
                    playerId INTEGER NOT NULL,
                    itemUniqueId INTEGER NOT NULL,
                    itemType TEXT CHECK(itemType IN ('icon', 'board')),
                    price INTEGER DEFAULT 0,
                    isUnlocked INTEGER DEFAULT 0,
                    acquiredAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (playerId) REFERENCES players(playerId) ON DELETE CASCADE
                );");


        // 🔹 로그인 기록 테이블
        ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS loginRecords (
                    loginId INTEGER PRIMARY KEY,  
                    playerId INTEGER NOT NULL,
                    loginTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    ipAddress TEXT DEFAULT NULL,
                    FOREIGN KEY (playerId) REFERENCES players(playerId) ON DELETE CASCADE
                );");

        // 🔹 매치 기록 테이블
        ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS matchRecords (
                    matchId INTEGER PRIMARY KEY,  
                    player1Id INTEGER NOT NULL,
                    player1Name TEXT NOT NULL,
                    player1Rating INTEGER NOT NULL,
                    player1Icon TEXT NOT NULL,
        
                    player2Id INTEGER NOT NULL,
                    player2Name TEXT NOT NULL,
                    player2Rating INTEGER NOT NULL,
                    player2Icon TEXT NOT NULL,

                    winnerId INTEGER NOT NULL,
                    matchDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        
                    FOREIGN KEY (player1Id) REFERENCES players(playerId) ON DELETE CASCADE,
                    FOREIGN KEY (player2Id) REFERENCES players(playerId) ON DELETE CASCADE,
                    FOREIGN KEY (winnerId) REFERENCES players(playerId) ON DELETE CASCADE
                );");

        // 🔹 랭킹 테이블 (상위 50명의 랭킹 정보 저장)
        // 🔹 rankings 테이블 (상위 50명의 랭킹 정보 저장)
        ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS rankings (           
                    playerId INTEGER NOT NULL,         -- 플레이어 ID
                    playerName TEXT NOT NULL,          -- 플레이어 닉네임
                    rating INTEGER NOT NULL,           -- 레이팅 점수
                    rankPosition INTEGER PRIMARY KEY,  -- 랭킹 순위 (1~50)
                    profileIcon TEXT DEFAULT NULL,     -- 플레이어 프로필 아이콘 (마지막 컬럼)
                    FOREIGN KEY (playerId) REFERENCES players(playerId) ON DELETE CASCADE
                );");

        // ✅ 개별 플레이어 랭킹 테이블 생성
        ExecuteQuery(connection, @"
                CREATE TABLE IF NOT EXISTS myRanking (
                    playerId INTEGER PRIMARY KEY,
                    playerName TEXT NOT NULL,
                    rating INTEGER NOT NULL,
                    rankPosition INTEGER NOT NULL,
                    profileIcon TEXT DEFAULT NULL  -- 프로필 아이콘 (마지막 컬럼)
                );
                ");


        Debug.Log("✅ SQLite 데이터베이스 초기화 완료 (Mono.Data.Sqlite)");

        ExecuteQuery(connection, @"
                DELETE FROM matchRecords 
                WHERE matchId NOT IN (
                    SELECT matchId FROM matchRecords 
                    ORDER BY matchDate DESC 
                    LIMIT 10
                );
            ");

        Debug.Log("✅ matchRecords 10개 유지 완료");
    }
    private IEnumerator CreateDatabase()
    {
        string streamingDbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        string persistentDbPath = Path.Combine(Application.persistentDataPath, dbName);

        // 📌 Step 2: `streamingAssetsPath`에서 복사 (PC, iOS)
        if (Application.platform != RuntimePlatform.Android)
        {
            if (File.Exists(streamingDbPath))
            {
                File.Copy(streamingDbPath, persistentDbPath, true);
                Debug.Log("✅ SQLite DB 파일 복사 완료! (PC, iOS)");
                yield break;
            }
            else
            {
                Debug.LogError("❌ StreamingAssets 폴더에 DB 파일이 존재하지 않음!");
            }
        }
        else // 📌 Step 3: `streamingAssetsPath`에서 다운로드 (Android)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(streamingDbPath))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(persistentDbPath, request.downloadHandler.data);
                    Debug.Log("✅ Android에서 SQLite DB 복사 완료!");
                    yield break;
                }
                else
                {
                    Debug.LogError("❌ Android에서 DB 다운로드 실패: " + request.error);
                }
            }
        }

        // 📌 Step 4: `persistentDataPath`에도 DB가 없으면 새로 생성
        if (!File.Exists(persistentDbPath))
        {
            Debug.LogWarning("⚠️ DB 파일이 어디에도 없음 → 새로 생성합니다.");
            yield return StartCoroutine(CreateNewDatabase(persistentDbPath));
        }
    }


    private IEnumerator CreateNewDatabase(string dbPath)
    {
        try
        {
            using (var connection = new SqliteConnection("URI=file:" + dbPath))
            {
                connection.Open();
                Debug.Log("✅ 새 SQLite 데이터베이스 생성 완료!");
                createTables(connection);
            }
            Debug.Log("✅ 새 SQLite 데이터베이스 테이블 생성 완료!");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 새 DB 생성 실패: " + e.Message);
        }

        // ✅ 서버에서 플레이어 데이터 가져오기 (먼저 실행해야 함)
        if (ClientNetworkManager.Instance != null)
        {
            Debug.Log("🌍 [Client] 서버에서 플레이어 데이터 요청 중...");

            // ✅ 먼저 플레이어 데이터를 받아옴
            yield return StartCoroutine(ClientNetworkManager.Instance.GetPlayerData(player.googleId == null ? "deviceId" : "googleId", player.googleId == null ? player.deviceId : player.googleId,true));


            // ✅ 플레이어 ID가 `0`이 아닐 때까지 기다림
            yield return new WaitUntil(() => SQLiteManager.Instance.LoadPlayerData().playerId != 0);
            // ✅ player.playerId가 설정된 후에 나머지 요청을 병렬 실행
            Debug.Log($"✅ 플레이어 ID 확인: {SQLiteManager.Instance.LoadPlayerData().playerId}");

            // ✅ 병렬 요청을 위한 플래그 설정
            bool isPlayerStatsLoaded = false;
            bool isLoginDataLoaded = false;
            bool isMatchRecordsLoaded = false;
            bool isPlayerItemsLoaded = false;
            bool isRankingListLoaded = false;

            // ✅ 나머지 데이터를 병렬로 요청
            StartCoroutine(LoadPlayerStatsServerRpc(ClientNetworkManager.Instance, () => isPlayerStatsLoaded = true));
            StartCoroutine(LoadLoginDataServerRpc(ClientNetworkManager.Instance, () => isLoginDataLoaded = true));
            StartCoroutine(LoadMatchRecordsServerRpc(ClientNetworkManager.Instance, () => isMatchRecordsLoaded = true));
            StartCoroutine(LoadPlayerItemsServerRpc(ClientNetworkManager.Instance, () => isPlayerItemsLoaded = true));
            StartCoroutine(LoadRankingListServerRpc(ClientNetworkManager.Instance, () => isRankingListLoaded = true));

            // ✅ 모든 요청이 끝날 때까지 대기
            yield return new WaitUntil(() =>
                isPlayerStatsLoaded &&
                isLoginDataLoaded &&
                isMatchRecordsLoaded &&
                isPlayerItemsLoaded &&
                isRankingListLoaded
            );

            Debug.Log("✅ [Client] 모든 데이터 동기화 완료!");
        }
    }
    #region Init Data Load
    private IEnumerator LoadPlayerStatsServerRpc(ClientNetworkManager clientNetworkManager, Action onComplete)
    {
        yield return StartCoroutine(clientNetworkManager.GetPlayerStats(SQLiteManager.Instance.LoadPlayerData().playerId));
        onComplete();
    }

    private IEnumerator LoadLoginDataServerRpc(ClientNetworkManager clientNetworkManager, Action onComplete)
    {
        yield return StartCoroutine(clientNetworkManager.GetLogin(SQLiteManager.Instance.LoadPlayerData().playerId));
        onComplete();
    }

    private IEnumerator LoadMatchRecordsServerRpc(ClientNetworkManager clientNetworkManager, Action onComplete)
    {
        yield return StartCoroutine(clientNetworkManager.GetMatchRecords(SQLiteManager.Instance.LoadPlayerData().playerId));
        onComplete();
    }

    private IEnumerator LoadPlayerItemsServerRpc(ClientNetworkManager clientNetworkManager, Action onComplete)
    {
        yield return StartCoroutine(clientNetworkManager.GetPlayerItems(SQLiteManager.Instance.LoadPlayerData().playerId));
        onComplete();
    }

    private IEnumerator LoadRankingListServerRpc(ClientNetworkManager clientNetworkManager, Action onComplete)
    {
        yield return StartCoroutine(clientNetworkManager.GetRankingList());
        onComplete();
    }



    #endregion


    /// <summary>
    /// SQLite 쿼리 실행 함수
    /// </summary>
    private void ExecuteQuery(SqliteConnection connection, string query)
    {
        using (var command = new SqliteCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    // 🔹 모든 데이터 불러오기 (게임 시작 시 실행)
    public void LoadAllData()
    {
        Debug.Log("🔹 모든 데이터를 SQLite에서 불러옵니다.");
        player = LoadPlayerData();
        stats = LoadPlayerStats();
        login = LoadLoginData();
        matches = LoadMatchHistory();
        items = LoadPlayerItems();
        rankings = LoadRankings();
        myRankingData = LoadMyRankingData();

        Debug.Log($"✅ 플레이어 데이터 불러오기 완료: {player?.playerName ?? "없음"}");
        if (stats != null) Debug.Log($"✅ 플레이어 스탯 불러오기 완료: id = {stats.playerId} , total = {stats.totalGames} , wins = {stats.wins}");
        Debug.Log($"✅ 로그인 데이터 불러오기 완료: {login?.loginTime ?? "없음"}");
        Debug.Log($"✅ 매치 기록 개수: {matches.Count}");
        Debug.Log($"✅ 보유 아이템 개수: {items.Count}");
        Debug.Log($"✅ 랭킹 인원 수 : {rankings.Count}");

        // 모든 데이터가 로드되면 그 떄 UI 업데이트를 실행
        OnSQLiteDataLoaded?.Invoke();
    }

    private void saveRankDataToDictionary()
    {
        foreach (var rankData in rankings)
        {
            rankDictionary.Add(rankData.playerId, rankData);
        }
    }

    public void SavePlayerData(PlayerData player)
    {
        //Debug.Log(player.ToString());
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
            INSERT OR REPLACE INTO players 
            (playerId, deviceId, googleId, playerName, profileIcon, boardImage, rating, currency, createdAt) 
            VALUES (@playerId, @deviceId, @googleId, @playerName, @profileIcon, @boardImage, @rating, @currency, @createdAt);
            ";

                command.Parameters.AddWithValue("@playerId", player.playerId);
                command.Parameters.AddWithValue("@deviceId", player.deviceId);
                command.Parameters.AddWithValue("@googleId", player.googleId);
                command.Parameters.AddWithValue("@playerName", player.playerName);
                command.Parameters.AddWithValue("@profileIcon", player.profileIcon);
                command.Parameters.AddWithValue("@boardImage", player.boardImage);
                command.Parameters.AddWithValue("@rating", player.rating);
                command.Parameters.AddWithValue("@currency", player.currency);
                command.Parameters.AddWithValue("@createdAt", player.createdAt);

                int rowsAffected = command.ExecuteNonQuery();
                //Debug.Log($"✅ 플레이어 데이터 저장 완료! 변경된 행 수: {rowsAffected}");
            }
        }
    }

    // 🔹 플레이어 스탯 저장
    public void SavePlayerStats(PlayerStatsData stats)
    {
        Debug.Log($"🔹 SQLite 저장: PlayerID={stats.playerId}, TotalGames={stats.totalGames}, Wins={stats.wins}, Losses={stats.losses}, WinRate={stats.winRate}");

        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                INSERT INTO playerStats (playerId, totalGames, wins, losses)
                VALUES (@playerId, @totalGames, @wins, @losses)
                ON CONFLICT(playerId) DO UPDATE SET
                totalGames = excluded.totalGames,
                wins = excluded.wins,
                losses = excluded.losses;
            ";

                command.Parameters.AddWithValue("@playerId", stats.playerId);
                command.Parameters.AddWithValue("@totalGames", stats.totalGames);
                command.Parameters.AddWithValue("@wins", stats.wins);
                command.Parameters.AddWithValue("@losses", stats.losses);

                int rowsAffected = command.ExecuteNonQuery();
                Debug.Log($"✅ 플레이어 스탯 SQLite 저장 완료, 변경된 행 수: {rowsAffected}");
            }
            connection.Close();
        }
    }

    // 🔹 로그인 데이터 저장
    public void SaveLoginData(LoginData login)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT OR REPLACE INTO loginRecords 
                    (loginId, playerId, loginTime, ipAddress) 
                    VALUES (@loginId, @playerId, @loginTime, @ipAddress);
                ";

                command.Parameters.AddWithValue("@loginId", login.loginId);
                command.Parameters.AddWithValue("@playerId", login.playerId);
                command.Parameters.AddWithValue("@loginTime", login.loginTime);
                command.Parameters.AddWithValue("@ipAddress", login.ipAddress);

                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        Debug.Log("✅ 로그인 데이터 SQLite에 저장 완료");
    }

    // 🔹 매치 기록 저장
    public void SaveMatchHistory(MatchHistoryData match)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                INSERT OR REPLACE INTO matchRecords 
                (matchId, player1Id, player1Name, player1Rating, player1Icon,
                 player2Id, player2Name, player2Rating, player2Icon, 
                 winnerId, matchDate) 
                VALUES (@matchId, @player1Id, @player1Name, @player1Rating, @player1Icon,
                        @player2Id, @player2Name, @player2Rating, @player2Icon,
                        @winnerId, @matchDate);
            ";

                command.Parameters.AddWithValue("@matchId", match.matchId);
                command.Parameters.AddWithValue("@player1Id", match.player1Id);
                command.Parameters.AddWithValue("@player1Name", match.player1Name);
                command.Parameters.AddWithValue("@player1Rating", match.player1Rating);
                command.Parameters.AddWithValue("@player1Icon", match.player1Icon);

                command.Parameters.AddWithValue("@player2Id", match.player2Id);
                command.Parameters.AddWithValue("@player2Name", match.player2Name);
                command.Parameters.AddWithValue("@player2Rating", match.player2Rating);
                command.Parameters.AddWithValue("@player2Icon", match.player2Icon);

                command.Parameters.AddWithValue("@winnerId", match.winnerId);
                command.Parameters.AddWithValue("@matchDate", match.matchDate);

                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        Debug.Log("✅ 매치 기록 SQLite에 저장 완료");
    }


    // 🔹 플레이어 아이템 저장
    public void SavePlayerItem(PlayerItemData item)
    {


        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                INSERT OR REPLACE INTO playerItems 
                (itemId, playerId, itemUniqueId, itemType, price, isUnlocked, acquiredAt) 
                VALUES (@itemId, @playerId, @itemUniqueId, @itemType, @price, @isUnlocked, @acquiredAt);
            ";

                command.Parameters.AddWithValue("@itemId", item.itemId);
                command.Parameters.AddWithValue("@playerId", item.playerId);
                command.Parameters.AddWithValue("@itemUniqueId", item.itemUniqueId);
                command.Parameters.AddWithValue("@itemType", item.itemType);
                command.Parameters.AddWithValue("@price", item.price);
                command.Parameters.AddWithValue("@isUnlocked", item.isUnlocked ? 1 : 0);
                command.Parameters.AddWithValue("@acquiredAt", item.acquiredAt);

                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        //Debug.Log("✅ 플레이어 아이템 SQLite에 저장 완료");
    }


    // 🔹 랭킹 데이터 저장
    public void SaveRankingData(PlayerRankingData ranking)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                INSERT OR REPLACE INTO rankings (playerId, playerName, rating, rankPosition, profileIcon)
                VALUES (@playerId, @playerName, @rating, @rankPosition, @profileIcon);";

                command.Parameters.AddWithValue("@playerId", ranking.playerId);
                command.Parameters.AddWithValue("@playerName", ranking.playerName);
                command.Parameters.AddWithValue("@rating", ranking.rating);
                command.Parameters.AddWithValue("@rankPosition", ranking.rankPosition);
                command.Parameters.AddWithValue("@profileIcon", ranking.profileIcon);

                command.ExecuteNonQuery();
            }
        }
    }
    // 내 랭킹 데이터 저장
    public void SaveMyRankingData(PlayerRankingData myRanking)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                INSERT OR REPLACE INTO myRanking 
                (playerId, playerName, rating, rankPosition, profileIcon)
                VALUES (@playerId, @playerName, @rating, @rankPosition, @profileIcon);
            ";

                command.Parameters.AddWithValue("@playerId", myRanking.playerId);
                command.Parameters.AddWithValue("@playerName", myRanking.playerName);
                command.Parameters.AddWithValue("@rating", myRanking.rating);
                command.Parameters.AddWithValue("@rankPosition", myRanking.rankPosition);
                command.Parameters.AddWithValue("@profileIcon", myRanking.profileIcon);


                command.ExecuteNonQuery();
            }
        }
        Debug.Log("✅ 내 랭킹 데이터 SQLite에 저장 완료!");
    }

    // ===================== 🟢 데이터 로드 함수들 ===================== //

    // 🔹 1️⃣ 플레이어 데이터 불러오기
    public PlayerData LoadPlayerData()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            string query = "SELECT * FROM players Limit 1";//WHERE deviceId = @deviceId";//추가 가능

            using (var command = new SqliteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        PlayerData loadedPlayer = new PlayerData(
                            reader.GetInt32(0),  // playerId
                            reader.GetString(1), // deviceId
                            reader.GetString(2), // googleId
                            reader.GetString(3), // playerName
                            reader.GetString(4), // profileIcon
                            reader.GetString(5), // boardImage
                            reader.GetInt32(6),  // rating
                            reader.GetInt32(7),  // currency
                            reader.GetString(8)  // createdAt
                        );
                        //Debug.Log($"✅ 불러온 플레이어 데이터: {loadedPlayer.ToString()}");
                        return loadedPlayer;
                    }
                }
            }
        }
        Debug.Log("❌ SQLite에 플레이어 데이터 없음");
        return null; // 플레이어 데이터 없음
    }


    // 🔹 2️⃣ 플레이어 스탯 불러오기 (SQLite)
    public PlayerStatsData LoadPlayerStats()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            string query = "SELECT * FROM playerStats WHERE playerId = @playerId";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@playerId", player.playerId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int totalGames = reader.GetInt32(1);
                        int wins = reader.GetInt32(2);
                        int losses = reader.GetInt32(3);
                        double winRate = reader.GetDouble(4); // REAL 값 가져오기

                        //Debug.Log($"✅ 플레이어 스탯 로드 성공: playerId={player.playerId}, totalGames={totalGames}, wins={wins}, losses={losses}, winRate={winRate}");

                        return new PlayerStatsData(player.playerId, totalGames, wins, losses, (float)winRate);
                    }
                }
            }
        }
        Debug.LogWarning($"⚠️ playerStats 테이블에서 playerId={player.playerId} 데이터를 찾을 수 없음!");
        return null;
    }

    // 🔹 3️⃣ 로그인 기록 불러오기
    public LoginData LoadLoginData()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            string query = "SELECT * FROM loginRecords ORDER BY loginTime DESC LIMIT 1";
            using (var command = new SqliteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int loginId = reader.GetInt32(0);
                        int playerId = reader.GetInt32(1);
                        // 🔹 loginTime이 NULL일 가능성이 있으므로 체크
                        string loginTime = reader.IsDBNull(2) ? "" : reader.GetString(2);

                        // 🔹 ipAddress가 NULL일 가능성이 있으므로 체크
                        string ipAddress = reader.IsDBNull(3) ? "0.0.0.0" : reader.GetString(3);

                        return new LoginData(
                            loginId,  // loginId
                            playerId,  // playerId
                            loginTime, // loginTime
                            ipAddress  // ipAddress
                        );
                    }
                }
            }
        }
        return null; // 로그인 기록 없음
    }

    // 🔹 4️⃣ 매치 기록 불러오기 (리스트 반환)
    public List<MatchHistoryData> LoadMatchHistory()
    {
        List<MatchHistoryData> matchList = new List<MatchHistoryData>();
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            string query = @"
            SELECT matchId, player1Id, player1Name, player1Rating, player1Icon, 
                   player2Id, player2Name, player2Rating, player2Icon, 
                   winnerId, strftime('%Y-%m-%d %H:%M:%S', matchDate) as matchDate 
            FROM matchRecords ORDER BY matchDate DESC
            LIMIT 10;";

            using (var command = new SqliteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        matchList.Add(new MatchHistoryData(
                            reader.GetInt32(0),  // matchId
                            reader.GetInt32(1),  // player1Id
                            reader.GetString(2), // player1Name
                            reader.GetInt32(3),  // player1Rating
                            reader.GetString(4), // player1Icon

                            reader.GetInt32(5),  // player2Id
                            reader.GetString(6), // player2Name
                            reader.GetInt32(7),  // player2Rating
                            reader.GetString(8), // player2Icon

                            reader.GetInt32(9),  // winnerId
                            reader.GetValue(10).ToString()  // matchDate
                        ));
                    }
                }
            }
        }
        return matchList;
    }

    // 🔹 플레이어 아이템 불러오기 (리스트 반환)
    public List<PlayerItemData> LoadPlayerItems()
    {
        List<PlayerItemData> itemList = new List<PlayerItemData>();
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            string query = "SELECT * FROM playerItems";
            using (var command = new SqliteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemList.Add(new PlayerItemData(
                            reader.GetInt32(0),  // itemId
                            reader.GetInt32(1),  // playerId
                            reader.GetInt32(2),  // itemUniqueId
                            reader.GetString(3), // itemType
                            reader.GetInt32(4),  // 
                            reader.GetInt32(5) == 1, // isUnlocked
                            reader.GetString(6)  // acquiredAt
                        ));
                    }
                }
            }
        }
        return itemList;
    }


    // 🔹 랭킹 데이터 불러오기
    public List<PlayerRankingData> LoadRankings()
    {
        List<PlayerRankingData> rankings = new List<PlayerRankingData>();

        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            string query = "SELECT * FROM rankings ORDER BY rankPosition ASC";
            using (var command = new SqliteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rankings.Add(new PlayerRankingData(
                            reader.GetInt32(0), // playerId
                            reader.GetString(1), // playerName
                            reader.GetInt32(2),  // rating
                            reader.GetInt32(3),  // rankPosition
                            !reader.IsDBNull(4) ? reader.GetString(4) : "101" // 🔹 NULL 체크 후 기본값 설정
                        ));
                    }
                }
            }
        }
        return rankings;
    }
    // 내 랭킹 데이터 불러오기
    public PlayerRankingData LoadMyRankingData()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM myRanking WHERE playerId = @playerId";
                command.Parameters.AddWithValue("@playerId", player.playerId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        PlayerRankingData myRanking = new PlayerRankingData(
                            reader.GetInt32(0),  // playerId
                            reader.GetString(1), // playerName
                            reader.GetInt32(2),  // rating
                            reader.GetInt32(3),   // rankPosition
                            !reader.IsDBNull(4) ? reader.GetString(4) : "101" // 🔹 NULL 체크 후 기본값 설정
                        );

                        Debug.Log($"✅ [SQLite] 내 랭킹 데이터 로드 성공: {myRanking.playerName} (Rank: {myRanking.rankPosition})");
                        return myRanking;
                    }
                }
            }
        }

        Debug.LogWarning("⚠️ [SQLite] 내 랭킹 데이터 없음!");
        return null; // 저장된 내 랭킹 데이터가 없는 경우
    }

}
