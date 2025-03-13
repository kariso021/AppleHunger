using System;
using System.Transactions;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

public class ClientNetworkManager : NetworkBehaviour
{
    private ServerToAPIManager serverToAPIManager;

    private void Start()
    {
        serverToAPIManager = FindAnyObjectByType<ServerToAPIManager>();

    }
    #region DataSync
    // ✅ 플레이어 데이터 동기화
    private void SyncPlayerData()
    {
        Debug.Log("🔄 [Client] 플레이어 데이터 자동 동기화 시작...");
        GetPlayerData("playerId", SQLiteManager.Instance.player.playerId.ToString());
    }

    // ✅ 플레이어 아이템 동기화
    private void SyncPlayerItems()
    {
        Debug.Log("🔄 [Client] 플레이어 아이템 자동 동기화 시작...");
        GetPlayerItems(SQLiteManager.Instance.player.playerId);
    }

    // ✅ 플레이어 스탯 동기화
    private void SyncPlayerStats()
    {
        Debug.Log("🔄 [Client] 플레이어 스탯 자동 동기화 시작...");
        GetPlayerStats(SQLiteManager.Instance.player.playerId);
    }

    // ✅ 랭킹 동기화
    private void SyncPlayerRanking()
    {
        Debug.Log("🔄 [Client] 랭킹 데이터 자동 동기화 시작...");
        GetRankingList();
    }

    // ✅ 매치 기록 동기화
    private void SyncMatchHistory()
    {
        Debug.Log("🔄 [Client] 매치 기록 자동 동기화 시작...");
        GetMatchRecords(SQLiteManager.Instance.player.playerId);
    }
    #endregion
    #region Player Data
    // 🔹 플레이어 데이터 요청
    public void GetPlayerData(string idType, string idValue) => serverToAPIManager?.RequestGetPlayerServerRpc(idType, idValue);
    [ClientRpc]
    public void TargetReceivePlayerDataClientRpc(ulong clientId, string jsonData)
    {
        SQLiteManager.Instance.SavePlayerData(JsonUtility.FromJson<PlayerData>(jsonData));

        // ✅ 저장 후 바로 다시 로드하여 확인
        PlayerData loadedPlayer = SQLiteManager.Instance.LoadPlayerData();
        Debug.Log($"✅ [Client] SQLite에서 불러온 PlayerData: {loadedPlayer.ToString()}");
    }

    // 🔹 플레이어 추가
    public void AddPlayer(string name) => serverToAPIManager?.RequestAddPlayerServerRpc(name);

    // 🔹 플레이어 삭제
    public void DeletePlayer(int playerId) => serverToAPIManager?.RequestDeletePlayerServerRpc(playerId);

    // 🔹 플레이어 정보 업데이트
    public void UpdatePlayerData() => serverToAPIManager?.RequestUpdatePlayerDataServerRpc(SQLiteManager.Instance.player);
    #endregion
    #region Player Items
    // 🔹 플레이어 아이템 요청
    public void GetPlayerItems(int playerId) => serverToAPIManager?.RequestGetPlayerItemsServerRpc(playerId);

    [ClientRpc]
    public void TargetReceivePlayerItemsClientRpc(ulong clientId, string jsonData)
    {
        SQLiteManager.Instance.SavePlayerItem(JsonUtility.FromJson<PlayerItemData>(jsonData));
    }

    // 플레이어 아이템 구매 요청
    public void PurchasePlayerItem(int playerId, int itemUniqueId)
    {
        if (serverToAPIManager != null)
            serverToAPIManager.RequestPurchaseItemServerRpc(playerId, itemUniqueId);
    }
    #endregion
    #region Player Stats
    // 🔹 플레이어 스탯 요청
    public void GetPlayerStats(int playerId) => serverToAPIManager?.RequestGetPlayerStatServerRpc(playerId);

    [ClientRpc] 
    public void TargetReceivePlayerStatsClientRpc(ulong clientId, string jsonData)
    {
        PlayerStatsResponse playerStatsResponse = JsonUtility.FromJson<PlayerStatsResponse>(jsonData);
        Debug.Log($"{playerStatsResponse.playerStats.playerId} , Total : {playerStatsResponse.playerStats.totalGames} , Winrate : {playerStatsResponse.playerStats.winRate}");
        SQLiteManager.Instance.SavePlayerStats(playerStatsResponse.playerStats);
    }
    #endregion
    #region Player Matches
    // 매치 업데이트 to DB
    public void AddMatchRecords(int winnerId, int loserId)
    {
        if (serverToAPIManager != null)
        {
            serverToAPIManager.RequestAddMatchResultServerRpc(winnerId, loserId);
        }
    }

    public void GetMatchRecords(int playerId)
    {
        if (serverToAPIManager != null)
            serverToAPIManager.RequestMatchResultServerRpc(playerId);
    }

    [ClientRpc]
    public void TargetReceiveMatchRecordsClientRpc(ulong clientId, MatchHistoryData matchHistoryData)
    {
        SQLiteManager.Instance.SaveMatchHistory(matchHistoryData);
    }
    #endregion
    #region Player Login
    // 로그인 정보 업데이트 to DB
    public void UpdateLogin(int playerId)
    {

        if (serverToAPIManager != null)
            serverToAPIManager.RequestUpdateLoginTimeServerRpc(playerId, "::1");
    }
    // 🔹 로그인 요청
    public void GetLogin(int playerId) => serverToAPIManager?.RequestGetLoginRecordsServerRpc(playerId);

    [ClientRpc]
    public void TargetReceiveLoginDataClientRpc(ulong clientId, string jsonData)
    {
        LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonData);

        Debug.Log($"{response.records.playerId} , ip : {response.records.ipAddress} , id : {response.records.loginId}");

        SQLiteManager.Instance.SaveLoginData(response.records);
    }
    #endregion
    #region Player Ranking
    // 랭킹 정보 업데이트 to DB
    public void GetRankingList()
    {
        Debug.Log("🔹 [Client] 랭킹 데이터 요청 시작");

        // 상위 50명 랭킹 요청
        serverToAPIManager?.RequestGetTopRankingServerRpc();

        // 개별 플레이어 랭킹 요청
        serverToAPIManager?.RequestGetMyRankingServerRpc(SQLiteManager.Instance.player.playerId);
    }

    // ✅ 서버에서 받은 상위 50명 랭킹 저장
    [ClientRpc]
    public void TargetReceiveTopRankingDataClientRpc(ulong clientId, string jsonData)
    {
        Debug.Log($"✅ [Client] 상위 50명 랭킹 데이터 수신: {jsonData}");

        RankingDataResponse rankingListResponse = JsonUtility.FromJson<RankingDataResponse>(jsonData);
        // SQLite에 저장
        foreach (var rankingData in rankingListResponse.topRankings)
        {
            SQLiteManager.Instance.SaveRankingData(rankingData);
        }

        Debug.Log($"📌 상위 50명 랭킹 저장 완료 (총 {rankingListResponse.topRankings.Length}명)");
    }

    // ✅ 서버에서 받은 내 개별 랭킹 저장
    [ClientRpc]
    public void TargetReceiveMyRankingDataClientRpc(ulong clientId, string jsonData)
    {
        Debug.Log($"✅ [Client] 개별 랭킹 데이터 수신: {jsonData}");

        MyRankingData myRankingData = JsonUtility.FromJson<MyRankingData>(jsonData);

        // SQLite에 저장
        SQLiteManager.Instance.SaveMyRankingData(myRankingData.myRanking);

        Debug.Log($"📌 내 랭킹 저장 완료: {myRankingData.myRanking.playerName} (Rank: {myRankingData.myRanking.rankPosition})");
    }

    [ClientRpc]
    public void TargetReceivePlayerDetailsDataClientRpc(ulong clientId, string jsonData)
    {
        Debug.Log($"✅ [Client] 상세 정보 수신: {jsonData}");

        PlayerDetailsResponse playerDetailsResponse = JsonUtility.FromJson<PlayerDetailsResponse>(jsonData);
        Debug.Log(playerDetailsResponse.playerDetails.ToString());
        SQLiteManager.Instance.playerDetails = playerDetailsResponse.playerDetails;

        Debug.Log($"📌 상세 정보 저장 완료: {playerDetailsResponse.playerDetails.playerName} (Rank: {playerDetailsResponse.playerDetails.rating})");

        // ✅ 데이터 수신 완료 후 실행
        FindAnyObjectByType<PopupManager>().OnDataReceived();

    }
    public void GetPlayerDetalis(int playerId) // 콜백 추가. 
    {
        if (serverToAPIManager != null)
            serverToAPIManager.RequestGetGetPlayerDetailsServerRpc(playerId);
    }
    #endregion

}

[System.Serializable]
public class LoginResponse
{
    public bool success;
    public LoginData records;
}

[System.Serializable]
public class PlayerStatsResponse
{
    public bool success;
    public PlayerStatsData playerStats;
}

[System.Serializable]
public class RankingDataResponse
{
    public bool success;
    public PlayerRankingData[] topRankings;
}

[System.Serializable]
public class PurchaseResponse
{
    public bool success;
    public string message;
    public int remainingCurrency;
    public string error;
}