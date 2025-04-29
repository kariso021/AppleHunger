using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class ServerToAPIManager : MonoBehaviour
{
    private string apiBaseUrl = "https://applehunger.site";

    private static ServerToAPIManager instance;
    public static ServerToAPIManager Instance => instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임이 진행하는 동안엔 삭제가 일어나면 안되므로
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #region Players Data Region

    /// <summary>
    /// 게임 최초 실행시 유저 데이터가 없다면 실행
    /// </summary>
    /// <returns></returns>
    public IEnumerator AddPlayer()
    {
        string url = $"{apiBaseUrl}/players";

        PlayerData newPlayer = new PlayerData(SQLiteManager.Instance.player.deviceId,
            SQLiteManager.Instance.player.googleId, $"User_{UnityEngine.Random.Range(0, 9999)}",
            "101",
            "201",
            1200, 500);
        string jsonData = JsonConvert.SerializeObject(newPlayer);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string playerJsonData = request.downloadHandler.text;
                PlayerAddResponse response = JsonConvert.DeserializeObject<PlayerAddResponse>(playerJsonData);

                // 클라이언트에 Players 정보 저장
                yield return TargetReceivePlayerDataClientRpc(playerJsonData);

                Debug.Log($"[ServerToAPI] Complete Player Add! ,Added Player Id: {response.playerId}");
            }
            else
                Debug.LogError("[ServerToAPI] Failed Player Add: " + request.error);
        }
    }

    private IEnumerator TargetReceivePlayerDataClientRpc(string jsonData)
    {
        yield return ClientNetworkManager.Instance.TargetReceivePlayerDataClientRpc(jsonData);
    }
    private IEnumerator TargetReceiveAsPlayerDataClientRpc(string jsonData)
    {
        yield return ClientNetworkManager.Instance.TargetReceiveAsPlayerDataClientRpc(jsonData);
    }
    public IEnumerator DeletePlayer(int playerId)
    {
        string url = $"{apiBaseUrl}/players/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("[ServerToAPI Complete Player Delete");
            else
                Debug.LogError("[ServerToAPI] Failed Player Delete: " + request.error);
        }
    }

    // 플레이어 정보 수정 , 클라이언트에 저장된 데이터를 그대로 json으로 api서버에 넘김
    public IEnumerator UpdatePlayerData(PlayerData updatedData)
    {
        string url = $"{apiBaseUrl}/players/updatePlayer/{updatedData.playerId}";

        string jsonData = JsonConvert.SerializeObject(updatedData);
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT")) // PUT 사용
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[ServerToAPI] Complete Player Info Update");
                // 🔹 데이터가 변경되었음을 알림 (자동 동기화)
                DataSyncManager.Instance.PlayerDataUpdated();
            }

            else
                Debug.LogError("[ServerToAPI] Failed Player Info Update: " + request.error);
        }
    }


    // 플레이어 정보 가져오기(By playerId), id는 나중에 googleId,guestId를 db에 추가해서 그걸로
    // 사용할 예정
    /// <summary>
    /// player의 google 혹은 device Id를 이용해 정보를 조회하는 함수. 만약 정보가 없다면 플레이어가 새로운 계정인 것으로 간주하여 새 플레이어 생성
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="idValue"></param>
    public IEnumerator GetPlayer(string idType, string idValue, bool isFirstTime) // 
    {
        string url = $"{apiBaseUrl}/players/search?{idType}={idValue}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                Debug.LogWarning($"Target : {jsonData}");
                yield return TargetReceiveAsPlayerDataClientRpc(jsonData);
            }
            else
            {
                Debug.LogError("[ServerToAPI] Failed Player Search : " + request.error);
                Debug.LogError("[ServerToAPI] Response: " + request.downloadHandler.text);
                if (isFirstTime)
                {
                    yield return StartCoroutine(AddPlayer());
                    Debug.Log("[ServerToAPI] ADD NEW PLAYER END");
                }
            }
        }
    }

    public IEnumerator UpdateNicknameOnServer(string playerName)
    {
        string url = $"{apiBaseUrl}/players/updateNickname";
        int playerId = SQLiteManager.Instance.player.playerId;
        string jsonData = JsonConvert.SerializeObject(new NicknameUpdateRequest(playerId, playerName));

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[ServerToAPI] Complete Nickname Change");
            DataSyncManager.Instance.PlayerDataUpdated();

            //// 팝업 닫기
            //PopupManager popupManager = FindAnyObjectByType<PopupManager>();
            //popupManager?.ClosePopup();

            //yield return new WaitForSeconds(1f);

            //SQLiteManager.Instance.LoadAllData();
        }
        else
        {
            Debug.LogError($"[ServerToAPI] Failed Nickname Change : {request.error}");
        }
    }

    public IEnumerator CheckNicknameDuplicate(string nickname, Action<bool> callback)
    {
        string url = $"{apiBaseUrl}/players/checkNickname?playerName={UnityWebRequest.EscapeURL(nickname)}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<NicknameDuplicateResponse>(request.downloadHandler.text);
                callback?.Invoke(response.isDuplicate);
            }
            else
            {
                Debug.LogError($"[ServerToAPI] Failed Nickname Duplicate Check: {request.error}");
                callback?.Invoke(false); // 실패 시 기본값 false
            }
        }
    }

    public IEnumerator GetCurrency()
    {
        int playerId = SQLiteManager.Instance.player.playerId;
        string url = $"{apiBaseUrl}/players/getCurrency/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = request.downloadHandler.text;
                    JObject obj = JObject.Parse(json);
                    int currency = obj["currency"].Value<int>();
                    SQLiteManager.Instance.SavePlayerCurrency(currency);

                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ServerToAPI] Failed Currency Json Parsing: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[ServerToAPI] Failed Currency Communicate with server: {request.error}");
            }
        }
    }

    public IEnumerator UpdatePlayerGoogleId(int playerId, string googleId) 
    {
        string url = $"{apiBaseUrl}/players/updateGoogleId";
        string json = JsonConvert.SerializeObject(new GoogleIdUpdateRequest(playerId,googleId));
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[ServerToAPI] Failed Google Id Update: {request.error}");
        }
        else
        {
            Debug.Log($"[ServerToAPI] Complete Google Id Update: {request.downloadHandler.text}");
            // 이미 SQLiteManager 부분에서 player.googleId 에 값을 넣어둔 상태라 저장만 하면 됨.
            SQLiteManager.Instance.SavePlayerData(SQLiteManager.Instance.player);
        }
    }
    #endregion

    #region Player MatchRecords Region
    public IEnumerator AddMatchResult(int winnerId, int loserId)
    {
        string url = $"{apiBaseUrl}/matchrecords";

        // JSON 데이터 생성
        string jsonData = $"{{\"winnerId\":{winnerId},\"loserId\":{loserId}}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ServerToAPI] Complete Match Result Update - Winner: {winnerId}, Loser: {loserId}");

                // 🔹 자동 동기화 트리거
                DataSyncManager.Instance.MatchHistoryUpdated(); // 매치 기록 업데이트
                DataSyncManager.Instance.PlayerStatsUpdated();  // 플레이어 스탯 업데이트
                //DataSyncManager.Instance.PlayerRankingUpdated(); // 랭킹 업데이트 (승패 반영) , 랭킹은 굳이 실시간으로 체크해줄 필요가 없음. 서버에서 일정 시간마다 최신화를 해주는게 더 효율적
            }
            else
                Debug.LogError($"[ServerToAPI] Failed to save match result: {request.error}");
        }
    }
    public IEnumerator GetMatchResult(int playerId)
    {
        string url = $"{apiBaseUrl}/matchRecords/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text; // Matchrecords 테이블에서 playerId가 동일한 컬럼들만 추려서 json형태로 list를 만들어 가져온다는 느낌
                MatchHistoryResponse response = JsonConvert.DeserializeObject<MatchHistoryResponse>(jsonData);
                Debug.Log($"[ServerToAPI] Complete Match Records Retrieval - Total {response.matches.Length} matches");

                foreach (var match in response.matches)
                {
                    TargetReceiveMatchRecordsClientRpc(match);
                }
            }
            else
            {
                Debug.LogError($"[ServerToAPI] Failed to retrieve match records: {request.error}");
            }
        }
    }
    public void TargetReceiveMatchRecordsClientRpc(MatchHistoryData matchHistoryData)
    {
        ClientNetworkManager.Instance.TargetReceiveMatchRecordsClientRpc(matchHistoryData);
    }



    #endregion

    #region Player Stat Region

    // 플레이어 스탯(매치,승리,패배 수) 조회 API
    public IEnumerator GetPlayerStat(int playerId)
    {
        string url = $"{apiBaseUrl}/playerStats/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ServerToAPI] Complete Player Stats Retrieval");
                TargetReceivePlayerStatClientRpc(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ServerToAPI] Failed to retrieve player stats: {request.error}");
                Debug.LogError($"[ServerToAPI] Stats response: {request.downloadHandler.text}");
            }
        }
    }

    private void TargetReceivePlayerStatClientRpc(string jsonData)
    {
        ClientNetworkManager.Instance.TargetReceivePlayerStatsClientRpc(jsonData);
    }


    #endregion

    #region Player Item Region

    // 플레이어 아이템 정보 조회(프로필 정보에 들어갈 내용)
    public IEnumerator GetPlayerItems(int playerId)
    {
        string url = $"{apiBaseUrl}/playerItems/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                PlayerItemsResponse response = JsonConvert.DeserializeObject<PlayerItemsResponse>(jsonData);
                Debug.Log($"[ServerToAPI] Complete Player Items Retrieval");
                // 🔹 리스트 안에 여러 개의 아이템이 들어있으므로, 각각을 TargetReceivePlayerItems로 넘겨줌
                foreach (var playerItem in response.items)
                {
                    TargetReceivePlayerItemsClientRpc(JsonConvert.SerializeObject(playerItem));
                }

                //DataSyncManager.Instance.PlayerItemsUpdated(); // 아이템 상태 업데이트
            }
            else
                Debug.LogError($"[ServerToAPI] Failed to retrieve player items: {request.error}");
        }
    }

    // JSON 데이터 로드 후 변환
    private void TargetReceivePlayerItemsClientRpc(string jsonData)
    {
        ClientNetworkManager.Instance.TargetReceivePlayerItemsClientRpc(jsonData);
    }

    // 🔹 아이템 구매 요청
    public IEnumerator PurchaseItem(int playerId, int itemUniqueId)
    {
        string url = $"{apiBaseUrl}/playerItems/purchase";
        string jsonData = $"{{\"playerId\":{playerId}, \"itemUniqueId\":{itemUniqueId}}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ServerToAPI] Complete Item Purchase - playerId: {playerId}, itemUniqueId: {itemUniqueId}");
                // 🔹 자동 동기화 트리거
                DataSyncManager.Instance.PlayerDataUpdated();  // 재화(currency) 업데이트
                DataSyncManager.Instance.PlayerItemsUpdated(); // 아이템 상태 업데이트
            }
            else
            {
                Debug.LogError($"[ServerToAPI] Failed to purchase item: {request.error}");
            }
        }
    }
    #endregion

    #region Player Login Region

    // 로그인 정보 조회

    public IEnumerator GetLoginRecords(int playerId)
    {
        string url = $"{apiBaseUrl}/loginRecords/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ServerToAPI] Complete Login Records Retrieval");
                TargetReceiveLoginRecordsClientRpc(request.downloadHandler.text);
            }
            else
                Debug.LogError($"[ServerToAPI] Failed to retrieve login records: {request.error}");
        }
    }

    private void TargetReceiveLoginRecordsClientRpc(string jsonData)
    {
        ClientNetworkManager.Instance.TargetReceiveLoginDataClientRpc(jsonData);

        // 로그인 데이터를 여러개로 관리할 게 아니라 하나로 관리할 예정인데 이건 나중에 order같은걸 해서 빼던가 해야할거같음
        //List<LoginRecordData> loginRecords =
        //
        //.FromJson<LoginRecordList>(jsonData).records;

        //foreach (var record in loginRecords)
        //{
        //    Debug.Log($"📌 로그인 기록 - playerId: {record.playerId}, time: {record.loginTime}, IP: {record.ipAddress}");
        //}
    }

    // 로그인 정보 업데이트
    public IEnumerator UpdateLoginTime(int playerId, string ipAddress)
    {
        string url = $"{apiBaseUrl}/loginRecords";
        string jsonData = JsonConvert.SerializeObject(new LoginUpdateRequest(playerId, ipAddress));
        // JsonUtility는 명시적인 클래스 구조를 필요로 하기때문에 별도의 DTO(Data Transfer Object) 클래스 생성해서 넘겨줌
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ServerToAPI] Complete Login Time Update");

                // 🔹 자동 동기화 트리거
                DataSyncManager.Instance.PlayerDataUpdated(); // 로그인 정보 업데이트
            }
            else
                Debug.LogError($"[ServerToAPI] Failed to update login time: {request.error}");
        }
    }
    #endregion

    #region Player Ranking Data
    // 랭킹 정보 

    public IEnumerator GetTopRankingData()
    {
        string url = $"{apiBaseUrl}/rankings";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ServerToAPI] 상위 50명 랭킹 조회 성공: {request.downloadHandler.text}");
                TargetReceiveTopRankingDataClientRpc(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ServerToAPI] 상위 50명 랭킹 조회 실패: {request.error}");
            }
        }
    }
    private void TargetReceiveTopRankingDataClientRpc(string jsonData)
    {
        ClientNetworkManager.Instance.TargetReceiveTopRankingDataClientRpc(jsonData);
    }


    public IEnumerator GetMyRankingData(int playerId)
    {
        string url = $"{apiBaseUrl}/rankings/{playerId}";

        Debug.Log($"최초 실행시 들어오는 playerID : {playerId}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[ServerToAPI] Complete Individual Ranking Retrieval - Data: {request.downloadHandler.text}");
                TargetReceiveMyRankingDataClientRpc(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ServerToAPI] Failed to retrieve individual ranking: {request.error}");
            }
        }
    }
    private void TargetReceiveMyRankingDataClientRpc(string jsonData)
    {
        ClientNetworkManager.Instance.TargetReceiveMyRankingDataClientRpc(jsonData);
    }

    public IEnumerator GetPlayerDetails(int playerId)
    {
        string url = $"{apiBaseUrl}/playerDetails/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"[ServerToAPI] Complete Player Details Retrieval");
                TargetReceivePlayerDetailsDataClientRpc(json);
            }
            else
            {
                Debug.LogError($"[ServerToAPI] Failed to retrieve player details: {request.error}");
            }
        }
    }
    private void TargetReceivePlayerDetailsDataClientRpc(string jsonData)
    {
        ClientNetworkManager.Instance.TargetReceivePlayerDetailsDataClientRpc(jsonData);
    }

    /// <summary>
    /// 랭킹 업데이트를 위한 트리거 함수
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckRankingShouldUpdate()
    {
        string url = $"{apiBaseUrl}/rankings/update/shouldUpdate";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                var result = JsonConvert.DeserializeObject<RankingShouldUpdateResponse>(json);

                if (result.shouldUpdate)
                {
                    Debug.Log($"[ServerToAPI] Ranking update required - refreshing data and UI");

                    // 랭킹 갱신
                    yield return DataSyncManager.Instance.PlayerRankingUpdated();

                    // UI 갱신 트리거
                    DataSyncManager.Instance.InvokeUIRankingUpdateEvent();
                }
                else
                {
                    Debug.Log($"[ServerToAPI] Ranking data is up to date");
                }
            }
            else
            {
                Debug.LogError($"[ServerToAPI] Failed to check ranking update status: {request.error}");
            }
        }
    }


    #endregion

    // <====================== InGame ======================>

    #region Unity Auth
    public async Task SignInWithCustomId(string customId)
    {
        string url = $"{apiBaseUrl}/unityAuth/getUnityTokens";

        // JSON 데이터 준비
        string jsonData = JsonConvert.SerializeObject(new { customId = customId });


        // UnityWebRequest POST 생성
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ServerToAPI] Failed to request token: {request.error}");
                return;
            }

            // 응답 파싱
            string json = request.downloadHandler.text;
            UnityTokenResponse tokens = JsonConvert.DeserializeObject<UnityTokenResponse>(json);

            AuthenticationService.Instance.ProcessAuthenticationTokens(tokens.idToken, tokens.sessionToken);
            Debug.Log("[ServerToAPI] Complete Custom Id Login");
        }
    }
    #endregion
    #region Session
    public async Task<bool> GetIsInGame(int playerId)
    {
        string url = $"{apiBaseUrl}/gameSession/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                IsInGameResponse response = JsonUtility.FromJson<IsInGameResponse>(json);
                Debug.Log($"[ServerToAPI] playerId: {playerId}, isInGame: {response.isInGame}");
                return response.isInGame == 1;
            }
            else
            {
                Debug.LogError($"[ServerToAPI]세션 조회 실패: {request.responseCode} / {request.error}");
                return false;
            }
        }
    }
    #endregion

    // 데이터 구조
    [System.Serializable]
    public class LoginRecordData
    {
        public int loginId;
        public int playerId;
        public string loginTime;
        public string ipAddress;
    }

    [System.Serializable]
    public class LoginRecordList
    {
        public List<LoginRecordData> records;
    }
    // JSON 파싱을 위한 클래스
    [System.Serializable]
    public class MatchHistoryResponse
    {
        public bool success;
        public MatchHistoryData[] matches;
    }

    [System.Serializable]
    public class PlayerItemsResponse
    {
        public bool success;
        public PlayerItemData[] items;
    }

    [System.Serializable]
    public class LoginUpdateRequest
    {
        public int playerId;
        public string ipAddress;

        public LoginUpdateRequest(int playerId, string ipAddress)
        {
            this.playerId = playerId;
            this.ipAddress = ipAddress;
        }
    }
    [System.Serializable]
    public class RankingShouldUpdateResponse
    {
        public bool shouldUpdate;
    }

    [System.Serializable]
    public class NicknameUpdateRequest
    {
        public int playerId;
        public string playerName;

        public NicknameUpdateRequest(int id, string nickname)
        {
            playerId = id;
            playerName = nickname;
        }
    }
    [System.Serializable]
    public class NicknameDuplicateResponse
    {
        public bool isDuplicate;
    }

    [System.Serializable]
    public class UnityTokenResponse
    {
        public string idToken;
        public string sessionToken;
    }

    [System.Serializable]
    public class GoogleIdUpdateRequest
    {
        public int playerId;
        public string googleId;

        public GoogleIdUpdateRequest(int playerId, string googleId)
        {
            this.playerId = playerId;
            this.googleId = googleId;
        }
    }
    [System.Serializable]
    public class PlayerSessionRequest
    {
        public int playerId;
        public int isInGame; // bool로 보내고 싶다면 1/0으로 변환해서 넣기
    }
    [System.Serializable]
    public class IsInGameResponse
    {
        public int isInGame;
    }
}