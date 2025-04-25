using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerUISingle : MonoBehaviour
{
    public static PlayerUISingle Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Timer UI")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Profile UI")]
    [SerializeField] private Image profileImage;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI ratingText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ������ �ʱ� ǥ��
        var player = SQLiteManager.Instance.player;
        AddressableManager.Instance.LoadImageFromGroup(player.profileIcon, profileImage);
        nicknameText.text = player.playerName;
        ratingText.text = $"R: {player.rating}";

        // �ʱ� �������޺� ǥ��
        UpdateScoreUI(ScoreManagerSingle.Instance.TotalScore,
                      ScoreManagerSingle.Instance.ComboCount);
    }

    private void OnEnable()
    {
        GameTimer.OnTimerUpdated += UpdateTimerUI;
    }

    private void OnDisable()
    {
        GameTimer.OnTimerUpdated -= UpdateTimerUI;
    }

    /// <summary>
    /// ���� �� �޺� ����
    /// </summary>
    public void UpdateScoreUI(int totalScore, int comboCount)
    {
        if (scoreText != null) scoreText.text = $"Score: {totalScore}";
        if (comboText != null) comboText.text = $"Combo: {comboCount}";
    }

    /// <summary>
    /// Ÿ�̸� UI ����
    /// </summary>
    private void UpdateTimerUI(float remainingTime)
    {
        if (timerSlider != null)
            timerSlider.value = remainingTime / 60f;
        if(timerText != null)
            timerText.text = $"{Mathf.FloorToInt(remainingTime)}";
    }
}