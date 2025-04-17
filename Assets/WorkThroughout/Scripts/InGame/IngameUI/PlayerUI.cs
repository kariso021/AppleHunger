using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;

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

    [Header("NickName UI")]
    [SerializeField] private TextMeshProUGUI myNicknameText;
    [SerializeField] private TextMeshProUGUI opponentNicknameText;

    [Header("Rating UI")]
    [SerializeField] private TextMeshProUGUI myRatingText;
    [SerializeField] private TextMeshProUGUI opponentRatingText;

    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private ulong myClientId;

    public static PlayerUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerInitialized += SetPlayerId;
        GameTimer.OnTimerUpdated += UpdateTimerUI;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerInitialized -= SetPlayerId;
        GameTimer.OnTimerUpdated -= UpdateTimerUI;
    }

    private void SetPlayerId(ulong clientId)
    {
        myClientId = NetworkManager.Singleton.LocalClientId;
        // 서버에서 할당된 내 player number, rating, icon, nickname 등을 초기 표시
        UploadProfileImageSelf();
        UploadNickNameSelf();
        UploadRatingSelf();
        // myNumberText는 서버에서 SetMyNumber RPC로 설정됩니다
    }

    // ---------------- Score ----------------
    public void UpdateScoreUI(ulong clientId, int newScore)
    {
        playerScores[clientId] = newScore;
        RefreshScoreUI();
    }

    private void RefreshScoreUI()
    {
        if (playerScores.TryGetValue(myClientId, out var myScore))
            myScoreText.text = $"My Score: {myScore}";

        foreach (var kv in playerScores)
        {
            if (kv.Key != myClientId)
            {
                opponentScoreText.text = $"Opponent Score: {kv.Value}";
                break;
            }
        }
    }

    // ---------------- Timer ----------------
    private void UpdateTimerUI(float remainingTime)
    {
        if (timerSlider != null)
            timerSlider.value = remainingTime / 60f;
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
    public void SetMyNumber(int number)
    {
        myNumberText.text = number.ToString();
    }

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
}
