using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;
public class ServerToAPIManager : NetworkBehaviour
{
    private string apiBaseUrl = "https://applehunger.site";


    private void Start()
    {
    }

    #region Players Data Region

    /// <summary>
    /// 게임 실행시 단 한번만 발생해야 함. 서버에 유저정보를 하나 늘리는 개념이라서
    /// </summary>
    /// <param name="name"></param>
    /// <param name="conn"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestAddPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(AddPlayer(clientId));
    }

    /// <summary>
    /// 게임 최초 실행시 유저 데이터가 없다면 실행
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    private IEnumerator AddPlayer(ulong clientId)
    {
        string url = $"{apiBaseUrl}/players";

        PlayerData newPlayer = new PlayerData(SystemInfo.deviceUniqueIdentifier,
            "", $"User_{UnityEngine.Random.Range(1000, 9999)}",
            "101",
            "201",
            1200, 500);
        string jsonData = JsonUtility.ToJson(newPlayer);

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
                PlayerAddResponse response = JsonUtility.FromJson<PlayerAddResponse>(playerJsonData);

                // 클라이언트에 Players 정보 저장
                TargetReceivePlayerDataClientRpc(clientId,playerJsonData);

                Debug.Log($"플레이어 추가 성공! 할당된 playerId: {response.playerId}");
            }
            else
                Debug.LogError("플레이어 추가 실패: " + request.error);
        }
    }

    [ClientRpc] // 서버 투 에이피아이 매니저 -> 클라 네트워크 매니저 -> 클라 순으로 진행되게 
    private void TargetReceivePlayerDataClientRpc(ulong clientId, string jsonData)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        FindAnyObjectByType<ClientNetworkManager>().TargetReceivePlayerDataClientRpc(clientId, jsonData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDeletePlayerServerRpc(int playerId)
    {
        StartCoroutine(DeletePlayer(playerId));
    }

    private IEnumerator DeletePlayer(int playerId)
    {
        string url = $"{apiBaseUrl}/players/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log(" 플레이어 삭제 성공");
            else
                Debug.LogError(" 플레이어 삭제 실패: " + request.error);
        }
    }

    // 플레이어 정보 수정 , 클라이언트에 저장된 데이터를 그대로 json으로 api서버에 넘김
    [ServerRpc(RequireOwnership = false)]
    public void RequestUpdatePlayerDataServerRpc(PlayerData updatedData, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(UpdatePlayerData(updatedData, clientId));
    }

    private IEnumerator UpdatePlayerData(PlayerData updatedData, ulong clientId)
    {
        string url = $"{apiBaseUrl}/players/{updatedData.playerId}";

        string jsonData = JsonUtility.ToJson(updatedData);
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT")) // PUT 사용
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("플레이어 클라이언트 데이터를  데이터 서버로 업데이트 성공");
                // 🔹 데이터가 변경되었음을 알림 (자동 동기화)
                DataSyncManager.Instance.PlayerDataUpdated();
            }

            else
                Debug.LogError("업데이트 실패: " + request.error);
        }
    }


    // 플레이어 정보 가져오기(By playerId), id는 나중에 googleId,guestId를 db에 추가해서 그걸로
    // 사용할 예정
    /// <summary>
    /// player의 google 혹은 device Id를 이용해 정보를 조회하는 함수. 만약 정보가 없다면 플레이어가 새로운 계정인 것으로 간주하여 새 플레이어 생성
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="idValue"></param>
    /// <param name="conn"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetPlayerServerRpc(string idType, string idValue, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(GetPlayer(idType, idValue, clientId));
    }

    private IEnumerator GetPlayer(string idType, string idValue, ulong clientId) // 
    {
        string url = $"{apiBaseUrl}/players/search?{idType}={idValue}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                TargetReceivePlayerDataClientRpc(clientId, jsonData);
            }
            else
            {
                Debug.LogError("❌ 플레이어 조회 실패: " + request.error);
                Debug.LogError(" 응답 내용: " + request.downloadHandler.text);
                yield return StartCoroutine(AddPlayer(clientId));
            }
        }
    }

    #endregion

    #region Player MatchRecords Region
    [ServerRpc(RequireOwnership = false)]
    public void RequestAddMatchResultServerRpc(int winnerId, int loserId, ServerRpcParams rpcParams = default) // Matchrecords-ADD 과정
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(AddMatchResult(winnerId, loserId, clientId));
    }

    private IEnumerator AddMatchResult(int winnerId, int loserId, ulong clientId)
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
                Debug.Log($"✅ 매치 결과를 서버에 저장 성공! Winner: {winnerId}, Loser: {loserId}");

                // 🔹 자동 동기화 트리거
                DataSyncManager.Instance.MatchHistoryUpdated(); // 매치 기록 업데이트
                DataSyncManager.Instance.PlayerStatsUpdated();  // 플레이어 스탯 업데이트
                //DataSyncManager.Instance.PlayerRankingUpdated(); // 랭킹 업데이트 (승패 반영) , 랭킹은 굳이 실시간으로 체크해줄 필요가 없음. 서버에서 일정 시간마다 최신화를 해주는게 더 효율적
            }
            else
                Debug.LogError($"❌ 매치 결과 저장 실패: {request.error}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMatchResultServerRpc(int playerId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(GetMatchResult(playerId, clientId));
    }

    private IEnumerator GetMatchResult(int playerId,ulong clientId)
    {
        string url = $"{apiBaseUrl}/matchRecords/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text; // Matchrecords 테이블에서 playerId가 동일한 컬럼들만 추려서 json형태로 list를 만들어 가져온다는 느낌
                Debug.Log($"매치 데이터 json {jsonData}");
                MatchHistoryResponse response = JsonUtility.FromJson<MatchHistoryResponse>(jsonData);

                Debug.Log($"✅ 매치 기록 조회 성공! 총 {response.matches.Length}개 경기");

                foreach (var match in response.matches)
                {
                    Debug.Log($"Match ID: {match.matchId}, Winner: {match.winnerId}, Date: {match.matchDate}");
                    TargetReceiveMatchRecordsClientRpc(clientId, match);
                }
            }
            else
            {
                Debug.LogError($"❌ 매치 기록 조회 실패: {request.error}");
            }
        }
    }
    [ClientRpc]
    public void TargetReceiveMatchRecordsClientRpc(ulong clientId, MatchHistoryData matchHistoryData)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        FindAnyObjectByType<ClientNetworkManager>().TargetReceiveMatchRecordsClientRpc(clientId, matchHistoryData);
    }

    #endregion

    #region Player Stat Region

    // 플레이어 스탯(매치,승리,패배 수) 조회 API
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetPlayerStatServerRpc(int playerId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(GetPlayerStat(playerId, clientId));
    }

    private IEnumerator GetPlayerStat(int playerId, ulong clientId)
    {
        string url = $"{apiBaseUrl}/playerStats/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                TargetReceivePlayerStatClientRpc(clientId, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("❌ 플레이어 스탯 조회 실패: " + request.error);
                Debug.LogError(" 응답 내용: " + request.downloadHandler.text);
            }
        }
    }

    [ClientRpc]
    private void TargetReceivePlayerStatClientRpc(ulong clientId, string jsonData)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        Debug.Log($"✅ 서버에서 받은 PlayerStats 데이터: {jsonData}");

        FindAnyObjectByType<ClientNetworkManager>().TargetReceivePlayerStatsClientRpc(clientId, jsonData);
    }


    #endregion

    #region Player Item Region

    // 플레이어 아이템 정보 조회(프로필 정보에 들어갈 내용)
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetPlayerItemsServerRpc(int playerId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(GetPlayerItems(playerId, clientId));
    }

    private IEnumerator GetPlayerItems(int playerId, ulong clientId)
    {
        string url = $"{apiBaseUrl}/playerItems/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                PlayerItemsResponse response = JsonUtility.FromJson<PlayerItemsResponse>(jsonData);

                // 🔹 리스트 안에 여러 개의 아이템이 들어있으므로, 각각을 TargetReceivePlayerItems로 넘겨줌
                foreach (var playerItem in response.items)
                {
                    TargetReceivePlayerItemsClientRpc(clientId, JsonUtility.ToJson(playerItem));
                }

                //DataSyncManager.Instance.PlayerItemsUpdated(); // 아이템 상태 업데이트
            }
            else
                Debug.LogError($"❌ PlayerItems 조회 실패: {request.error}");
        }
    }

    // JSON 데이터 로드 후 변환
    [ClientRpc]
    private void TargetReceivePlayerItemsClientRpc(ulong clientId, string jsonData)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        FindAnyObjectByType<ClientNetworkManager>().TargetReceivePlayerItemsClientRpc(clientId, jsonData);
    }

    // 🔹 아이템 구매 요청
    [ServerRpc(RequireOwnership = false)]
    public void RequestPurchaseItemServerRpc(int playerId, int itemUniqueId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(PurchaseItem(playerId, itemUniqueId, clientId));
    }

    private IEnumerator PurchaseItem(int playerId, int itemUniqueId,ulong clientId)
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
                Debug.Log($"✅ 아이템 구매 성공! playerId: {playerId}, itemUniqueId: {itemUniqueId}");
                // 🔹 자동 동기화 트리거
                DataSyncManager.Instance.PlayerDataUpdated();  // 재화(currency) 업데이트
                DataSyncManager.Instance.PlayerItemsUpdated(); // 아이템 상태 업데이트
            }
            else
            {
                Debug.LogError($"❌ 아이템 구매 실패: {request.error}");
            }
        }
    }

    #endregion

    #region Player Login Region

    // 로그인 정보 조회
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetLoginRecordsServerRpc(int playerId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(GetLoginRecords(playerId, clientId));
    }

    private IEnumerator GetLoginRecords(int playerId, ulong clientId)
    {
        string url = $"{apiBaseUrl}/loginRecords/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                TargetReceiveLoginRecordsClientRpc(clientId, request.downloadHandler.text);
            else
                Debug.LogError($"❌ LoginRecords 조회 실패: {request.error}");
        }
    }

    [ClientRpc]
    private void TargetReceiveLoginRecordsClientRpc(ulong clientId, string jsonData)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        Debug.Log($"✅ 서버에서 받은 LoginRecords 데이터: {jsonData}");
        FindAnyObjectByType<ClientNetworkManager>().TargetReceiveLoginDataClientRpc(clientId, jsonData);

        // 로그인 데이터를 여러개로 관리할 게 아니라 하나로 관리할 예정인데 이건 나중에 order같은걸 해서 빼던가 해야할거같음
        //List<LoginRecordData> loginRecords = JsonUtility.FromJson<LoginRecordList>(jsonData).records;

        //foreach (var record in loginRecords)
        //{
        //    Debug.Log($"📌 로그인 기록 - playerId: {record.playerId}, time: {record.loginTime}, IP: {record.ipAddress}");
        //}
    }

    // 로그인 정보 업데이트
    [ServerRpc(RequireOwnership = false)]
    public void RequestUpdateLoginTimeServerRpc(int playerId, string ipAddress)
    {
        StartCoroutine(UpdateLoginTime(playerId, ipAddress));
    }

    private IEnumerator UpdateLoginTime(int playerId, string ipAddress)
    {
        string url = $"{apiBaseUrl}/loginRecords";
        string jsonData = JsonUtility.ToJson(new LoginUpdateRequest(playerId, ipAddress));
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
                Debug.Log("✅ 로그인 시간 업데이트 성공");

                // 🔹 자동 동기화 트리거
                DataSyncManager.Instance.PlayerDataUpdated(); // 로그인 정보 업데이트
            }
            else
                Debug.LogError($"❌ 로그인 시간 업데이트 실패: {request.error}");
        }
    }
    #endregion

    #region Player Ranking Data
    // 랭킹 정보 
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetTopRankingServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(GetTopRankingData(clientId));
    }

    private IEnumerator GetTopRankingData(ulong clientId)
    {
        string url = $"{apiBaseUrl}/rankings";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ [Server] 상위 50명 랭킹 조회 성공: {request.downloadHandler.text}");
                TargetReceiveTopRankingDataClientRpc(clientId, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"❌ [Server] 상위 50명 랭킹 조회 실패: {request.error}");
            }
        }
    }
    [ClientRpc]
    private void TargetReceiveTopRankingDataClientRpc(ulong clientId, string jsonData)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        FindAnyObjectByType<ClientNetworkManager>().TargetReceiveTopRankingDataClientRpc(clientId, jsonData);
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestGetMyRankingServerRpc(int playerId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(GetMyRankingData(playerId, clientId));
    }

    private IEnumerator GetMyRankingData(int playerId, ulong clientId)
    {
        string url = $"{apiBaseUrl}/rankings/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ [Server] 개별 랭킹 조회 성공: {request.downloadHandler.text}");
                TargetReceiveMyRankingDataClientRpc(clientId, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"❌ [Server] 개별 랭킹 조회 실패: {request.error}");
            }
        }
    }
    [ClientRpc]
    private void TargetReceiveMyRankingDataClientRpc(ulong clientId, string jsonData)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        FindAnyObjectByType<ClientNetworkManager>().TargetReceiveMyRankingDataClientRpc(clientId, jsonData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestGetGetPlayerDetailsServerRpc(int playerId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        StartCoroutine(GetPlayerDetails(playerId, clientId));
    }

    private IEnumerator GetPlayerDetails(int playerId, ulong clientId)
    {
        string url = $"{apiBaseUrl}/playerDetails/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                TargetReceivePlayerDetailsDataClientRpc(clientId, json);
            }
            else
            {
                Debug.LogError("❌ 플레이어 상세 정보 조회 실패: " + request.error);
            }
        }
    }

    [ClientRpc]
    private void TargetReceivePlayerDetailsDataClientRpc(ulong clientId, string jsonData)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        FindAnyObjectByType<ClientNetworkManager>().TargetReceivePlayerDetailsDataClientRpc(clientId, jsonData);
    }
    #endregion
    // 🔹 데이터 구조
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

}