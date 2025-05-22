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

        // �ʱ� �������޺� ǥ��
        UpdateScoreUI(ScoreManagerSingle.Instance.TotalScore,
                      ScoreManagerSingle.Instance.ComboCount);

        // ������ �ʱ� ǥ��
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
        Debug.Log($"��Ʈ �ݷ�Ʈ Ÿ�� {sms.lastCollectTime}");

        while(elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        sms.lastCollectTime = Time.time;
        Debug.Log($"��Ʈ �ݷ�Ʈ Ÿ�� ���ع��� {sms.lastCollectTime}");

    }
    /// <summary>
    /// �ܺο��� �޺� Ȯ�� Ʈ���Ÿ� ���� �� ȣ���� �Լ�
    /// ScoreManagerSingle �� AddScore �ȿ��� ȣ���� �ּ���
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

            // �����̴� fill ������Ʈ
            float remaining = Mathf.Clamp01(1f - (elapsed / duration));
            maxComboEffectTimerSlider.fillAmount = remaining;

            // ����: �޺� �����ų� �ð� �ʰ�
            if (elapsed >= duration || ScoreManagerSingle.Instance.ComboCount < ScoreManagerSingle.Instance.MaxCombo)
                break;

            yield return null;
        }

        // ����Ʈ ����
        maxComboEffect.SetActive(false);
        maxComboEffectTimerSlider.gameObject.SetActive(false);
        comboEffectCoroutine = null;
    }

}