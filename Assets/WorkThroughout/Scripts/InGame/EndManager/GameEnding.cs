using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class GameEnding : NetworkBehaviour
{
    public static GameEnding Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject extendPanel;
    private bool hasExtendedOnce = false;
    private bool resultIsDraw = false;
    public Managers Managers; // DB 등 외부 매니저

    private bool hasFinalGameBeenHandled = false; // 최종 게임 종료 처리 여부

    public enum GameResultType
    {
        Win,     // 승패 있음
        Draw,    // 무승부 (연장 후)
        Extend   // 무승부 (연장 필요)
    }

    public static int LastWinnerId { get; private set; }
    public static int LastLoserId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        GameTimer.OnGameEnded += OnGameEndedHandler;
    }

    private void OnDisable()
    {
        GameTimer.OnGameEnded -= OnGameEndedHandler;
    }

    // OnGameEndedHandler에서 연장 상태이면 최종 처리 없이 무시함
    private void OnGameEndedHandler()
    {
        // 만약 현재 GameTimer가 연장 모드라면, 최종 로직을 진행하지 않음
        if (GameTimer.Instance != null && GameTimer.Instance.IsInExtension)
        {
            Debug.Log("연장 중이므로 OnGameEndedHandler 실행 무시");
            return;
        }

        StartCoroutine(HandleGameEnd());
    }

    /// <summary>
    /// 서버에서 게임 종료 처리
    /// </summary>
    private IEnumerator HandleGameEnd()
    {
        if (hasFinalGameBeenHandled)
        {
            yield break;
        }

        var result = DetermineWinner(out int winnerId, out int loserId, out int winnerRating, out int loserRating);


        if (result == GameResultType.Extend)
        {
            Debug.Log("게임 연장 처리됨 → 종료 중단");
            yield break;
        }

        hasFinalGameBeenHandled = true;
        LastWinnerId = winnerId;
        LastLoserId = loserId;

        if (result == GameResultType.Draw)
        {
            yield return SubmitDrawToDB(winnerId, loserId, winnerRating, loserRating);
        }
        else if (result == GameResultType.Win)
        {
            yield return SubmitWinnerToDB(winnerId, loserId, winnerRating, loserRating);
        }

        NotifyClientsToFetchDataClientRpc();
        ShutdownNetworkObject();
    }

    public GameResultType DetermineWinner(
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
            Debug.LogWarning("플레이어 없음");
            return GameResultType.Win;
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
                Debug.Log("연장 후 무승부 처리");
                winnerId = playerDataManager.GetNumberFromClientID(sorted[0].Key);
                loserId = playerDataManager.GetNumberFromClientID(sorted[1].Key);
                winnerRating = playerDataManager.GetRatingFromClientID(sorted[0].Key);
                loserRating = playerDataManager.GetRatingFromClientID(sorted[1].Key);
                return GameResultType.Draw;
            }

            hasExtendedOnce = true;
            Debug.Log("무승부 → 15초 연장");
            StartCoroutine(HandleGameTimeExtension());
            return GameResultType.Extend;
        }

        resultIsDraw = false;
        winnerId = playerDataManager.GetNumberFromClientID(sorted[0].Key);
        loserId = playerDataManager.GetNumberFromClientID(sorted[1].Key);
        winnerRating = playerDataManager.GetRatingFromClientID(sorted[0].Key);
        loserRating = playerDataManager.GetRatingFromClientID(sorted[1].Key);
        return GameResultType.Win;
    }

    //-------------------------------------- 게임시간 15초 연장  ---------------------------------------
    private IEnumerator HandleGameTimeExtension()
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

        yield return new WaitForSeconds(2f);

        if (extendPanel != null)
            extendPanel.SetActive(false);
    }
    //---------------------------------------------------------------------------------------------

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
            result = "Draw!";
            finalRating += ratingDelta;
            finalGold += winnerGold;
        }
        else
        {
            result = "Unknown";
        }

        string ratingLine = $"Rating: {myRating} → {finalRating}  ({(ratingDelta >= 0 ? "+" : "")}{(isDraw ? ratingDelta : (isWinner ? ratingDelta : -ratingDelta))})";
        string goldLine = $"Gold: {myCurrency} → {finalGold}  (+{(isDraw ? winnerGold : (isWinner ? winnerGold : (isLoser ? loserGold : 0)))})";

        resultText.text = $"{result}\n{ratingLine}\n{goldLine}";
    }

    ///------------------------------ 서버로 결과 전송 -----------------------------------
    private IEnumerator SubmitWinnerToDB(int winnerID, int loserID, int winnerRating, int loserRating)
    {
        Debug.Log("서버에 승자 제출");


        //문제인 DB 결과제출
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

    [ClientRpc]
    private void NotifyClientsToFetchDataClientRpc()
    {
        Debug.Log("클라에서 DB 데이터 업데이트 요청 성공");
        StartCoroutine(ClientNetworkManager.Instance.GetMatchRecords(SQLiteManager.Instance.player.playerId));
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerStats(SQLiteManager.Instance.player.playerId));
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerData("playerId", SQLiteManager.Instance.player.playerId.ToString(), false));
    }

    private int CalculateRatingDelta(int winnerRating, int loserRating)
    {
        int ratingGap = Math.Abs(winnerRating - loserRating);

        if (ratingGap >= 200) return 10;
        if (ratingGap >= 100) return 15;
        if (ratingGap >= 0) return 20;

        return 30;
    }

    private void ShutdownNetworkObject()
    {
        Debug.Log("서버 종료");
        NetworkManager.Singleton.Shutdown();
    }
}
