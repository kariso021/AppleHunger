using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ServerToAPIManager : NetworkBehaviour
{
    private string apiBaseUrl = "https://localhost";


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
    public void RequestAddPlayerServerRpc(string name, NetworkConnection conn = null)
    {
        StartCoroutine(AddPlayer(name, conn));
    }

    private IEnumerator AddPlayer(string name, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/players";

        PlayerData newPlayer = new PlayerData("deviceId-" + UnityEngine.Random.Range(0, 1000),
            "googleId-" + UnityEngine.Random.Range(0, 1000), name,
            "profileIcon",
            "boardImage",
            UnityEngine.Random.Range(900, 1500), UnityEngine.Random.Range(3000, 15000));
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
                //TargetReceivePlayerData(conn, playerJsonData);

                Debug.Log($"플레이어 추가 성공! 할당된 playerId: {response.playerId}");
            }
            else
                Debug.LogError("플레이어 추가 실패: " + request.error);
        }
    }

    [TargetRpc] // 서버 투 에이피아이 매니저 -> 클라 네트워크 매니저 -> 클라 순으로 진행되게 
    private void TargetReceivePlayerData(NetworkConnection conn, string jsonData)
    {
        FindAnyObjectByType<ClientNetworkManager>().TargetReceivePlayerData(conn, jsonData);
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
    public void RequestUpdatePlayerDataServerRpc(PlayerData updatedData, NetworkConnection conn = null)
    {
        StartCoroutine(UpdatePlayerData(updatedData, conn));
    }

    private IEnumerator UpdatePlayerData(PlayerData updatedData, NetworkConnection conn)
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
    /// idType = playerId,deviceId,googleId 중 택 1, idValue는 device,google의 경우 기기의 id값을, playerId는 playerId값을 입력
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="idValue"></param>
    /// <param name="conn"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetPlayerServerRpc(string idType, string idValue, NetworkConnection conn = null)
    {
        StartCoroutine(GetPlayer(idType, idValue, conn));
    }

    private IEnumerator GetPlayer(string idType, string idValue, NetworkConnection conn) // 
    {
        string url = $"{apiBaseUrl}/players/search?{idType}={idValue}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                TargetReceivePlayerData(conn, jsonData);
            }
            else
            {
                Debug.LogError("❌ 플레이어 조회 실패: " + request.error);
                Debug.LogError(" 응답 내용: " + request.downloadHandler.text);
            }
        }
    }

    #endregion

    #region Player MatchRecords Region
    [ServerRpc(RequireOwnership = false)]
    public void RequestAddMatchResultServerRpc(int winnerId, int loserId, NetworkConnection conn = null) // Matchrecords-ADD 과정
    {
        StartCoroutine(AddMatchResult(winnerId, loserId, conn));
    }

    private IEnumerator AddMatchResult(int winnerId, int loserId, NetworkConnection conn)
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
    public void RequestMatchResultServerRpc(int playerId, NetworkConnection conn = null) // Matchrecords-Get과정
    {
        StartCoroutine(GetMatchResult(playerId, conn));
    }

    private IEnumerator GetMatchResult(int playerId, NetworkConnection conn)
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
                    TargetReceiveMatchRecords(conn, match);
                }
            }
            else
            {
                Debug.LogError($"❌ 매치 기록 조회 실패: {request.error}");
            }
        }
    }
    [TargetRpc]
    public void TargetReceiveMatchRecords(NetworkConnection conn, MatchHistoryData matchHistoryData)
    {
        FindAnyObjectByType<ClientNetworkManager>().TargetReceiveMatchRecords(conn, matchHistoryData);
    }

    #endregion

    #region Player Stat Region

    // 플레이어 스탯(매치,승리,패배 수) 조회 API
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetPlayerStatServerRpc(int playerId, NetworkConnection conn = null)
    {
        StartCoroutine(GetPlayerStat(playerId, conn));
    }

    private IEnumerator GetPlayerStat(int playerId, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/playerStats/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                TargetReceivePlayerStat(conn, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("❌ 플레이어 스탯 조회 실패: " + request.error);
                Debug.LogError(" 응답 내용: " + request.downloadHandler.text);
            }
        }
    }

    [TargetRpc]
    private void TargetReceivePlayerStat(NetworkConnection conn, string jsonData)
    {
        Debug.Log($"✅ 서버에서 받은 PlayerStats 데이터: {jsonData}");

        FindAnyObjectByType<ClientNetworkManager>().TargetReceivePlayerStats(conn, jsonData);
    }


    #endregion

    #region Player Item Region

    // 플레이어 아이템 정보 조회(프로필 정보에 들어갈 내용)
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetPlayerItemsServerRpc(int playerId, NetworkConnection conn = null)
    {
        StartCoroutine(GetPlayerItems(playerId, conn));
    }

    private IEnumerator GetPlayerItems(int playerId, NetworkConnection conn)
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
                    TargetReceivePlayerItems(conn, JsonUtility.ToJson(playerItem));
                }

                //DataSyncManager.Instance.PlayerItemsUpdated(); // 아이템 상태 업데이트
            }
            else
                Debug.LogError($"❌ PlayerItems 조회 실패: {request.error}");
        }
    }

    // JSON 데이터 로드 후 변환
    [TargetRpc]
    private void TargetReceivePlayerItems(NetworkConnection conn, string jsonData)
    {
        FindAnyObjectByType<ClientNetworkManager>().TargetReceivePlayerItems(conn, jsonData);
    }

    // 🔹 아이템 구매 요청
    [ServerRpc(RequireOwnership = false)]
    public void RequestPurchaseItemServerRpc(int playerId, int itemUniqueId, NetworkConnection conn = null)
    {
        StartCoroutine(PurchaseItem(playerId, itemUniqueId, conn));
    }

    private IEnumerator PurchaseItem(int playerId, int itemUniqueId, NetworkConnection conn)
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
    public void RequestGetLoginRecordsServerRpc(int playerId, NetworkConnection conn = null)
    {
        StartCoroutine(GetLoginRecords(playerId, conn));
    }

    private IEnumerator GetLoginRecords(int playerId, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/loginRecords/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                TargetReceiveLoginRecords(conn, request.downloadHandler.text);
            else
                Debug.LogError($"❌ LoginRecords 조회 실패: {request.error}");
        }
    }

    [TargetRpc]
    private void TargetReceiveLoginRecords(NetworkConnection conn, string jsonData)
    {
        Debug.Log($"✅ 서버에서 받은 LoginRecords 데이터: {jsonData}");
        FindAnyObjectByType<ClientNetworkManager>().TargetReceiveLoginData(conn, jsonData);

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
    public void RequestGetTopRankingServerRpc(NetworkConnection conn = null)
    {
        StartCoroutine(GetTopRankingData(conn));
    }

    private IEnumerator GetTopRankingData(NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/rankings";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ [Server] 상위 50명 랭킹 조회 성공: {request.downloadHandler.text}");
                TargetReceiveTopRankingData(conn, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"❌ [Server] 상위 50명 랭킹 조회 실패: {request.error}");
            }
        }
    }
    [TargetRpc]
    private void TargetReceiveTopRankingData(NetworkConnection conn, string jsonData)
    {
        FindAnyObjectByType<ClientNetworkManager>().TargetReceiveTopRankingData(conn, jsonData);
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestGetMyRankingServerRpc(int playerId, NetworkConnection conn = null)
    {
        StartCoroutine(GetMyRankingData(playerId, conn));
    }

    private IEnumerator GetMyRankingData(int playerId, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/rankings/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ [Server] 개별 랭킹 조회 성공: {request.downloadHandler.text}");
                TargetReceiveMyRankingData(conn, request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"❌ [Server] 개별 랭킹 조회 실패: {request.error}");
            }
        }
    }
    [TargetRpc]
    private void TargetReceiveMyRankingData(NetworkConnection conn, string jsonData)
    {
        FindAnyObjectByType<ClientNetworkManager>().TargetReceiveMyRankingData(conn, jsonData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestGetGetPlayerDetailsServerRpc(int playerId,NetworkConnection conn = null)
    {
        StartCoroutine(GetPlayerDetails(playerId,conn));
    }

    private IEnumerator GetPlayerDetails(int playerId, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/playerDetails/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                TargetReceivePlayerDetailsData(conn, json);
            }
            else
            {
                Debug.LogError("❌ 플레이어 상세 정보 조회 실패: " + request.error);
            }
        }
    }

    [TargetRpc]
    private void TargetReceivePlayerDetailsData(NetworkConnection conn, string jsonData)
    {
        FindAnyObjectByType<ClientNetworkManager>().TargetReceivePlayerDetailsData(conn, jsonData);
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