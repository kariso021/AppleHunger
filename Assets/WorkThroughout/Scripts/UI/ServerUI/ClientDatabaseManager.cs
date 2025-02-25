using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
public class ClientDatabaseManager : NetworkBehaviour
{
    ServerToAPIManager serverToAPIManager;
    public ProfilePopup profilePopup;
    private PlayerData ownerPlayerData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        serverToAPIManager = FindAnyObjectByType<ServerToAPIManager>();
        //profilePopup = FindAnyObjectByType<ProfilePopup>();
        //serverToAPIManager.RequestDeletePlayerServerRpc(2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 플레이어 정보 업데이트 to DB
    public void UpdatePlayerData()
    {

        if (serverToAPIManager != null)
            serverToAPIManager.RequestUpdatePlayerDataServerRpc(ownerPlayerData);
    }
    public void AddPlayer(string name)
    {

        if (serverToAPIManager != null)
            serverToAPIManager.RequestAddPlayerServerRpc(name);
    }
    public void DeletePlayer(int playerId)
    {
        if (serverToAPIManager != null)
            serverToAPIManager.RequestDeletePlayerServerRpc(playerId);
    }
    public void GetPlayerData(int playerId)
    {

        if (serverToAPIManager != null)
            serverToAPIManager.RequestGetPlayerServerRpc(playerId);
        
    }

    // JSON 데이터를 받아서 ProfilePopup UI 업데이트
    public void ApplyPlayerData(string jsonData)
    {
        ownerPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);

        if (profilePopup != null)
        {
            //2025-02-24 현재 구조를 변경중이므로 수정 필요
            //profilePopup.SetProfile(
            //    null, // 프로필 이미지는 경로로 관리되므로, 나중에 로드 필요
            //    ownerPlayerData.playerName,
            //    ownerPlayerData.rating,
            //    //ownerPlayerData.icons,
            //    //ownerPlayerData.boards
            //);
        }
    }

    // Client
    public void ChangePlayerDataTest(int newRating)
    {
        if(ownerPlayerData == null) return;
        int temp = ownerPlayerData.rating;
        ownerPlayerData.rating = newRating;
        UpdatePlayerData();
        Debug.Log($"BASIC : {temp} , NEW : {ownerPlayerData.rating}");
    }
}
