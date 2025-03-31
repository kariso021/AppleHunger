using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Services.Matchmaker.Models;

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

        DetermineWinner(out int winnerId, out int loserId,out int winnerRating, out int loserRating);

        // 전역 변수 설정
        LastWinnerId = winnerId;
        LastLoserId = loserId;

        // 클라이언트에 결과 전달
        ShowGameOverScreenClientRpc(winnerId, loserId);

        //여기에 승자를 제출하는 것을 만들면 됨

        SubmitWinnerToDB(winnerId, loserId, winnerRating, loserRating);

        NotifyClientsToFetchDataClientRpc();

        // 5초 후 로비 이동
        Invoke(nameof(GoToLobby), 5f);
    }

    private void DetermineWinner(
    out int winnerPlayerId, out int loserPlayerId,
    out int winnerRating, out int loserRating)
    {
        winnerPlayerId = -1;
        loserPlayerId = -1;
        winnerRating = 0;
        loserRating = 0;

        var scores = ScoreManager.Instance.GetScores();
        var playerDataManager = PlayerDataManager.Instance;

        if (scores.Count == 1)
        {
            var onlyPlayer = scores.First();
            winnerPlayerId = playerDataManager.GetNumberFromClientID(onlyPlayer.Key);
            winnerRating = playerDataManager.GetRatingFromClientID(onlyPlayer.Key);
            loserPlayerId = -1;
            loserRating = 0;
            return;
        }

        var sorted = scores.OrderByDescending(p => p.Value).ToList();
        ulong winnerClientId = sorted[0].Key;
        ulong loserClientId = sorted[1].Key;

        winnerPlayerId = playerDataManager.GetNumberFromClientID(winnerClientId);
        loserPlayerId = playerDataManager.GetNumberFromClientID(loserClientId);
        winnerRating = playerDataManager.GetRatingFromClientID(winnerClientId);
        loserRating = playerDataManager.GetRatingFromClientID(loserClientId);

        Debug.Log($"✅ Winner: {winnerPlayerId} (R: {winnerRating}), ❌ Loser: {loserPlayerId} (R: {loserRating})");
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

    private void SubmitWinnerToDB(int winnerID, int loserID, int winnerRating, int loserRating)
    {
        if (Managers == null)
        {
            Debug.Log("❌ Managers 참조가 없습니다");
            return;
        }

        Debug.Log("서버에 승자 제출");

        StartCoroutine(Managers.AddMatchResult(winnerID, loserID));

        int winnerGold = 100 + Random.Range(0, 91);
        int loserGold = Random.Range(0, 91);

        int ratingDelta = CalculateRatingDelta(winnerRating, loserRating);

        StartCoroutine(Managers.UpdateCurrencyAndRating(winnerID, winnerGold, ratingDelta));
        StartCoroutine(Managers.UpdateCurrencyAndRating(loserID, loserGold, -ratingDelta));
    }





    ///-----------------------------------------------------------------클라로 전송부분---------------------------------------------------------- 
    [ClientRpc]
    private void NotifyClientsToFetchDataClientRpc()
    {
        Debug.Log("클라에서 DB로 데이터 업데이트 요청 성공");
        StartCoroutine(ClientNetworkManager.Instance.GetMatchRecords(SQLiteManager.Instance.player.playerId));
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerStats(SQLiteManager.Instance.player.playerId));
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerData("playerId", SQLiteManager.Instance.player.playerId.ToString(), false));

    }

    ///--------------------------------------추후에 로딩씬으로 넘기고 나중에 바꾸기<로딩씬으로>--------------------------------------------------------



    /// 
    private void GoToLobby()
    {

        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);

    }

    //-----------------------------------------------점수처리함수 목록------------------------------------------------------------------

    int CalculateRatingDelta(int winnerRating, int loserRating)
    {
        int ratingGap = winnerRating - loserRating;
        

        //임시 레이팅으로 만든 부분
        if (ratingGap >= 200) return 10;
        if (ratingGap >= 100) return 15;
        if (ratingGap >= 0) return 20;
        if (ratingGap >= -100) return 25;
        return 30;
    }
}
