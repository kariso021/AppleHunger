﻿using UnityEngine;
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


    [Header("Opponent IntroduceUI")]
    [SerializeField] private CanvasGroup opponentIntroduceUI;



    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private int myPlayerId;

    public static PlayerUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

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
            timerSlider.value = remainingTime / 60f;
        if (timerText != null)
            timerText.text = $"{remainingTime:F0}";
    }

    // ---------------- Self Initial Upload ----------------
    private void UploadProfileImageSelf()
    {
        string path = SQLiteManager.Instance.player.profileIcon;
        AddressableManager.Instance.LoadImageFromGroup(path, myProfileImage);
    }

    private void UploadNickNameSelf()
    {
        myNicknameText.text = SQLiteManager.Instance.player.playerName;
    }

    private void UploadRatingSelf()
    {
        myRatingText.text = $"R: {SQLiteManager.Instance.player.rating}";
    }

    // ---------------- Self Setters for Reconnect ----------------

    public void SetMyRating(int rating)
    {
        myRatingText.text = $"R: {rating}";
    }

    public void SetMyProfileImage(string iconKey)
    {
        AddressableManager.Instance.LoadImageFromGroup(iconKey, myProfileImage);
    }

    public void SetMyNickname(string nickname)
    {
        myNicknameText.text = nickname;
    }

    // ---------------- Opponent ----------------
    public void SetOpponentNumber(int number)
    {
        opponentNumberText.text = number.ToString();
    }

    public void SetOpponentIconImage(string iconKey)
    {
        AddressableManager.Instance.LoadImageFromGroup(iconKey, opponentProfileImage);
    }

    public void SetOpponentNickName(string nickname)
    {
        opponentNicknameText.text = nickname;
    }

    public void SetOpponentRating(int rating)
    {
        opponentRatingText.text = $"R: {rating}";
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
        // 1) 패널 보이기
        ShowIntroducePanel();
        
        StartCoroutine(FadeOutAndHide(duration)); 
        
    }

    public void ShowIntroducePanel()
    {
        opponentIntroduceUI.gameObject.SetActive(true);
    }


    public void HideIntroducePanel()
    {
        opponentIntroduceUI.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutAndHide(float fadeDuration)
    {
        float startAlpha = opponentIntroduceUI.alpha;  // 보통 1
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            opponentIntroduceUI.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 완전 투명 & 패널 비활성화
        opponentIntroduceUI.alpha = 0f;
        opponentIntroduceUI.gameObject.SetActive(false);
    }

    //------------------ 콤보 max 이팩트 ----------------

    public void ShowMaxComboEffect()
    {
        Debug.Log("콤보 max 이팩트 실행");
    }

}
