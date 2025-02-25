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

    // JSON 데이터를 받아서 ProfilePopup UI 업데이트
    public void ApplyPlayerData(string jsonData)
    {
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        if (profilePopup != null)
        {
            profilePopup.SetProfile(
                null, // 프로필 이미지는 경로로 관리되므로, 나중에 로드 필요
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
