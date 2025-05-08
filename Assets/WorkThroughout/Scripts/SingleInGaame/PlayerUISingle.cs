using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Rendering;
using System.Collections;

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

    [SerializeField] private GameObject notifyPanel;

    public GameObject EmoticonPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        EmoticonPanel.SetActive(false);

        // 초기 점수·콤보 표시
        UpdateScoreUI(ScoreManagerSingle.Instance.TotalScore,
                      ScoreManagerSingle.Instance.ComboCount);

        // 프로필 초기 표시
        if (SQLiteManager.Instance != null)
        {
            var player = SQLiteManager.Instance.player;
            AddressableManager.Instance.LoadImageFromGroup(player.profileIcon, profileImage);
            nicknameText.text = player.playerName;
            ratingText.text = $"R: {player.rating}";
        }


    }
    private void OnEnable()
    {
        GameTimerSingle.OnTimerUpdated += UpdateTimerUI;
    }

    private void OnDisable()
    {
        GameTimerSingle.OnTimerUpdated -= UpdateTimerUI;
    }

    /// <summary>
    /// 점수 및 콤보 갱신
    /// </summary>
    public void UpdateScoreUI(int totalScore, int comboCount)
    {
        if (scoreText != null) scoreText.text = $"Score: {totalScore}";
        if (comboText != null) comboText.text = $"Combo: {comboCount}";
    }

    /// <summary>
    /// 타이머 UI 갱신
    /// </summary>
    private void UpdateTimerUI(float remainingTime)
    {
        if (timerSlider != null)
            timerSlider.value = remainingTime / 60f;
        if(timerText != null)
            timerText.text = $"{Mathf.FloorToInt(remainingTime)}";
    }

    public void ShowNotifyPanelForSeconds(float seconds)
    {
        if (notifyPanel == null) return;

        StartCoroutine(NotifyRoutine(seconds));
    }

    private IEnumerator NotifyRoutine(float seconds)
    {
        notifyPanel.SetActive(true);
        StartCoroutine(StopComboTimeWhenNotifyRoutineActive(seconds));
        yield return new WaitForSeconds(seconds);
        notifyPanel.SetActive(false);
    }

    private IEnumerator StopComboTimeWhenNotifyRoutineActive(float seconds)
    {
        float elapsed = 0f;
        ScoreManagerSingle sms = ScoreManagerSingle.Instance;
        Debug.Log($"라스트 콜렉트 타임 {sms.lastCollectTime}");

        while(elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        sms.lastCollectTime = Time.time;
        Debug.Log($"라스트 콜렉트 타임 더해버림 {sms.lastCollectTime}");

    }
}