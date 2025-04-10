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
    [SerializeField] private GameObject extendPanel;
    private bool hasExtendedOnce = false;
    private bool resultIsDraw = false;
    public Managers Managers;

    private enum GameResultType
    {
        Win,     // 승패 있음
        Draw,    // 무승부 (연장 후)
        Extend   // 무승부 (연장 필요)
    }

    public static int LastWinnerId { get; private set; }
    public static int LastLoserId { get; private set; }

    private void OnEnable()
    {
        GameTimer.OnGameEnded += OnGameEndedHandler;
    }

    private void OnDisable()
    {
        GameTimer.OnGameEnded -= OnGameEndedHandler;
    }

    private void OnGameEndedHandler()
    {
        StartCoroutine(HandleGameEnd());
    }

    /// 서버에서 게임 종료 처리
    private IEnumerator HandleGameEnd()
    {
        var result = DetermineWinner(out int winnerId, out int loserId, out int winnerRating, out int loserRating);

        if (result == GameResultType.Extend)
        {
            Debug.Log("⏸ 게임 연장 처리됨 → 종료 중단");
            yield break;
        }

        LastWinnerId = winnerId;
        LastLoserId = loserId;

        if (result == GameResultType.Draw)
        {
            yield return SubmitDrawToDB(winnerId,loserId,winnerRating,loserRating);
        }
        else if (result == GameResultType.Win)
        {
            yield return SubmitWinnerToDB(winnerId, loserId, winnerRating, loserRating);
        }

        NotifyClientsToFetchDataClientRpc();
        ShutdownNetworkObject();
    }


    private GameResultType DetermineWinner(
    out int winnerId,
    out int loserId,
    out int winnerRating,
    out int loserRating)
    {
        winnerId = -1;
        loserId = -1;
        winnerRating = 0;
        loserRating = 0;

        var scores = ScoreManager.Instance.GetScores();
        var playerDataManager = PlayerDataManager.Instance;

        if (scores.Count == 0)
        {
            Debug.LogWarning("❌ 플레이어 없음");
            return GameResultType.Win; // 그냥 종료
        }

        if (scores.Count == 1)
        {
            var only = scores.First();
            winnerId = playerDataManager.GetNumberFromClientID(only.Key);
            winnerRating = playerDataManager.GetRatingFromClientID(only.Key);
            return GameResultType.Win;
        }

        var sorted = scores.OrderByDescending(s => s.Value).ToList();
        int topScore = sorted[0].Value;
        int secondScore = sorted[1].Value;

        if (topScore == secondScore)
        {
            if (hasExtendedOnce)
            {
                resultIsDraw = true;
                Debug.Log("🤝 연장 후 무승부 처리");
                winnerId = playerDataManager.GetNumberFromClientID(sorted[0].Key);
                loserId = playerDataManager.GetNumberFromClientID(sorted[1].Key);
                winnerRating = playerDataManager.GetRatingFromClientID(sorted[0].Key);
                loserRating = playerDataManager.GetRatingFromClientID(sorted[1].Key);
                return GameResultType.Draw;
            }

            hasExtendedOnce = true;
            Debug.Log("🔁 무승부 → 15초 연장");
            StartCoroutine(HandleGameTimeExtension(15f));
            return GameResultType.Extend;
        }

        // 정상 승패 처리
        resultIsDraw = false;
        winnerId = playerDataManager.GetNumberFromClientID(sorted[0].Key);
        loserId = playerDataManager.GetNumberFromClientID(sorted[1].Key);
        winnerRating = playerDataManager.GetRatingFromClientID(sorted[0].Key);
        loserRating = playerDataManager.GetRatingFromClientID(sorted[1].Key);
        return GameResultType.Win;
    }


    //--------------------------------------게임시간 15초 연장-------------------------------------------------------------------------------------
    private void ExtendGameTime(float v)
    {
        StartCoroutine(HandleGameTimeExtension(v));
    }

    [ClientRpc]
    private void NotifyClientsToExtendGameTimeClientRpc()
    {
        StartCoroutine(ShowExtendMessageCoroutine());
    }

    private IEnumerator ShowExtendMessageCoroutine()
    {
        if (extendPanel != null)
            extendPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text = "15초 연장!";
            resultText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2f);


        if (extendPanel != null)
            extendPanel.SetActive(false);
    }

    private IEnumerator HandleGameTimeExtension(float extraSeconds)
    {

        NotifyClientsToExtendGameTimeClientRpc();


        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent(out PlayerController pc))
            {
                pc.RestrictDragOnlyClientRpc();
            }
        }

 
        yield return new WaitForSeconds(2f);


        if (IsServer && GameTimer.Instance != null)
            GameTimer.Instance.ExtendTime(extraSeconds);
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

        bool isWinner = (myId == winnerPlayerId && winnerPlayerId != loserPlayerId);
        bool isLoser = (myId == loserPlayerId && winnerPlayerId != loserPlayerId);
        bool isDraw = (winnerPlayerId == loserPlayerId && myId == winnerPlayerId);

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
        else if (isDraw)
        {
            result = " Draw!";
            finalRating += ratingDelta;
            finalGold += winnerGold; // winnerGold를 무승부 보상으로 재사용
        }
        else
        {
            result = "Unknown";
        }

        string ratingLine = $"Rating: {myRating} → {finalRating}  ({(ratingDelta >= 0 ? "+" : "")}{(isDraw ? ratingDelta : (isWinner ? ratingDelta : -ratingDelta))})";
        string goldLine = $"Gold: {myCurrency} → {finalGold}  (+{(isDraw ? winnerGold : (isWinner ? winnerGold : (isLoser ? loserGold : 0)))})";

        resultText.text = $"{result}\n{ratingLine}\n{goldLine}";
    }

    ///-----------------------------------------------------------------서버 전송부분----------------------------------------------------------

    private IEnumerator SubmitWinnerToDB(int winnerID, int loserID, int winnerRating, int loserRating)
    {

        Debug.Log("서버에 승자 제출");

        yield return StartCoroutine(Managers.AddMatchResult(winnerID, loserID));

        int winnerGold = 100 + UnityEngine.Random.Range(0, 91);
        int loserGold = UnityEngine.Random.Range(0, 91);

        int ratingDelta = CalculateRatingDelta(winnerRating, loserRating);

        yield return StartCoroutine(Managers.UpdateCurrencyAndRating(winnerID, winnerGold, ratingDelta));
        yield return StartCoroutine(Managers.UpdateCurrencyAndRating(loserID, loserGold, -ratingDelta));




        ShowGameOverScreenClientRpc(winnerID, loserID, ratingDelta, winnerGold, loserGold);

    }

    private IEnumerator SubmitDrawToDB(int ID1, int ID2, int ID1Rating, int ID2Rating)
    {
        Debug.Log("🤝 무승부 → 서버에 결과 제출");

        int DrawGold = 50 + UnityEngine.Random.Range(0, 91);

        yield return StartCoroutine(Managers.UpdateCurrencyAndRating(ID1, DrawGold, 0));
        yield return StartCoroutine(Managers.UpdateCurrencyAndRating(ID2, DrawGold, 0));

        ShowGameOverScreenClientRpc(ID1, ID2, DrawGold, DrawGold, DrawGold);
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


