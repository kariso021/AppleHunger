using UnityEngine;
using FishNet.Object;
public class ClientDatabaseManager : NetworkBehaviour
{
    ServerToAPIManager serverToAPIManager;
    public ProfilePopup profilePopup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        serverToAPIManager = FindAnyObjectByType<ServerToAPIManager>();
        //profilePopup = FindAnyObjectByType<ProfilePopup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPlayer(string name, string icon, string board, int games, int wins, int losses, int rating, int currency, int icons, int boards)
    {
        if(serverToAPIManager != null)
            serverToAPIManager.RequestAddPlayerServerRpc(name, icon, board, games, wins, losses, rating, currency, icons, boards);
    }
    public void GetPlayerData(int playerId)
    {
        if (serverToAPIManager != null)
            serverToAPIManager.RequestGetPlayerServerRpc(playerId);
    }

    // JSON �����͸� �޾Ƽ� ProfilePopup UI ������Ʈ
    public void ApplyPlayerData(string jsonData)
    {
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        if (profilePopup != null)
        {
            profilePopup.SetProfile(
                null, // ������ �̹����� ��η� �����ǹǷ�, ���߿� �ε� �ʿ�
                playerData.playerName,
                playerData.totalGames,
                playerData.wins,
                playerData.losses,
                playerData.rating,
                playerData.icons,
                playerData.boards
            );
        }
    }
}
