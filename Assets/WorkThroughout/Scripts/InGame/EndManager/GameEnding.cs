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

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject extendPanel;

    [Header("Config")]
    [SerializeField] private int extensionSeconds = 15;
    [SerializeField] private int extendNoticeDuration = 2;

    public Managers Managers; // DB/API 호출 매니저

    private bool hasExtendedOnce = false;
    private bool hasFinalGameBeenHandled = false;

    public enum GameResultType { Win, Draw, Extend }

    public static int LastWinnerId { get; private set; }
    public static int LastLoserId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnEnable() => GameTimer.OnGameEnded += OnGameEndedHandler;
    private void OnDisable() => GameTimer.OnGameEnded -= OnGameEndedHandler;

    private void OnGameEndedHandler()
    {
        if (GameTimer.Instance != null && GameTimer.Instance.IsInExtension)
        {
            Debug.Log("연장 중 - 처리 생략");
            return;
        }
        StartCoroutine(HandleGameEnd());
    }

    private IEnumerator HandleGameEnd()
    {
        if (hasFinalGameBeenHandled) yield break;

        // playerId→score 로 변경
        var pidScores = ScoreManager.Instance.GetAllScores();
        var (type, winnerpid, loserpid) = EvaluateScoresByPlayer(pidScores);

        if (type == GameResultType.Extend)
        {
            Debug.Log("무승부 연장");
            yield break;
        }

        hasFinalGameBeenHandled = true;

        // clientId → playerId/rating
        LastWinnerId = winnerpid;
        LastLoserId = loserpid;

        var pdm = PlayerDataManager.Instance;

        int winnerRating = pdm.GetPlayerRating(winnerpid);
        int loserRating = pdm.GetPlayerRating(loserpid);
        // 세션 false
        yield return pdm.UpdateAllSessionsFalse();
     

        // DB 제출 & 보상 계산
        yield return SubmitResultToDB(type, winnerpid, loserpid, winnerRating, loserRating);

        // 클라이언트 UI 갱신 요청
        NotifyClientsToFetchDataClientRpc();

        // 서버 종료
        ShutdownNetwork();
    }

    // 점수 비교 후 결과 및 해당 clientIds 반환
    public (GameResultType type, int winnerPid, int loserPid) EvaluateScoresByPlayer(
     Dictionary<int, int> scores)
    {
        if (scores.Count < 2)
        {
            int onlyPid = scores.Keys.FirstOrDefault();
            return (GameResultType.Win, onlyPid, onlyPid);
        }

        var sorted = scores.OrderByDescending(kv => kv.Value).ToList();
        int topScore = sorted[0].Value;
        int secondScore = sorted[1].Value;
        int topPid = sorted[0].Key;
        int secondPid = sorted[1].Key;

        if (topScore == secondScore)
        {
            if (!hasExtendedOnce)
            {
                hasExtendedOnce = true;
                StartCoroutine(ExtendGameTime());
                return (GameResultType.Extend, topPid, secondPid);
            }
            // 연장 후에도 동점 → 무승부
            return (GameResultType.Draw, topPid, secondPid);
        }

        // 승패 확정
        return (GameResultType.Win, topPid, secondPid);
    }

    private IEnumerator ExtendGameTime()
    {
        // 서버→클라 연장 알림
        NotifyClientsToExtendGameTimeClientRpc();

        // 클라이언트 조작 제한
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            if (client.PlayerObject.TryGetComponent(out PlayerController pc))
                pc.RestrictDragOnlyClientRpc();

        yield return new WaitForSeconds(extendNoticeDuration);
    }

    [ClientRpc]
    private void NotifyClientsToExtendGameTimeClientRpc()
        => StartCoroutine(ShowExtendPanel());

    private IEnumerator ShowExtendPanel()
    {
        extendPanel.SetActive(true);
        yield return new WaitForSeconds(extendNoticeDuration);
        extendPanel.SetActive(false);
    }

    // DB 제출＋보상 계산 공통화
    private IEnumerator SubmitResultToDB(
        GameResultType type,
        int winId, int loseId,
        int winRating, int loseRating)
    {
        if (type == GameResultType.Draw)
        {
            Debug.Log("무승부 처리");
            int gold = UnityEngine.Random.Range(50, 141);
            yield return StartCoroutine(Managers.UpdateCurrencyAndRating(winId, gold, 0));
            yield return StartCoroutine(Managers.UpdateCurrencyAndRating(loseId, gold, 0));
            ShowGameOverScreenClientRpc(winId, loseId, 0, gold, gold);
        }
        else // Win
        {
            Debug.Log("승패 처리");
            yield return StartCoroutine(Managers.AddMatchResult(winId, loseId));
            int winGold = UnityEngine.Random.Range(100, 191);
            int loseGold = UnityEngine.Random.Range(0, 91);
            int delta = CalculateRatingDelta(winRating, loseRating);

            yield return StartCoroutine(Managers.UpdateCurrencyAndRating(winId, winGold, delta));
            yield return StartCoroutine(Managers.UpdateCurrencyAndRating(loseId, loseGold, -delta));
            ShowGameOverScreenClientRpc(winId, loseId, delta, winGold, loseGold);
        }
    }

    [ClientRpc]
    private void ShowGameOverScreenClientRpc(
        int winnerPlayerId,
        int loserPlayerId,
        int ratingDelta,
        int winnerGold,
        int loserGold)
    {
        gameOverPanel.SetActive(true);

        var player = SQLiteManager.Instance.player;
        bool isWinner = player.playerId == winnerPlayerId && winnerPlayerId != loserPlayerId;
        bool isLoser = player.playerId == loserPlayerId && winnerPlayerId != loserPlayerId;
        bool isDraw = winnerPlayerId == loserPlayerId && player.playerId == winnerPlayerId;

        string title;
        if (isWinner) title = "🏆 Winner!";
        else if (isLoser) title = "❌ Loser...";
        else if (isDraw) title = "🤝 Draw!";
        else title = "Unknown";

        int finalRating = player.rating + (isWinner ? ratingDelta : isDraw ? 0 : -ratingDelta);
        int finalGold = player.currency + (isWinner ? winnerGold : isLoser ? loserGold : winnerGold);

        string ratingLine = $"Rating: {player.rating} → {finalRating}  ({(ratingDelta >= 0 ? "+" : "")}{ratingDelta})";
        string goldLine = $"Gold:   {player.currency} → {finalGold}  (+{(isLoser ? loserGold : winnerGold)})";

        resultText.text = $"{title}\n{ratingLine}\n{goldLine}";
    }

    [ClientRpc]
    private void NotifyClientsToFetchDataClientRpc()
    {
        var pid = SQLiteManager.Instance.player.playerId;
        StartCoroutine(ClientNetworkManager.Instance.GetMatchRecords(pid));
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerStats(pid));
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerData("playerId", pid.ToString(), false));
    }

    private int CalculateRatingDelta(int w, int l)
    {
        int gap = Math.Abs(w - l);
        if (gap >= 200) return 10;
        if (gap >= 100) return 15;
        return 20;
    }

    private void ShutdownNetwork()
    {
        Debug.Log("서버 종료");
        NetworkManager.Singleton.Shutdown();
    }
}