using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Services.Matchmaker.Models;
#if UNITY_SEREVR
using Unity.Services.Multiplay;
#endif
using Unity.Services.Authentication;
using System.Collections;

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
        DetermineWinner(out int winnerId, out int loserId, out int winnerRating, out int loserRating);

        LastWinnerId = winnerId;
        LastLoserId = loserId;


        SubmitWinnerToDB(winnerId, loserId, winnerRating, loserRating);


        NotifyClientsToFetchDataClientRpc();

        ShutdownNetworkObject();
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

        if (scores.Count == 0)
        {
            Debug.LogWarning("❌ 플레이어가 없습니다.");
            return;
        }

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

        Debug.Log($"Winner: {winnerPlayerId} (R: {winnerRating}), ❌ Loser: {loserPlayerId} (R: {loserRating})");
    }

    /// 게임 결과 UI 표시 (클라이언트)
    [ClientRpc]
    private void ShowGameOverScreenClientRpc(
     int winnerPlayerId,
     int loserPlayerId,
     int ratingDelta,
     int winnerGold,
     int loserGold)
    {
        gameOverPanel.SetActive(true);

        int myId = SQLiteManager.Instance.player.playerId;
        int myRating = SQLiteManager.Instance.player.rating;
        int myCurrency = SQLiteManager.Instance.player.currency;

        string result;
        int finalRating = myRating;
        int finalGold = myCurrency;

        bool isWinner = (myId == winnerPlayerId);
        bool isLoser = (myId == loserPlayerId);

        if (isWinner)
        {
            result = "🏆 Winner!";
            finalRating += ratingDelta;
            finalGold += winnerGold;
        }
        else if (isLoser)
        {
            result = "❌ Loser...";
            finalRating -= ratingDelta;
            finalGold += loserGold;
        }
        else
        {
            result = "Draw?";
        }

        string ratingLine = $"Rating: {myRating} → {finalRating}  ({(isWinner ? "+" : "-")}{ratingDelta})";
        string goldLine = $"Gold: {myCurrency} → {finalGold}  (+{(isWinner ? winnerGold : (isLoser ? loserGold : 0))})";

        resultText.text = $"{result}\n{ratingLine}\n{goldLine}";
    }

    ///-----------------------------------------------------------------서버 전송부분----------------------------------------------------------

    private void SubmitWinnerToDB(int winnerID, int loserID, int winnerRating, int loserRating)
    {

        Debug.Log("서버에 승자 제출");

        StartCoroutine(Managers.AddMatchResult(winnerID, loserID));

        int winnerGold = 100 + UnityEngine.Random.Range(0, 91);
        int loserGold = UnityEngine.Random.Range(0, 91);

        int ratingDelta = CalculateRatingDelta(winnerRating, loserRating);

        StartCoroutine(Managers.UpdateCurrencyAndRating(winnerID, winnerGold, ratingDelta));
        StartCoroutine(Managers.UpdateCurrencyAndRating(loserID, loserGold, -ratingDelta));


        ShowGameOverScreenClientRpc(winnerID, loserID, ratingDelta, winnerGold, loserGold);

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




    //-----------------------------------------------점수처리함수 목록------------------------------------------------------------------

    int CalculateRatingDelta(int winnerRating, int loserRating)
    {
        int ratingGap = Math.Abs(winnerRating - loserRating);

        // 절대값 기준 레이팅 델타
        if (ratingGap >= 200) return 10;
        if (ratingGap >= 100) return 15;
        if (ratingGap >= 0) return 20;

        return 30;
    }

    //-------------------------------------------------------ServerShutDown---------------------------------------------------------------
    //어려웠던 부분


    private void ShutdownNetworkObject()
    {
        Debug.Log("서버 종료");
        NetworkManager.Singleton.Shutdown();
    }

    //------------------------------------------------------------------------------------ DB로 결과 조정 순차적 진행을 위해 코루틴.








}
