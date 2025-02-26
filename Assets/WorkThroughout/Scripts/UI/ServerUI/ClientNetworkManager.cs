using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

public class ClientNetworkManager : NetworkBehaviour
{
    private ServerToAPIManager serverToAPIManager;

    private void Start()
    {
        serverToAPIManager = FindAnyObjectByType<ServerToAPIManager>();
    }

    // 🔹 플레이어 데이터 요청
    public void GetPlayerData(int playerId) => serverToAPIManager?.RequestGetPlayerServerRpc(playerId);

    [TargetRpc]
    public void TargetReceivePlayerData(NetworkConnection conn, string jsonData)
        => ClientDataManager.Instance.SetPlayerData(JsonUtility.FromJson<PlayerData>(jsonData));

    // 🔹 플레이어 추가
    public void AddPlayer(string name) => serverToAPIManager?.RequestAddPlayerServerRpc(name);

    // 🔹 플레이어 삭제
    public void DeletePlayer(int playerId) => serverToAPIManager?.RequestDeletePlayerServerRpc(playerId);

    // 🔹 플레이어 정보 업데이트
    public void UpdatePlayerData() => serverToAPIManager?.RequestUpdatePlayerDataServerRpc(ClientDataManager.Instance.playerData);

    // 🔹 플레이어 아이템 요청
    public void GetPlayerItems(int playerId) => serverToAPIManager?.RequestGetPlayerItemsServerRpc(playerId);

    [TargetRpc]
    public void TargetReceivePlayerItems(NetworkConnection conn, string jsonData)
    {
        PlayerItemList response = JsonUtility.FromJson<PlayerItemList>(jsonData);
        foreach (var item in response.items) ClientDataManager.Instance.AddPlayerItem(item);
    }
    // 플레이어 아이템 해금 요청
    public void UnlockPlayerItems(int playerId, int itemUniqueId)
    {

        if (serverToAPIManager != null)
            serverToAPIManager.RequestUnlockPlayerItemServerRpc(playerId, itemUniqueId);
    }
    // 🔹 플레이어 스탯 요청
    public void GetPlayerStats(int playerId) => serverToAPIManager?.RequestGetPlayerStatServerRpc(playerId);

    [TargetRpc]
    public void TargetReceivePlayerStats(NetworkConnection conn, string jsonData)
        => ClientDataManager.Instance.SetPlayerStats(JsonUtility.FromJson<PlayerStatsData>(jsonData));

    // 매치 업데이트 to DB
    public void AddMatchRecords(int winnerId, int loserId)
    {
        if (serverToAPIManager != null)
        {
            serverToAPIManager.RequestAddMatchResultServerRpc(winnerId, loserId);
        }
    }

    [TargetRpc]
    public void TargetReceiveMatchRecords(NetworkConnection conn, string jsonData) 
        => ClientDataManager.Instance.AddMatchHistory(JsonUtility.FromJson<MatchHistoryData>(jsonData));

    // 로그인 정보 업데이트 to DB
    public void UpdateLogin(int playerId)
    {

        if (serverToAPIManager != null)
            serverToAPIManager.RequestUpdateLoginTimeServerRpc(playerId, "ipAddress-" + Random.Range(1, 99999));
    }
    // 🔹 로그인 요청
    public void GetLogin(int playerId) => serverToAPIManager?.RequestGetLoginRecordsServerRpc(playerId);

    [TargetRpc]
    public void TargetReceiveLoginData(NetworkConnection conn, string jsonData)
        => ClientDataManager.Instance.SetLoginData(JsonUtility.FromJson<LoginData>(jsonData));

    // 랭킹 정보 업데이트 to DB
    public void GetRankingList(int playerId)
    {

        if (serverToAPIManager != null)
            Debug.Log("아직 안됨");
    }
    public void GetRanking(int playerId)
    {

        if (serverToAPIManager != null)
            Debug.Log("아직 안됨");

    }
}
