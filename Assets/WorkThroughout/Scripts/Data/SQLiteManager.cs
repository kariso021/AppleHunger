using SQLite;
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

    // =========================== 나중에 꼭 지워야 한다 ===============================/
    public bool isDummy = false;
    // =================================================================================
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
    public PlayerSessionData playerSession;
    // 데이터로드가 끝나면 실행될 이벤트
    public event Action OnSQLiteDataLoaded;

    //
    public bool isSqlExist = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임이 진행하는 동안엔 삭제가 일어나면 안되므로

            player.deviceId = TransDataClass.deviceIdToApply;
            player.googleId = TransDataClass.googleIdToApply;

            if (player.googleId != null)
                StartCoroutine(ClientNetworkManager.Instance.UpdatePlayerGoogleId(player.deviceId, player.googleId));

            StartCoroutine(InitializeDatabase());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            createTables(connection); // 어차피 쿼리 차원에서 중복 생성 막아둠.
        }

    }
    private IEnumerator InitializeDatabase()
    {
        // =========================== 나중에 꼭 지워야 한다 ===============================/
        string rawDbPath = !isDummy ? Path.Combine(Application.persistentDataPath, dbName).Replace("\\", "/") : Path.Combine(Application.persistentDataPath, "game_data_dummy.db").Replace("\\", "/");
        // =================================================================================
        dbPath = rawDbPath;  // SQLite 연결을 위해 여전히 사용
        Debug.Log($"[SQL] SQLite DB 경로: {dbPath}");

        // Step 1: SQLite DB가 존재하는지 확인
        if (File.Exists(rawDbPath))
        {
            Debug.Log("[SQL] SQLite DB가 이미 존재합니다. 서버 요청 없이 로컬 DB 사용.");
            isSqlExist = true;
            // 여기서 재화를 서버에서 받아오는 부분이 추가되어야 할 것 같음. 재화같은 경우엔 이벤트 등으로 넣어주는게 되니까
            // Step 2: DB가 존재하면 서버에서 데이터를 받을 필요 없이 로드 후 종료
            yield return loadAllDataAwait();

            saveRankDataToDictionary();

            // yield return DataSyncManager.Instance.PlayerRankingUpdated();

            ClientNetworkManager.Instance.GetPlayerItems(player.playerId);
            yield break;
        }

        yield return StartCoroutine(CreateDatabaseAndFetchPlayerData());
        LoadAllData();

        //dbPath = "URI=file:" + Path.Combine(Application.persistentDataPath, dbName);
    }
    private IEnumerator CreateDatabaseAndFetchPlayerData()
    {
        yield return StartCoroutine(CreateDatabase()); // ✅ SQLite DB 생성
    }
    private void createTables(SQLiteConnection connection)
    {
        // 🔹 플레이어 테이블 (Auto-Increment 제거)
        connection.Execute(@"
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
        connection.Execute(@"
                CREATE TABLE IF NOT EXISTS playerStats (
                    playerId INTEGER PRIMARY KEY,  
                    totalGames INTEGER DEFAULT 0,
                    wins INTEGER DEFAULT 0,
                    losses INTEGER DEFAULT 0,
                    winRate REAL GENERATED ALWAYS AS (CASE WHEN totalGames = 0 THEN 0 ELSE (wins * 100.0 / totalGames) END) STORED,
                    FOREIGN KEY (playerId) REFERENCES players(playerId) ON DELETE CASCADE
                );");

        // 🔹 플레이어 아이템 테이블
        connection.Execute(@"
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
        connection.Execute(@"
                CREATE TABLE IF NOT EXISTS loginRecords (
                    loginId INTEGER PRIMARY KEY,  
                    playerId INTEGER NOT NULL,
                    loginTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    ipAddress TEXT DEFAULT NULL,
                    FOREIGN KEY (playerId) REFERENCES players(playerId) ON DELETE CASCADE
                );");

        // 🔹 매치 기록 테이블
        connection.Execute(@"
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
        connection.Execute(@"
                CREATE TABLE IF NOT EXISTS rankings (           
                    playerId INTEGER NOT NULL,         -- 플레이어 ID
                    playerName TEXT NOT NULL,          -- 플레이어 닉네임
                    rating INTEGER NOT NULL,           -- 레이팅 점수
                    rankPosition INTEGER PRIMARY KEY,  -- 랭킹 순위 (1~50)
                    profileIcon TEXT DEFAULT NULL,     -- 플레이어 프로필 아이콘 (마지막 컬럼)
                    FOREIGN KEY (playerId) REFERENCES players(playerId) ON DELETE CASCADE
                );");

        // ✅ 개별 플레이어 랭킹 테이블 생성
        connection.Execute(@"
                CREATE TABLE IF NOT EXISTS myRanking (
                    playerId INTEGER PRIMARY KEY,
                    playerName TEXT NOT NULL,
                    rating INTEGER NOT NULL,
                    rankPosition INTEGER NOT NULL,
                    profileIcon TEXT DEFAULT NULL  -- 프로필 아이콘 (마지막 컬럼)
                );
                ");

        //  플레이어 게임 티켓 테이블 생성
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS PlayerSession (
                playerId     INTEGER PRIMARY KEY,
                ticketId     TEXT DEFAULT '0',
                serverIp     TEXT DEFAULT '0',
                serverPort   INTEGER DEFAULT 0,
                isInGame     INTEGER DEFAULT 0,
                isConnected  INTEGER DEFAULT 0,
                timestamp    TEXT DEFAULT (datetime('now', '+9 hours')),
                FOREIGN KEY (playerId) REFERENCES players(playerId) ON DELETE CASCADE
            );
        ");

        Debug.Log("[SQL] SQLite 데이터베이스 초기화 완료");

        connection.Execute(@"
                DELETE FROM matchRecords 
                WHERE matchId NOT IN (
                    SELECT matchId FROM matchRecords 
                    ORDER BY matchDate DESC 
                    LIMIT 10
                );
            ");

        Debug.Log("[SQL] matchRecords 10개 유지 완료");
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
                Debug.Log("[SQL]SQLite DB 파일 복사 완료! (PC, iOS)");
                yield break;
            }
            else
            {
                Debug.LogError("[SQL] StreamingAssets 폴더에 DB 파일이 존재하지 않음!");
            }
        }
        else // 📌 Step 3: `streamingAssetsPath`에서 다운로드 (Android)
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, dbName);
            Debug.Log("[SQL] StreamingAssets SQLite 경로: " + sourcePath);
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityWebRequest request = UnityWebRequest.Get(sourcePath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                File.WriteAllBytes(persistentDbPath, request.downloadHandler.data);
            } else {
                Debug.LogError("❌ Android에서 DB 다운로드 실패: " + request.error);
            }
#else
            File.Copy(sourcePath, persistentDbPath, true);
            Debug.Log("[SQL] PC/iOS에서 SQLite DB 복사 완료!");
#endif

            //using (UnityWebRequest request = UnityWebRequest.Get(streamingDbPath))
            //{
            //    yield return request.SendWebRequest();

            //    if (request.result == UnityWebRequest.Result.Success)
            //    {
            //        File.WriteAllBytes(persistentDbPath, request.downloadHandler.data);
            //        Debug.Log("✅ Android에서 SQLite DB 복사 완료!");
            //        yield break;
            //    }
            //    else
            //    {
            //        Debug.LogError("❌ Android에서 DB 다운로드 실패: " + request.error);
            //    }
            //}
        }

        // 📌 Step 4: `persistentDataPath`에도 DB가 없으면 새로 생성
        if (!File.Exists(persistentDbPath))
        {
            Debug.LogWarning("[SQL] DB 파일이 어디에도 없음 → 새로 생성합니다.");
            yield return StartCoroutine(CreateNewDatabase(persistentDbPath));
        }
    }


    private IEnumerator CreateNewDatabase(string dbPath)
    {
        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                Debug.Log("[SQL] 새 SQLite 데이터베이스 생성 완료!");
                createTables(connection);
            }
            Debug.Log("[SQL] 새 SQLite 데이터베이스 테이블 생성 완료!");
        }
        catch (Exception e)
        {
            Debug.LogError("[SQL] 새 DB 생성 실패: " + e.Message);
        }

        // ✅ 서버에서 플레이어 데이터 가져오기 (먼저 실행해야 함)
        if (ClientNetworkManager.Instance != null)
        {
            Debug.Log("[SQL] 서버에서 플레이어 데이터 요청 중...");

            yield return ClientNetworkManager.Instance.GetPlayerData(
                "deviceId", SystemInfo.deviceUniqueIdentifier, true);
            // googleId가 존재한다면 → 서버에 업데이트 요청
            if (!string.IsNullOrEmpty(TransDataClass.googleIdToApply))
            {
                SQLiteManager.Instance.player.googleId = TransDataClass.googleIdToApply;
                yield return ClientNetworkManager.Instance.UpdatePlayerData();
            }

            // ✅ 먼저 플레이어 데이터를 받아옴
            //yield return ClientNetworkManager.Instance.GetPlayerData(player.googleId == null ? "deviceId" : "googleId", player.googleId == null ? SystemInfo.deviceUniqueIdentifier : player.googleId, true);


            // ✅ 플레이어 ID가 `0`이 아닐 때까지 기다림
            yield return new WaitUntil(() => SQLiteManager.Instance.LoadPlayerData().playerId != 0);
            // ✅ player.playerId가 설정된 후에 나머지 요청을 병렬 실행
            Debug.Log($"[SQL] 플레이어 ID 확인: {SQLiteManager.Instance.LoadPlayerData().playerId}");

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


            DataSyncManager.Instance.InvokeUIRankingUpdateEvent();
            Debug.Log("[SQL] [Client] 모든 데이터 동기화 완료!");
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
    private void ExecuteQuery(SQLiteConnection connection, string query)
    {
        connection.Execute(query);
    }

    // 🔹 모든 데이터 불러오기 (게임 시작 시 실행)
    public void LoadAllData()
    {
        Debug.Log("[SQL] 모든 데이터를 SQLite에서 불러옵니다.");
        player = LoadPlayerData();
        stats = LoadPlayerStats();
        login = LoadLoginData();
        matches = LoadMatchHistory();
        items = LoadPlayerItems();
        rankings = LoadRankings();
        myRankingData = LoadMyRankingData();

        Debug.Log($"[SQL]플레이어 데이터 불러오기 완료: {player?.playerName ?? "없음"}");
        if (stats != null) Debug.Log($"[SQL] 플레이어 스탯 불러오기 완료: id = {stats.playerId} , total = {stats.totalGames} , wins = {stats.wins}");
        Debug.Log($"[SQL] 로그인 데이터 불러오기 완료: {login?.loginTime ?? "없음"}");
        Debug.Log($"[SQL] 매치 기록 개수: {matches.Count}");
        Debug.Log($"[SQL] 보유 아이템 개수: {items.Count}");
        Debug.Log($"[SQL] 랭킹 인원 수 : {rankings.Count}");

        // 모든 데이터가 로드되면 그 떄 UI 업데이트를 실행
        // 
        OnSQLiteDataLoaded?.Invoke();
    }

    private IEnumerator loadAllDataAwait()
    {
        LoadAllData();

        if (SQLiteManager.Instance.player == null || SQLiteManager.Instance.player.playerId == 0)
        {
            Debug.Log("[SQL] 플레이어 데이터가 비어있음...재요청");
            yield return ClientNetworkManager.Instance.GetPlayerData(
                player.googleId == null ? "deviceId" : "googleId",
                player.googleId == null ? SystemInfo.deviceUniqueIdentifier : player.googleId,
                false);
            LoadAllData();
        }
        yield return null;
    }
    private void saveRankDataToDictionary()
    {
        foreach (var rankData in rankings)
        {
            if (rankDictionary.ContainsKey(rankData.playerId))
            {
                Debug.LogWarning($"[SQL] 중복된 playerId 발견: {rankData.playerId}");
                continue;
            }

            rankDictionary.Add(rankData.playerId, rankData);
        }
    }
    public void SavePlayerData(PlayerData player)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            INSERT OR REPLACE INTO players 
            (playerId, deviceId, googleId, playerName, profileIcon, boardImage, rating, currency, createdAt) 
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);",
                player.playerId,
                player.deviceId,
                player.googleId,
                player.playerName,
                player.profileIcon,
                player.boardImage,
                player.rating,
                player.currency,
                player.createdAt
            );

            int rowsAffected = command.ExecuteNonQuery();
            Debug.Log($"[SQL] 저장 완료: {rowsAffected}행 변경됨");
        }
    }
    public void SavePlayerCurrency(int currency)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            UPDATE players 
            SET currency = ? 
            WHERE playerId = ?;",
                currency,
                SQLiteManager.Instance.player.playerId
            );

            int rowsAffected = command.ExecuteNonQuery();
            Debug.Log($"[SQL] Currency 업데이트 완료: {rowsAffected}행 변경됨");
        }
    }
    // 🔹 플레이어 스탯 저장
    public void SavePlayerStats(PlayerStatsData stats)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            INSERT INTO playerStats (playerId, totalGames, wins, losses)
            VALUES (?, ?, ?, ?)
            ON CONFLICT(playerId) DO UPDATE SET
            totalGames = excluded.totalGames,
            wins = excluded.wins,
            losses = excluded.losses;
        ",
            stats.playerId,
            stats.totalGames,
            stats.wins,
            stats.losses);

            int rows = command.ExecuteNonQuery();
            Debug.Log($"[SQL] 플레이어 스탯 저장 완료: {rows}행 변경됨");
        }
    }

    // 🔹 로그인 데이터 저장
    public void SaveLoginData(LoginData login)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            INSERT OR REPLACE INTO loginRecords 
            (loginId, playerId, loginTime, ipAddress) 
            VALUES (?, ?, ?, ?);",
                login.loginId,
                login.playerId,
                login.loginTime,
                login.ipAddress);

            command.ExecuteNonQuery();
        }
        Debug.Log("✅ 로그인 데이터 저장 완료");
    }

    // 🔹 매치 기록 저장
    public void SaveMatchHistory(MatchHistoryData match)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            INSERT OR REPLACE INTO matchRecords 
            (matchId, player1Id, player1Name, player1Rating, player1Icon,
             player2Id, player2Name, player2Rating, player2Icon, 
             winnerId, matchDate) 
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);",
                match.matchId,
                match.player1Id,
                match.player1Name,
                match.player1Rating,
                match.player1Icon,
                match.player2Id,
                match.player2Name,
                match.player2Rating,
                match.player2Icon,
                match.winnerId,
                match.matchDate);

            command.ExecuteNonQuery();
        }
        Debug.Log("✅ 매치 기록 저장 완료");
    }
    // 🔹 플레이어 아이템 저장
    public void SavePlayerItem(PlayerItemData item)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            INSERT OR REPLACE INTO playerItems 
            (itemId, playerId, itemUniqueId, itemType, price, isUnlocked, acquiredAt) 
            VALUES (?, ?, ?, ?, ?, ?, ?);",
                item.itemId,
                item.playerId,
                item.itemUniqueId,
                item.itemType,
                item.price,
                item.isUnlocked ? 1 : 0,
                item.acquiredAt);

            command.ExecuteNonQuery();
        }
    }

    // 🔹 랭킹 데이터 저장
    public void SaveRankingData(PlayerRankingData ranking)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            INSERT OR REPLACE INTO rankings 
            (playerId, playerName, rating, rankPosition, profileIcon)
            VALUES (?, ?, ?, ?, ?);",
                ranking.playerId,
                ranking.playerName,
                ranking.rating,
                ranking.rankPosition,
                ranking.profileIcon);

            command.ExecuteNonQuery();
        }
    }

    // 내 랭킹 데이터 저장
    public void SaveMyRankingData(PlayerRankingData myRanking)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            INSERT OR REPLACE INTO myRanking 
            (playerId, playerName, rating, rankPosition, profileIcon)
            VALUES (?, ?, ?, ?, ?);",
                myRanking.playerId,
                myRanking.playerName,
                myRanking.rating,
                myRanking.rankPosition,
                myRanking.profileIcon);

            command.ExecuteNonQuery();
        }
        Debug.Log("✅ 내 랭킹 데이터 저장 완료");
    }

    public void SavePlayerSession(PlayerSessionData playerSession)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            INSERT OR REPLACE INTO playerSession
            (playerId, ticketId, serverIp, serverPort, isInGame, isConnected)
            VALUES (?, ?, ?, ?, ?, ?);",
                playerSession.playerId,
                playerSession.ticketId,
                playerSession.serverIp,
                playerSession.serverPort,
                playerSession.isInGame ? 1 : 0, // bool → int 변환
                playerSession.isConnected ? 1 : 0
            );


            int rowsAffected = command.ExecuteNonQuery();
            Debug.Log($"[SQL] 저장 완료: {rowsAffected}행 변경됨");
        }
    }
    // ===================== 🟢 데이터 로드 함수들 ===================== //

    // 🔹 1️⃣ 플레이어 데이터 불러오기
    public PlayerData LoadPlayerData()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var result = connection.Query<PlayerData>("SELECT * FROM players LIMIT 1");
            if (result.Count > 0)
            {
                return result[0];
            }
        }
        Debug.Log("❌ SQLite에 플레이어 데이터 없음");
        return null;
    }
    // 🔹 2️⃣ 플레이어 스탯 불러오기 (SQLite)
    public PlayerStatsData LoadPlayerStats()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var result = connection.Query<PlayerStatsData>(
                "SELECT * FROM playerStats WHERE playerId = ?", player.playerId);
            if (result.Count > 0)
            {
                return result[0];
            }
        }
        Debug.LogWarning($"playerStats 테이블에서 playerId={player.playerId} 데이터를 찾을 수 없음!");
        return null;
    }
    // 🔹 3️⃣ 로그인 기록 불러오기
    public LoginData LoadLoginData()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var result = connection.Query<LoginData>(
                "SELECT * FROM loginRecords ORDER BY loginTime DESC LIMIT 1");
            if (result.Count > 0)
            {
                return result[0];
            }
        }
        return null;
    }

    // 🔹 4️⃣ 매치 기록 불러오기 (리스트 반환)
    public List<MatchHistoryData> LoadMatchHistory()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            return connection.Query<MatchHistoryData>(
                @"SELECT matchId, player1Id, player1Name, player1Rating, player1Icon, 
                     player2Id, player2Name, player2Rating, player2Icon, 
                     winnerId, strftime('%Y-%m-%d %H:%M:%S', matchDate) as matchDate 
              FROM matchRecords 
              ORDER BY matchDate DESC 
              LIMIT 10;");
        }
    }
    // 🔹 플레이어 아이템 불러오기 (리스트 반환)
    public List<PlayerItemData> LoadPlayerItems()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            return connection.Query<PlayerItemData>("SELECT * FROM playerItems");
        }
    }
    public List<PlayerRankingData> LoadRankings()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand("SELECT * FROM rankings ORDER BY rankPosition ASC");
            return new List<PlayerRankingData>(command.ExecuteDeferredQuery<PlayerRankingData>());
        }
    }
    public PlayerRankingData LoadMyRankingData()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(
                "SELECT * FROM myRanking WHERE playerId = ?", player.playerId);

            var result = new List<PlayerRankingData>(command.ExecuteDeferredQuery<PlayerRankingData>());

            if (result.Count > 0)
            {
                Debug.Log($" [SQLite] 내 랭킹 데이터 로드 성공: {result[0].playerName} (Rank: {result[0].rankPosition})");
                return result[0];
            }
        }

        Debug.LogWarning(" [SQLite] 내 랭킹 데이터 없음!");
        return null;
    }

    public PlayerSessionData LoadPlayerSession()
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            SELECT playerId, ticketId, serverIp, serverPort, isInGame, timestamp
            FROM playerSession
            WHERE playerId = ?;", player.playerId);

            var result = command.ExecuteQuery<PlayerSessionData>();
            if (result.Count > 0)
            {
                Debug.Log($" [SQLite] PlayerSession 불러오기 완료 - playerId: {player.playerId}");
                return result[0];
            }
            else
            {
                Debug.LogWarning($" [SQLite]  PlayerSession 없음 - playerId: {player.playerId}");
                return null;
            }
        }
    }


    // <======================== 데이터 초기화 함수 =========================>
    public void ResetPlayerSession(int playerId)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            var command = connection.CreateCommand(@"
            UPDATE playerSession
            SET ticketId = '0',
                serverIp = '0',
                serverPort = 0,
                isInGame = 0,
                isConnected = 0,
                timestamp = datetime('now', '+9 hours')
            WHERE playerId = ?;",
                playerId);

            command.ExecuteNonQuery();
            Debug.Log($"[SQL] PlayerSession 초기화 완료 - playerId: {playerId}");
        }
    }

}
