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
    public void RequestAddPlayerServerRpc(string name, NetworkConnection conn = null)
    {
        StartCoroutine(AddPlayer(name,conn));
    }

    private IEnumerator AddPlayer(string name, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/players";

        PlayerData newPlayer = new PlayerData("deviceId-"+Random.Range(0,1000), "googleId-" + Random.Range(0, 1000), name, "profileIcon", "boardImage", Random.Range(900,1500), Random.Range(3000,15000));
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
                string responseText = request.downloadHandler.text;
                PlayerAddResponse response = JsonUtility.FromJson<PlayerAddResponse>(responseText);

                Debug.Log($"플레이어 추가 성공! 할당된 playerId: {response.playerId}");
            }
            else
                Debug.LogError("플레이어 추가 실패: " + request.error);
        }
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

    // 플레이어 정보 수정
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
                Debug.Log("플레이어 데이터 업데이트 성공");
            else
                Debug.LogError("업데이트 실패: " + request.error);
        }
    }


    // 플레이어 정보 가져오기(By playerId), id는 나중에 googleId,guestId를 db에 추가해서 그걸로
    // 사용할 예정
    [ServerRpc(RequireOwnership = false)]
    public void RequestGetPlayerServerRpc(int playerId, NetworkConnection conn = null) // conn에 클라 객체들에 대한 
    {
        StartCoroutine(GetPlayer(playerId, conn));
    }

    private IEnumerator GetPlayer(int playerId, NetworkConnection conn)
    {
        string url = $"{apiBaseUrl}/players/{playerId}";

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