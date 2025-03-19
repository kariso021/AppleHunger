using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameEnding : NetworkBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMPro.TextMeshProUGUI resultText;

    private void OnEnable()
    {
      
       
            Debug.Log("게임이벤트 등록 호출");
            GameTimer.OnGameEnded += HandleGameEnd;
        
    }

    private void OnDisable()
    {
 
        
            GameTimer.OnGameEnded -= HandleGameEnd;
        
    }

    /// ✅ 게임 종료 시 승자 결정 및 UI 표시
    private void HandleGameEnd()
    {
        Debug.Log("[GameEnding] HandleGameEnd 실행됨!");
        ulong winnerId = DetermineWinner();
        ShowGameOverScreenClientRpc(winnerId);
    }

    /// ✅ 서버에서 승자 결정 (점수 기반)
    private ulong DetermineWinner()
    {
        ulong winnerId = 0;
        int highestScore = int.MinValue;
        Dictionary<ulong, int> playerScores = ScoreManager.Instance.GetScores();

        foreach (var player in playerScores)
        {
            if (player.Value > highestScore)
            {
                highestScore = player.Value;
                winnerId = player.Key;
            }
        }

        return winnerId;
    }

    /// ✅ 클라이언트 UI에 결과 표시
    [ClientRpc]
    private void ShowGameOverScreenClientRpc(ulong winnerId)
    {
        gameOverPanel.SetActive(true);

        if (NetworkManager.Singleton.LocalClientId == winnerId)
        {
            resultText.text = "Winner!";
        }
        else
        {
            resultText.text = "Loser...";
        }

        //Invoke(nameof(GoToLobby), 5f);
    }

    /// ✅ 5초 후 로비 씬으로 이동
    private void GoToLobby()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("LobbyScene");
    }
}
