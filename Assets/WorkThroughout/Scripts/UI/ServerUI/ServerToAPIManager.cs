using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using FishNet.Demo.AdditiveScenes;
public class ServerToAPIManager : NetworkBehaviour
{
    private string apiBaseUrl = "https://localhost";

    ClientDatabaseManager clientDatabaseManager;

    private void Start()
    {
        clientDatabaseManager = FindAnyObjectByType<ClientDatabaseManager>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestAddPlayerServerRpc(string playerName, string profileIcon, string boardImage, int totalGames, int wins, int losses, int rating, int currency, int icons, int boards)
    {
        StartCoroutine(AddPlayer(playerName, profileIcon, boardImage, totalGames, wins, losses, rating, currency, icons, boards));
    }

    private IEnumerator AddPlayer(string playerName, string profileIcon, string boardImage, int totalGames, int wins, int losses, int rating, int currency, int icons, int boards)
    {
        string url = $"{apiBaseUrl}/addPlayer";

        PlayerData newPlayer = new PlayerData(0, playerName, profileIcon, boardImage, totalGames, wins, losses, rating, currency, icons, boards);
        string jsonData = JsonUtility.ToJson(newPlayer);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log(" 플레이어 추가 성공");
            else
                Debug.LogError("플레이어 추가 실패: " + request.error);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestUpdatePlayerDataServerRpc(PlayerData updatedData, NetworkConnection conn = null)
    {
        StartCoroutine(UpdatePlayerData(updatedData, conn));
    }

    private IEnumerator UpdatePlayerData(PlayerData updatedData, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/updatePlayer";

        string jsonData = JsonUtility.ToJson(updatedData);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("플레이어 데이터 업데이트 성공");
            else
                Debug.LogError("업데이트 실패: " + request.error);
        }
    }


    // �÷��̾� ���� ��û
    [ServerRpc(RequireOwnership = false)]
    public void RequestDeletePlayerServerRpc(int playerId)
    {
        StartCoroutine(DeletePlayer(playerId));
    }

    private IEnumerator DeletePlayer(int playerId)
    {
        string url = $"{apiBaseUrl}/deletePlayer/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log(" 플레이어 삭제 실패");
            else
                Debug.LogError(" 플레이어 삭제 실패: " + request.error);
        }
    }

    // �÷��̾� ���� ��ȸ ��û
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetPlayerServerRpc(int playerId, NetworkConnection conn = null) // conn에 클라 객체들에 대한 
    {
        StartCoroutine(GetPlayer(playerId, conn));
    }

    private IEnumerator GetPlayer(int playerId, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/player/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                TargetReceivePlayerData(conn, request.downloadHandler.text);
            else
            {
                Debug.LogError(" 플레이어 조회 실패: " + request.error);
                Debug.LogError(" 응답 내용: " + request.downloadHandler.text);
            }
        }
    }

    [TargetRpc]
    private void TargetReceivePlayerData(NetworkConnection conn, string jsonData)
    {
        
        Debug.Log(" 플레이어 정보 수신: " + jsonData);
        
        if (clientDatabaseManager != null)
        {
            clientDatabaseManager.ApplyPlayerData(jsonData);
        }
        else
            Debug.Log("클라 베이스 못찾음");
    }
}