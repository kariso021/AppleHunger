using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;


//엔딩이라기보단... 분기로직에 가까움
public class GameEnding : NetworkBehaviour
{
    public static GameEnding Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;



    //Result 세분화해야함
    [SerializeField] private TextMeshProUGUI resultText_WinLose;
    [SerializeField] private TextMeshProUGUI resultText_Rating;
    [SerializeField] private TextMeshProUGUI resultText_RatingChanged;

    [SerializeField] private TextMeshProUGUI resultText_Currency;
    [SerializeField] private TextMeshProUGUI resultText_CurrencyChanged;





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

    // ------------------------------------------
    // player 둘 다 나갔을 때 방 폭파 로직 (오직 서버 전용)
    // ------------------------------------------
    private bool _hasClientEverConnected = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;    // 서버가 아니면 아래 로직 진입하지 않음
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
        base.OnNetworkDespawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        _hasClientEverConnected = true;

        HideDisconnectedClientRpc();


        Debug.Log($"[ClientConnected] ClientId: {clientId}");
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == clientId)
            {
                Debug.Log($"[ClientDisconnected] ClientId: {client.ClientId}");
                break;
            }
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        // 서버에서만 동작하도록 추가 체크
        if (!IsServer || !_hasClientEverConnected)
            return;


        // disconnected 문구 띄우게끔 함 어차피 2인이라서

        ShowDisconnectedClientRpc();

        Debug.Log(NetworkManager.Singleton.ConnectedClientsList.Count);

        foreach(var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == clientId)
            {
                Debug.Log($"[ClientDisconnected] ClientId: {client.ClientId}");
                break;
            }
        }

        // 남은 클라이언트가 없으면 셧다운 나갈때 기점으로 몇명인지 보여주는거라서 1이하가 맞음
        if (NetworkManager.Singleton.ConnectedClientsList.Count <= 1)
        {
            Debug.Log("Shutdown발동함");
            GameTimer.Instance.StopForEndTimer();
            StartCoroutine(ShutdownAfterSessionUpdate());
        }
    }

    private IEnumerator ShutdownAfterSessionUpdate()
    {
        // 서버 전용 보강
        if (!IsServer)
            yield break;

        // 세션 false 처리 완료 대기
        yield return PlayerDataManager.Instance.UpdateAllSessionsFalse();

        // DB 갱신 후 서버 셧다운
        ShutdownNetwork();
    }


    //--------------------------------------------------------------------------------------------------------

    private void OnGameEndedHandler()
    {

        if (NetworkManager.Singleton.ConnectedClientsList.Count == 0)
        {
            Debug.Log("[GameEnding] No clients connected → Skip HandleGameEnd");
            return;
        }

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

        //여기서 드래그 제한
        RestictControllerWhenStartClientRpc(false);
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
                StartCoroutine(ExtendGameTime(extendNoticeDuration));
                return (GameResultType.Extend, topPid, secondPid);
            }
            // 연장 후에도 동점 → 무승부
            return (GameResultType.Draw, topPid, secondPid);
        }

        // 승패 확정
        return (GameResultType.Win, topPid, secondPid);
    }

    private IEnumerator ExtendGameTime(int seconds)
    {
        // 서버→클라 연장 알림
        NotifyClientsToExtendGameTimeClientRpc();

       RestrictAllClientsDragClientRpc(seconds);

        yield return new WaitForSeconds(extendNoticeDuration);
    }


    //제한 보내는것
    [ClientRpc]
    private void RestrictAllClientsDragClientRpc(int seconds)
    {
    
        var pc = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        pc.RestrictDragForWhile(seconds);
    }

    [ClientRpc]
    public void RestictControllerWhenStartClientRpc(bool start)
    {
        var pc = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
        if (start)
        {
            pc.RestrictDrag();
        }
        else
        {
            pc.UnrestrictDrag();
        }

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
            ShowGameOverScreenClientRpc(winId, loseId,true, 0, gold, gold);
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
            ShowGameOverScreenClientRpc(winId, loseId,false, delta, winGold, loseGold);
        }
    }

    [ClientRpc]
    private void ShowGameOverScreenClientRpc(
     int winnerPlayerId,
     int loserPlayerId,
     bool isDraw,
     int ratingDelta,
     int winnerGold,
     int loserGold)
    {
        gameOverPanel.SetActive(true);

        // 현재 플레이어 정보
        var player = SQLiteManager.Instance.player;
        int currentRating = player.rating;
        int currentCurrency = player.currency;

        if (isDraw)
        {
            // 무승부
            resultText_WinLose.text = "🤝 Draw!";
            resultText_Rating.text = $"Rating: {currentRating} → {currentRating}";
            resultText_RatingChanged.text = "+0";
            resultText_RatingChanged.color = Color.white;  // 기본 색
            resultText_Currency.text = $"Gold:   {currentCurrency} → {currentCurrency + winnerGold}";
            resultText_CurrencyChanged.text = $"+{winnerGold}";
            resultText_CurrencyChanged.color = Color.white;
            return;
        }

        // 승패 처리
        bool amIWinner = (player.playerId == winnerPlayerId);

        // 1) Win/Lose 타이틀
        resultText_WinLose.text = amIWinner
            ? "🏆 Winner!"
            : "❌ Loser...";

        // 2) Rating 텍스트
        int finalRating = currentRating + (amIWinner ? ratingDelta : -ratingDelta);
        resultText_Rating.text = $"Rating: {currentRating} → {finalRating}";

        // 3) Rating 변화량(sign + color)
        if (amIWinner)
        {
            resultText_RatingChanged.text = $"+{ratingDelta}";
            resultText_RatingChanged.color = Color.green;
        }
        else
        {
            resultText_RatingChanged.text = $"-{ratingDelta}";
            resultText_RatingChanged.color = Color.red;
        }

        // 4) Currency 텍스트
        int gainGold = amIWinner ? winnerGold : loserGold;
        int finalCurrency = currentCurrency + gainGold;
        resultText_Currency.text = $"Gold:   {currentCurrency} → {finalCurrency}";
        resultText_CurrencyChanged.text = $"+{gainGold}";
        resultText_CurrencyChanged.color = Color.white;
    }

    [ClientRpc]
    private void NotifyClientsToFetchDataClientRpc()
    {
        var pid = SQLiteManager.Instance.player.playerId;
        StartCoroutine(ClientNetworkManager.Instance.GetMatchRecords(pid));
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerStats(pid));
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerData("playerId", pid.ToString(), false));



        //IsConnected false로 바꿔줘야함
        SQLiteManager.Instance.playerSession.isConnected = false;
        SQLiteManager.Instance.SavePlayerSession(
            SQLiteManager.Instance.playerSession);
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

    [ClientRpc]
    private void ShowDisconnectedClientRpc(ClientRpcParams rpcParams = default)
       => PlayerUI.Instance.ShowDisconnectedText();

    [ClientRpc]
    private void HideDisconnectedClientRpc(ClientRpcParams rpcParams = default)
        => PlayerUI.Instance.HideDisconnectedText();

    [ServerRpc(RequireOwnership = false)]
    public void SurrenderRequestServerRpc(int surrenderPlayerId)
    {
        if (!IsServer || hasFinalGameBeenHandled) return;

        Debug.Log($"[Surrender] Player {surrenderPlayerId} surrendered.");

        // 타이머 즉시 멈춤
        GameTimer.Instance.StopForEndTimer();

        // 항복 전용 종료 처리 시작
        StartCoroutine(HandleGameEndBySurrender(surrenderPlayerId));
    }

    // 항복 처리 로직
    private IEnumerator HandleGameEndBySurrender(int surrenderPlayerId)
    {
        // true 일시 드래그 풀림
        RestictControllerWhenStartClientRpc(false);

        // 승자/패자 결정
        int loserId = surrenderPlayerId;
        int winnerId = ScoreManager.Instance.GetAllScores()
                           .Keys.FirstOrDefault(id => id != loserId);

        hasFinalGameBeenHandled = true;
        LastWinnerId = winnerId;
        LastLoserId = loserId;

        var pdm = PlayerDataManager.Instance;
        int winRating = pdm.GetPlayerRating(winnerId);
        int loseRating = pdm.GetPlayerRating(loserId);

        // 세션 종료 → DB 제출 → 보상 계산
        yield return pdm.UpdateAllSessionsFalse();
        yield return SubmitResultToDB(
            GameResultType.Win,
            winnerId, loserId,
            winRating, loseRating
        );

        // 클라이언트 UI 갱신
        NotifyClientsToFetchDataClientRpc();

        // 서버 셧다운
        ShutdownNetwork();
    }


}