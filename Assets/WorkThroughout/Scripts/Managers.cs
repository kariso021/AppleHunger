using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static ServerToAPIManager;

public class Managers : MonoBehaviour
{
    private string apiBaseUrl = "https://applehunger.site";

    #region About Player Data 
    // 플레이어 정보 수정 , 클라이언트에 저장된 데이터를 그대로 json으로 api서버에 넘김
    public IEnumerator UpdatePlayerData(PlayerData updatedData)
    {
        string url = $"{apiBaseUrl}/players/{updatedData.playerId}";

        string jsonData = JsonConvert.SerializeObject(updatedData);
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT")) // PUT 사용
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("업데이트 성공");
            }

            else
                Debug.LogError("업데이트 실패: " + request.error);
        }
    }

    //updateRewards
    public IEnumerator UpdateCurrencyAndRating(int playerId, int currency, int rating)
    {
        string url = $"{apiBaseUrl}/players/updateRewards";
        string jsonData = JsonConvert.SerializeObject(new UpdateStatsRequest(playerId, currency, rating));

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ 통화 및 점수 업데이트 성공!");
        }
        else
        {
            Debug.LogError("❌ 통화/점수 업데이트 실패: " + request.error);
        }
    }


    #endregion
    #region Player MatchRecords Region
    public IEnumerator AddMatchResult(int winnerId, int loserId)
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
                Debug.Log($"매치 결과를 서버에 저장 성공! Winner: {winnerId}, Loser: {loserId}");
            }
            else
                Debug.LogError($"매치 결과 저장 실패: {request.error}");
        }
    }

    #endregion
    #region Session
    public async Task UpdatePlayerSession(int playerId, bool isInGame)
    {
        string url = $"{apiBaseUrl}/gameSession/upsert";

        var payload = new PlayerSessionRequest
        {
            playerId = playerId,
            isInGame = isInGame ? 1 : 0 // bool → int 변환
        };

        string json = JsonConvert.SerializeObject(payload);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[ServerToAPI] playerSession 업데이트 성공: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[ServerToAPI]  playerSession 업데이트 실패: {request.responseCode} / {request.error}");
            }
        }
    }

    public async Task<bool> GetIsInGame(int playerId)
    {
        string url = $"{apiBaseUrl}/gameSession/{playerId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                IsInGameResponse response = JsonUtility.FromJson<IsInGameResponse>(json);
                Debug.Log($"[ServerToAPI] playerId: {playerId}, isInGame: {response.isInGame}");
                return response.isInGame == 1;
            }
            else
            {
                Debug.LogError($"[ServerToAPI]세션 조회 실패: {request.responseCode} / {request.error}");
                return false;
            }
        }
    }
    #endregion
    // 서버에서 위 함수들을 위 함수들을 이용해 DB 서버에 값을 넘기고
    // 클라로 넘기는 함수쪽에서 FindAnyObjectByType<> 을 이용해 
    // 각 클라의 ClientNetworkManager의 GetMatchrecords 와 GetPlayerData 를 실행시키도록
    // 명령한다. 

    [System.Serializable]
    public class UpdateStatsRequest
    {
        public int playerId;
        public int currency;
        public int rating;

        public UpdateStatsRequest(int id, int currency, int rating)
        {
            this.playerId = id;
            this.currency = currency;
            this.rating = rating;
        }
    }

}
