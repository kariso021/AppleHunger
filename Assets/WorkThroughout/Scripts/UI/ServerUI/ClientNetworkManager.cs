using FishNet.Connection;
using FishNet.Object;
using System;
using System.Transactions;
using UnityEngine;
using static UnityEditor.Progress;

public class ClientNetworkManager : NetworkBehaviour
{
    private ServerToAPIManager serverToAPIManager;

    private void Start()
    {
        serverToAPIManager = FindAnyObjectByType<ServerToAPIManager>();
    }

    // 🔹 플레이어 데이터 요청
    public void GetPlayerData(string idType,string idValue) => serverToAPIManager?.RequestGetPlayerServerRpc(idType,idValue);
    [TargetRpc]
    public void TargetReceivePlayerData(NetworkConnection conn, string jsonData)
    {
        SQLiteManager.Instance.SavePlayerData(JsonUtility.FromJson<PlayerData>(jsonData));
    }

    // 🔹 플레이어 추가
    public void AddPlayer(string name) => serverToAPIManager?.RequestAddPlayerServerRpc(name);

    // 🔹 플레이어 삭제
    public void DeletePlayer(int playerId) => serverToAPIManager?.RequestDeletePlayerServerRpc(playerId);

    // 🔹 플레이어 정보 업데이트
    public void UpdatePlayerData() => serverToAPIManager?.RequestUpdatePlayerDataServerRpc(SQLiteManager.Instance.player);

    // 🔹 플레이어 아이템 요청
    public void GetPlayerItems() => serverToAPIManager?.RequestGetPlayerItemsServerRpc(SQLiteManager.Instance.player.playerId);

    [TargetRpc]
    public void TargetReceivePlayerItems(NetworkConnection conn, string jsonData)
    {
        SQLiteManager.Instance.SavePlayerItem(JsonUtility.FromJson<PlayerItemData>(jsonData));
    }
    // 플레이어 아이템 해금 요청
    public void UnlockPlayerItems(int itemUniqueId)
    {

        if (serverToAPIManager != null)
            serverToAPIManager.RequestUnlockPlayerItemServerRpc(SQLiteManager.Instance.player.playerId, itemUniqueId);
    }
    // 🔹 플레이어 스탯 요청
    public void GetPlayerStats() => serverToAPIManager?.RequestGetPlayerStatServerRpc(SQLiteManager.Instance.player.playerId);

    [TargetRpc]
    public void TargetReceivePlayerStats(NetworkConnection conn, string jsonData)
    {
        PlayerStatsResponse playerStatsResponse = JsonUtility.FromJson<PlayerStatsResponse>(jsonData);
        Debug.Log($"{playerStatsResponse.playerStats.playerId} , Total : {playerStatsResponse.playerStats.totalGames} , Winrate : {playerStatsResponse.playerStats.winRate}");
        SQLiteManager.Instance.SavePlayerStats(playerStatsResponse.playerStats);
    }

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

    [TargetRpc]
    public void TargetReceiveMatchRecords(NetworkConnection conn, MatchHistoryData matchHistoryData)
    {
        SQLiteManager.Instance.SaveMatchHistory(matchHistoryData);
    }

    // 로그인 정보 업데이트 to DB
    public void UpdateLogin(int playerId)
    {

        if (serverToAPIManager != null)
            serverToAPIManager.RequestUpdateLoginTimeServerRpc(playerId, "::1");
    }
    // 🔹 로그인 요청
    public void GetLogin(int playerId) => serverToAPIManager?.RequestGetLoginRecordsServerRpc(playerId);

    [TargetRpc]
    public void TargetReceiveLoginData(NetworkConnection conn, string jsonData)
    {
        LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonData);

        Debug.Log($"{response.records.playerId} , ip : {response.records.ipAddress} , id : {response.records.loginId}");

        SQLiteManager.Instance.SaveLoginData(response.records);
    }

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
    [TargetRpc]
    public void TargetReceiveTopRankingData(NetworkConnection conn, string jsonData)
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
    [TargetRpc]
    public void TargetReceiveMyRankingData(NetworkConnection conn, string jsonData)
    {
        Debug.Log($"✅ [Client] 개별 랭킹 데이터 수신: {jsonData}");

        MyRankingData myRankingData = JsonUtility.FromJson<MyRankingData>(jsonData);

        // SQLite에 저장
        SQLiteManager.Instance.SaveMyRankingData(myRankingData.myRanking);

        Debug.Log($"📌 내 랭킹 저장 완료: {myRankingData.myRanking.playerName} (Rank: {myRankingData.myRanking.rankPosition})");
    }

    [TargetRpc]
    public void TargetReceivePlayerDetailsData(NetworkConnection conn, string jsonData)
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
