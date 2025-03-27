using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameEnding : NetworkBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMPro.TextMeshProUGUI resultText;
    public Managers Managers;

    public static int LastWinnerId { get; private set; }
    public static int LastLoserId { get; private set; }

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

        DetermineWinner(out int winnerId, out int loserId);

        // 전역 변수 설정
        LastWinnerId = winnerId;
        LastLoserId = loserId;

        // 클라이언트에 결과 전달
        ShowGameOverScreenClientRpc(winnerId, loserId);

        //여기에 승자를 제출하는 것을 만들면 됨

        SubmitWinnerToDB(winnerId, loserId);

        NotifyClientsToFetchDataClientRpc();

        // 5초 후 로비 이동
        Invoke(nameof(GoToLobby), 5f);
    }

    /// 승자/패자 판단 (2명 전용)
    private void DetermineWinner(out int winnerPlayerId, out int loserPlayerId)
    {
        winnerPlayerId = -1;
        loserPlayerId = -1;

        var scores = ScoreManager.Instance.GetScores();
        var playerDataManager = PlayerDataManager.Instance;

        if (scores.Count == 1)
        {
            // 혼자 플레이 중일 때: 승자만 지정하고 패자는 의미 없는 값
            var onlyPlayer = scores.First();
            winnerPlayerId = playerDataManager.GetNumberFromClientID(onlyPlayer.Key);
            loserPlayerId = -1; // 또는 999 등 임시 값

            Debug.Log($"✅ [1인 플레이] Winner: {winnerPlayerId}, Loser: 없음");
            return;
        }
        //2인 이하 플레이는 scores.Count 2이하로 해야될듯

        var sorted = scores.OrderByDescending(p => p.Value).ToList();
        ulong winnerClientId = sorted[0].Key;
        ulong loserClientId = sorted[1].Key;

        winnerPlayerId = playerDataManager.GetNumberFromClientID(winnerClientId);
        //loserPlayerId = playerDataManager.GetNumberFromClientID(loserClientId);

        Debug.Log($"✅ Winner: {winnerPlayerId}, ❌ Loser: {loserPlayerId}");
    }

    /// 게임 결과 UI 표시 (클라이언트)
    [ClientRpc]
    private void ShowGameOverScreenClientRpc(int winnerPlayerId, int loserPlayerId)
    {
        gameOverPanel.SetActive(true);

        int myId = SQLiteManager.Instance.player.playerId;

        if (myId == winnerPlayerId)
        {
            resultText.text = "Winner!";
        }
        else if (myId == loserPlayerId)
        {
            resultText.text = "Loser...";
        }
        else
        {
            resultText.text = "Draw?";
        }
    }

    ///-----------------------------------------------------------------서버 전송부분----------------------------------------------------------

    private void SubmitWinnerToDB(int winnerID, int LoserID)
    {
        if(Managers == null)
        {
            Debug.Log("참조가 없습니다");
        }
        Debug.Log("서버에 승자 제출");
        StartCoroutine(Managers.AddMatchResult(winnerID, 3));
        StartCoroutine(Managers.UpdateCurrencyAndRating(winnerID, 100, 10));
        StartCoroutine(Managers.UpdateCurrencyAndRating(3, 10, -10));
    }





    ///-----------------------------------------------------------------클라로 전송부분---------------------------------------------------------- 
    [ClientRpc]
    private void NotifyClientsToFetchDataClientRpc()
    {
        Debug.Log("클라에서 DB로 데이터 업데이트 요청 성공");
        ClientNetworkManager.Instance.GetMatchRecords(SQLiteManager.Instance.player.playerId);
        ClientNetworkManager.Instance.GetPlayerStats(SQLiteManager.Instance.player.playerId);
        ClientNetworkManager.Instance.GetPlayerData("playerId", SQLiteManager.Instance.player.playerId.ToString(), false);

    }

    ///--------------------------------------추후에 로딩씬으로 넘기고 나중에 바꾸기<로딩씬으로>--------------------------------------------------------



    /// 
    private void GoToLobby()
    {

        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);

    }
}
