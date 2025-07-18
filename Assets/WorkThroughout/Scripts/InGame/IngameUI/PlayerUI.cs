﻿using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerUI : MonoBehaviour
{
    [Header("Network Object")]
    [SerializeField] private GameObject NObj; // 네트워크 오브젝트 참조



    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI myScoreText;
    [SerializeField] private TextMeshProUGUI opponentScoreText;

    [Header("Player Number UI")]
    [SerializeField] private TextMeshProUGUI myNumberText;
    [SerializeField] private TextMeshProUGUI opponentNumberText;

    [Header("Image UI")]
    [SerializeField] private Image myProfileImage;
    [SerializeField] private Image opponentProfileImage;


    [Header("Timer UI")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("NickName UI")]
    [SerializeField] private TextMeshProUGUI myNicknameText;
    [SerializeField] private TextMeshProUGUI opponentNicknameText;

    [Header("Rating UI")]
    [SerializeField] private TextMeshProUGUI myRatingText;
    [SerializeField] private TextMeshProUGUI opponentRatingText;

    [Header("NoitfyResetPanel")]
    [SerializeField] private GameObject notifyResetPanel;


    //매칭 패널
    [Header("Opponent IntroduceUI")]
    [SerializeField] private CanvasGroup introduceCG;
    [SerializeField] private Image MatchingPanel_MyProfileImage;
    [SerializeField] private Image MatchingPanel_OpponentProfileImage;


    [SerializeField] private TextMeshProUGUI MatchingPanel_MyNicknameText;
    [SerializeField] private TextMeshProUGUI MatchingPanel_OpponentNicknameText;

    [SerializeField] private TextMeshProUGUI MatchingPanel_MyRatingText;
    [SerializeField] private TextMeshProUGUI MatchingPanel_OpponentRatingText;

    // 매칭 패널 이후 카운트 패널
    [Header("AfterMatching Panel CountPanel")]
    [SerializeField] private GameObject countPanel;
    [SerializeField] private TextMeshProUGUI countText;


    //끊김 메세지
    [Header("Disconnected Text")]
    [SerializeField] private TextMeshProUGUI DisconnectedText;

    [Header("Cancel the Matching")]
    [SerializeField] private Button MatchingCancelButton;
    [SerializeField] private MatchMakerClient matchMakerClient;


    //Surrender 패널 On/Off 그리고 surrender 승낙버튼
    [Header("SurrenerButton")]
    [SerializeField] private Button surrenderPopUpButton;
    [SerializeField] private Button surrenderAcceptButton; // 승낙 버튼
    [SerializeField] private Button surrenderCancelButton; // 취소 버튼
    [SerializeField] private GameObject surrenderPanel; // 패널을 활성화/비활성화할 GameObject

    //게임 종료시 로비로 돌아가는 버튼
    [Header("Button Exit")]
    [SerializeField] private Button exitButton; // 게임 종료 버튼

    




    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private int myPlayerId;

    public static PlayerUI Instance { get; private set; }

    private void Start()
    {
        MatchingCancelButton.onClick.AddListener(matchcancel);
        surrenderAcceptButton.onClick.AddListener(AcceptSurrender);
        surrenderCancelButton.onClick.AddListener(HideSurrenderPanel);
        surrenderPopUpButton.onClick.AddListener(ShowSurrenderPanel);
        exitButton.onClick.AddListener(CleanNetworkAndGoToLobby);
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


        DisconnectedText.gameObject.SetActive(false);
        countPanel.SetActive(false);
        surrenderPanel.SetActive(false);
        //opponentIntroduceUI.gameObject.SetActive(false);

    }

    private void OnEnable()
    {
        Debug.Log("[PlayerUI] Subscribing to timer updates");
        GameTimer.OnTimerUpdated += UpdateTimerUI;
        PlayerController.OnPlayerInitialized += SetPlayerId;
    }

    private void OnDisable()
    {
        Debug.Log("[PlayerUI] Unsubscribing from timer updates");
        GameTimer.OnTimerUpdated -= UpdateTimerUI;
        PlayerController.OnPlayerInitialized -= SetPlayerId;
    
    }

    private void SetPlayerId(ulong clientId)
    {
        UploadProfileImageSelf();
        UploadNickNameSelf();
        UploadRatingSelf();
        // myNumberText는 서버에서 SetMyNumber RPC로 설정됩니다
    }

    // ---------------- Score ----------------
    public void UpdateScoreUIByPlayerId(int playerId, int newScore)
    {
        // 로컬 플레이어 ID 가져오기
        int localId = SQLiteManager.Instance.player.playerId;

        // playerId가 내 ID와 같으면 내 점수, 아니면 상대 점수 업데이트
        if (playerId == localId)
        {
            myScoreText.text = $"Score: {newScore}";
            Debug.Log($"[PlayerUI] 내 점수 업데이트: {newScore}");
        }
        else
        {
            opponentScoreText.text = $"Score: {newScore}";
            Debug.Log($"[PlayerUI] 상대 점수 업데이트: {newScore}");
        }
    }


    // ---------------- Timer ----------------
    private void UpdateTimerUI(float remainingTime)
    {
        if (timerSlider != null)
        {
            if (remainingTime >= 60f)
            {
                timerSlider.value = 1f; // 60초 이상일 때는 슬라이더를 최대값으로 설정
            }
            else
            {
                timerSlider.value = remainingTime / 60f;
            }
        }

        if (timerText != null)
        {
            if (remainingTime >= 60f)
            {
                timerText.text = "60";
            }
            else
            {
                timerText.text = $"{remainingTime:F0}";
            }
        }
    }

    // ---------------- Self Initial Upload ----------------
    private void UploadProfileImageSelf()
    {
        string path = SQLiteManager.Instance.player.profileIcon;
        AddressableManager.Instance.LoadImageFromGroup(path, myProfileImage);
        AddressableManager.Instance.LoadImageFromGroup(path, MatchingPanel_MyProfileImage);
    }

    private void UploadNickNameSelf()
    {
        myNicknameText.text = SQLiteManager.Instance.player.playerName;
        MatchingPanel_MyNicknameText.text = SQLiteManager.Instance.player.playerName;
    }

    private void UploadRatingSelf()
    {
        myRatingText.text = $"R: {SQLiteManager.Instance.player.rating}";
        MatchingPanel_MyRatingText.text = $"Rating: {SQLiteManager.Instance.player.rating}";
    }

    // ---------------- Self Setters for Reconnect ----------------

    public void SetMyRating(int rating)
    {
        myRatingText.text = $"R: {rating}";
        MatchingPanel_MyRatingText.text = $"Rating: {rating}";
    }

    public void SetMyProfileImage(string iconKey)
    {
        AddressableManager.Instance.LoadImageFromGroup(iconKey, myProfileImage);
        AddressableManager.Instance.LoadImageFromGroup(iconKey, MatchingPanel_MyProfileImage);
    }

    public void SetMyNickname(string nickname)
    {
        myNicknameText.text = nickname;
        MatchingPanel_MyNicknameText.text = nickname;
    }

    // ---------------- Opponent ----------------
    public void SetOpponentNumber(int number)
    {
        opponentNumberText.text = number.ToString();

    }

    public void SetOpponentIconImage(string iconKey)
    {
        AddressableManager.Instance.LoadImageFromGroup(iconKey, opponentProfileImage);
        AddressableManager.Instance.LoadImageFromGroup(iconKey, MatchingPanel_OpponentProfileImage);
    }

    public void SetOpponentNickName(string nickname)
    {
        opponentNicknameText.text = nickname;
        MatchingPanel_OpponentNicknameText.text = nickname;
    }

    public void SetOpponentRating(int rating)
    {
        opponentRatingText.text = $"R: {rating}";
        MatchingPanel_OpponentRatingText.text = $"Rating: {rating}";
    }

    //-----------------Notify Reset Panel ----------------

    //
    public void ToggleNotifyResetPanel(float seconds)
    {
        StartCoroutine(NotifyResetCoroutine(seconds));
    }

    // 2) 실제 코루틴
    private IEnumerator NotifyResetCoroutine(float seconds)
    {
        notifyResetPanel.SetActive(true);
        yield return new WaitForSeconds(seconds);
        notifyResetPanel.SetActive(false);
    }

    //----------------Opponent Introduce UI ---------------- 매칭 잡혔을때 기준

    public void OnMatchFoundShowPanel(float panelDuration, float countDuration)
    {
        StartCoroutine(MatchIntroAndCountdown(panelDuration, countDuration));
    }

    private IEnumerator MatchIntroAndCountdown(float panelDuration, float countDuration)
    {
        // 1) 소개 패널 보이기 & 페이드 아웃 (panelDuration 전체 사용)
        ShowIntroducePanel();
        yield return FadeOutAndHide(panelDuration);

        // 2) 카운트다운 패널 보이기 (countDuration 만큼)
        countPanel.SetActive(true);

        int count = Mathf.CeilToInt(countDuration);
        for (int i = count; i > 0; i--)
        {
            countText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        // 3) 카운트 완료 후 숨기기
        countPanel.SetActive(false);
    }

    public void ShowIntroducePanel()
    {
        introduceCG.gameObject.SetActive(true);
        introduceCG.alpha = 1f;
        introduceCG.interactable = true;
        introduceCG.blocksRaycasts = true;
    }

    public IEnumerator FadeOutAndHide(float duration)
    {
        // 패널 전체 표시 시간(duration) 중에서
        // fadeDuration 만큼만 페이드 아웃에 쓰고, 나머지는 그냥 대기
        const float fadeDuration = 1f;
        float holdTime = Mathf.Max(0f, duration - fadeDuration);

        // 1) 잠시 대기
        yield return new WaitForSeconds(holdTime);

        // 2) fadeDuration 동안 alpha 1→0
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            introduceCG.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3) 클린업
        introduceCG.alpha = 0f;
        introduceCG.interactable = false;
        introduceCG.blocksRaycasts = false;
        introduceCG.gameObject.SetActive(false);
    }


    //------------------ 콤보 max 이팩트 ----------------

    public void ShowMaxComboEffect()
    {
        Debug.Log("콤보 max 이팩트 실행");
    }

    // ------------------ Disconnected Text ----------------

    public void ShowDisconnectedText()
    {
        DisconnectedText.gameObject.SetActive(true);
    }

    public void HideDisconnectedText()
    {
        DisconnectedText.gameObject.SetActive(false);
    }

    public void matchcancel()
    {
        if(matchMakerClient != null)
        {
            matchMakerClient.CancelMatch();
        }
        matchMakerClient.CancelMatch();
        CleanNetworkAndGoToLobby();
    }

    public void ShowSurrenderPanel()
    {
        Debug.Log("ShowSurrenderPanel called");
        surrenderPanel.SetActive(true);
    }

    public void HideSurrenderPanel()
    {
        surrenderPanel.SetActive(false);
    }

    public void AcceptSurrender()
    {
        GameEnding.Instance.SurrenderRequestServerRpc(SQLiteManager.Instance.player.playerId);
    }

    public void CleanNetworkAndGoToLobby()
    {
        Destroy(NObj);
        SceneManager.LoadScene("TestLobby");
    }

}
