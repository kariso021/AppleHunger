using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class ClientNetworkManager : NetworkBehaviour
{
    private ServerToAPIManager serverToAPIManager;

    private void Start()
    {
        serverToAPIManager = FindAnyObjectByType<ServerToAPIManager>();
    }

    // 🔹 플레이어 데이터 요청
    public void GetPlayerData() => serverToAPIManager?.RequestGetPlayerServerRpc(SQLiteManager.Instance.player.deviceId);

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
    public void UpdatePlayerData() => serverToAPIManager?.RequestUpdatePlayerDataServerRpc(SQLiteManager.Instance.LoadPlayerData());

    // 🔹 플레이어 아이템 요청
    public void GetPlayerItems() => serverToAPIManager?.RequestGetPlayerItemsServerRpc(SQLiteManager.Instance.player.playerId);

    [TargetRpc]
    public void TargetReceivePlayerItems(NetworkConnection conn, string jsonData)
    {
        PlayerItemList response = JsonUtility.FromJson<PlayerItemList>(jsonData);
        foreach (var item in response.items)
        {
            //ClientDataManager.Instance.AddPlayerItem(item);
            SQLiteManager.Instance.SavePlayerItem(item);
        }
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
            serverToAPIManager.RequestUpdateLoginTimeServerRpc(playerId, "ipAddress-" + UnityEngine.Random.Range(1, 99999));
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

        if (serverToAPIManager != null)
            Debug.Log("아직 안됨");
    }
    public void GetRanking()
    {

        if (serverToAPIManager != null)
            Debug.Log("아직 안됨");

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
