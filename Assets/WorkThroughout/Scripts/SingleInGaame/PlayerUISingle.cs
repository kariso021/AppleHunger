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

    [Header("Combo Effect UI")]
    [SerializeField] private GameObject maxComboEffect;
    [SerializeField] private Image maxComboEffectTimerSlider;
    private Coroutine comboEffectCoroutine;

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
            string path = SQLiteManager.Instance.player.profileIcon;
            AddressableManager.Instance.LoadImageFromGroup(path, profileImage);
            nicknameText.text = SQLiteManager.Instance.player.playerName; ;
            ratingText.text = $"R: {SQLiteManager.Instance.player.rating}";
        }


    }
    private void OnEnable()
    {
        GameTimerSingle.OnTimerUpdated += UpdateTimerUI;
    }

    private void OnDisable()
    {
        GameTimerSingle.OnTimerUpdated -= UpdateTimerUI;

        if (comboEffectCoroutine != null)
            StopCoroutine(comboEffectCoroutine);
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
    /// <summary>
    /// 외부에서 콤보 확인 트리거를 날릴 때 호출할 함수
    /// ScoreManagerSingle 쪽 AddScore 안에서 호출해 주세요
    /// </summary>
    public void TryStartComboEffect()
    {
        if (ScoreManagerSingle.Instance.ComboCount >= ScoreManagerSingle.Instance.MaxCombo)
        {
            if (comboEffectCoroutine == null)
            {
                comboEffectCoroutine = StartCoroutine(ComboEffectWatcher());
            }
        }
    }

    private IEnumerator ComboEffectWatcher()
    {
        float duration = ScoreManagerSingle.Instance.ComboDuration;
        float elapsed = 0f;

        maxComboEffect.SetActive(true);
        maxComboEffectTimerSlider.fillAmount = 1f;
        maxComboEffectTimerSlider.gameObject.SetActive(true);

        while (true)
        {
            elapsed = Time.time - ScoreManagerSingle.Instance.lastCollectTime;

            // 슬라이더 fill 업데이트
            float remaining = Mathf.Clamp01(1f - (elapsed / duration));
            maxComboEffectTimerSlider.fillAmount = remaining;

            // 조건: 콤보 깨지거나 시간 초과
            if (elapsed >= duration || ScoreManagerSingle.Instance.ComboCount < ScoreManagerSingle.Instance.MaxCombo)
                break;

            yield return null;
        }

        // 이펙트 종료
        maxComboEffect.SetActive(false);
        maxComboEffectTimerSlider.gameObject.SetActive(false);
        comboEffectCoroutine = null;
    }

}