using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Matchmaker.Models;

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
        DetermineWinner(out int winnerNumber, out List<int> loserNumbers);
        ShowGameOverScreenClientRpc(winnerNumber, loserNumbers.ToArray());
    }

    private void DetermineWinner(out int winnerNumber, out List<int> loserNumbers)
    {
        winnerNumber = -1;
        loserNumbers = new List<int>();

        Dictionary<ulong, int> scores = ScoreManager.Instance.GetScores();

        int highestScore = int.MinValue;
        ulong winnerClientId = 0;

        foreach (var pair in scores)
        {
            if (pair.Value > highestScore)
            {
                highestScore = pair.Value;
                winnerClientId = pair.Key;
            }
        }

        winnerNumber = PlayerDataManager.Instance.GetNumberFromClientID(winnerClientId);

        foreach (var pair in scores)
        {
            ulong clientId = pair.Key;
            if (clientId != winnerClientId)
            {
                int loserNumber = PlayerDataManager.Instance.GetNumberFromClientID(clientId);
                loserNumbers.Add(loserNumber);
            }
        }
    }




    [ClientRpc]
    private void ShowGameOverScreenClientRpc(int winnerNumber, int[] loserNumbers)
    {
        gameOverPanel.SetActive(true);

        int myNumber = SQLiteManager.Instance.player.playerId; // ✅ 내 DB ID

        if (myNumber == winnerNumber)
        {
            resultText.text = "Winner!";
        }
        else if (System.Array.Exists(loserNumbers, number => number == myNumber))
        {
            resultText.text = "Loser...";
        }
        else
        {
            resultText.text = "???";
        }

        //여기서 결정하면 됨


        ClientNetworkManager.Instance?.AddMatchRecords(winnerNumber, loserNumbers[0]);
        SQLiteManager.Instance.player.currency += (100 + UnityEngine.Random.Range(10, 90));
        SQLiteManager.Instance.SavePlayerData(SQLiteManager.Instance.player);
        ClientNetworkManager.Instance?.UpdatePlayerData();

        //Invoke(nameof(GoToLobby), 5f);
    }



    // 5초 후 로비 씬으로 이동
    private void GoToLobby()
    {
        
        NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("Lobby");
    }

    // 업데이트 레이팅 부분

    //public void UpdateRatings(Player playerA, Player playerB, bool playerAWon, float K, float R)
    //{
    //    float diff = playerB.Rating - playerA.Rating;

    //    if (playerAWon)
    //    {
    //        float gain = K + diff * 0.3f;
    //        float loss = K - diff * 0.1f;

    //        gain = Mathf.Clamp(gain, K, R);
    //        loss = Mathf.Clamp(loss, K * 0.25f, R);

    //        playerA.Rating += gain;
    //        playerB.Rating -= loss;
    //    }
    //    else
    //    {
    //        float gain = K + (-diff) * 0.3f; // B가 더 낮은 경우
    //        float loss = K - (-diff) * 0.1f;

    //        gain = Mathf.Clamp(gain, K, R);
    //        loss = Mathf.Clamp(loss, K * 0.25f, R);

    //        playerB.Rating += gain;
    //        playerA.Rating -= loss;
    //    }

    //    Debug.Log($"[Rating Update] A: {playerA.Rating}, B: {playerB.Rating}");
    //}
}
