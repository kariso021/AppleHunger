using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
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


    //매칭 잡히고 게임 시작되기까지의 준비시간
    [SerializeField] private float ReadyTime = 3f;


    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private int myPlayerId;

    public static PlayerUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


        DisconnectedText.gameObject.SetActive(false);
        countPanel.SetActive(false);
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
            myScoreText.text = $"My Score: {newScore}";
            Debug.Log($"[PlayerUI] 내 점수 업데이트: {newScore}");
        }
        else
        {
            opponentScoreText.text = $"Opponent Score: {newScore}";
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

    public void OnMatchFoundShowPanel(float duration)
    {
        StartCoroutine(MatchIntroAndCountdown(duration));
    }

    private IEnumerator MatchIntroAndCountdown(float introDuration)
    {
        // 1) 소개 패널 보이기 & 페이드 아웃
        ShowIntroducePanel();
        yield return FadeOutAndHide(introDuration);

        // 2) 카운트 패널 보이기
        countPanel.SetActive(true);

        int count = Mathf.FloorToInt(ReadyTime);
        for (int i = count; i > 0; i--)
        {
            countText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        // 3) 카운트 완료 후 숨기고 실제 게임 시작 콜백
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
        float fadeDuration = 1f;             
        float holdTime = duration - fadeDuration;

        
        introduceCG.alpha = 1f;
        yield return new WaitForSeconds(holdTime);

        //fadeDuration 동안 alpha를 1 → 0으로 선형 보간
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            introduceCG.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 비활성화 처리
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

}
