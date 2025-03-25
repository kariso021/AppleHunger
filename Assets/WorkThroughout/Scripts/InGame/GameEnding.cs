using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameEnding : NetworkBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMPro.TextMeshProUGUI resultText;

    private void OnEnable()
    {
        GameTimer.OnGameEnded += HandleGameEnd;
    }

    private void OnDisable()
    {
        GameTimer.OnGameEnded -= HandleGameEnd;
    }

    /// 서버에서 게임 종료 처리
    private void HandleGameEnd()
    {
        if (!IsServer) return;
        DetermineWinner(out int winnerNumber, out List<int> loserNumbers);

        // 모든 클라이언트에게 게임 결과 전송
        ShowGameOverScreenClientRpc(winnerNumber, loserNumbers.ToArray());

        // 서버 DB 또는 기록 처리 (선택적으로) -> 이부분은 클라에서 결정해야되는 것.
        SubmitMatchResultServerRpc(winnerNumber, loserNumbers.ToArray());

        // 5초 후 로비 씬 이동
        Invoke(nameof(GoToLobby), 5f);
    }

    /// 서버에서 승자 계산
    private void DetermineWinner(out int winnerPlayerId, out List<int> loserPlayerIds)
    { 
        winnerPlayerId = -1;
        loserPlayerIds = new List<int>();

        var scores = ScoreManager.Instance.GetScores();
        var playerDataManager = PlayerDataManager.Instance;

        int highestScore = int.MinValue;
        ulong winnerClientId = 0;

        Debug.Log("🧾 [ScoreManager] 현재 점수 목록:");
        foreach (var pair in scores)
        {
            int playerId = playerDataManager.GetNumberFromClientID(pair.Key);
            Debug.Log($"🟡 ClientID: {pair.Key} → PlayerID: {playerId}, Score: {pair.Value}");
        }

        foreach (var pair in scores)
        {
            if (pair.Value > highestScore)
            {
                highestScore = pair.Value;
                winnerClientId = pair.Key;
            }
        }

        winnerPlayerId = playerDataManager.GetNumberFromClientID(winnerClientId);

        Debug.Log($"✅ [결과] 승자 ClientID: {winnerClientId} → PlayerID: {winnerPlayerId}, Score: {highestScore}");

        foreach (var pair in scores)
        {
            if (pair.Key != winnerClientId)
            {
                int loserPlayerId = playerDataManager.GetNumberFromClientID(pair.Key);
                loserPlayerIds.Add(loserPlayerId);
                Debug.Log($"❌ 패자 ClientID: {pair.Key} → PlayerID: {loserPlayerId}");
            }
        }
    }

    [ClientRpc]
    private void ShowGameOverScreenClientRpc(int winnerPlayerId, int[] loserPlayerIds)
    {
        gameOverPanel.SetActive(true);

        int myPlayerId = SQLiteManager.Instance.player.playerId;

        if (myPlayerId == winnerPlayerId)
        {
            resultText.text = "Winner!";
        }
        else if (System.Array.Exists(loserPlayerIds, id => id == myPlayerId))
        {
            resultText.text = "Loser...";
        }
        else
        {
            resultText.text = "Draw";
        }
    }



    //이게 지금 되지 않고있는것. ServerRPC는 player 객체 또는 프리펩에서 호출되어야 정상적으로 작동
    /// 서버에 Match 결과 제출 요청 (클라이언트에서 호출)
    [ServerRpc(RequireOwnership = false)]
    private void SubmitMatchResultServerRpc(int winnerPlayerId, int[] loserPlayerIds)
    {

        if (ClientNetworkManager.Instance == null)
        {
            Debug.Log("Client 네트워크매니저가 현재 없습니다");
        }
        else
        {
            Debug.Log("클라네트워크 매니저 존재하고 작동하고 있음");
        }
        ClientNetworkManager.Instance?.AddMatchRecords(winnerPlayerId, 3);
        SavePlayerDataClientRpc(winnerPlayerId, NetworkManager.Singleton.ConnectedClientsIds.ToArray());
    }



    /// 각 클라이언트에게 DB 저장 요청
    [ClientRpc]
    private void SavePlayerDataClientRpc(int winnerPlayerId, ulong[] targetClientIds)
    {
        ulong myClientId = NetworkManager.Singleton.LocalClientId;

        if (!System.Array.Exists(targetClientIds, id => id == myClientId))
            return;

        int myPlayerId = SQLiteManager.Instance.player.playerId;

        if (myPlayerId == winnerPlayerId)
        {
            SQLiteManager.Instance.player.currency += (100 + UnityEngine.Random.Range(10, 90));
        }

        SQLiteManager.Instance.SavePlayerData(SQLiteManager.Instance.player);
        ClientNetworkManager.Instance?.UpdatePlayerData();
    }

    /// 씬 이동 (서버/클라 공통)
    private void GoToLobby()
    {
        // 서버가 전환하고, 클라이언트도 자동 따라옴
        if (IsClient)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
    }
}
