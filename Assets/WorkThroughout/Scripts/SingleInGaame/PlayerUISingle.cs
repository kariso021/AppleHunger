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


    // ��Ī �г� ���� ī��Ʈ �г�
    [Header("AfterMatching Panel CountPanel")]
    [SerializeField] private GameObject countPanel;
    [SerializeField] private TextMeshProUGUI countText;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        countPanel.SetActive(false);
    }

    private void Start()
    {
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

        StartCoroutine(CountDown());


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
    }

    /// <summary>
    /// Ÿ�̸� UI ����
    /// </summary>
    private void UpdateTimerUI(float remainingTime)
    {
        float totalTime = GameTimerSingle.Instance.totalGameTimeInSeconds;


        if (remainingTime > totalTime)
        {
            timerSlider.value = 1;
            timerText.text = $"{Mathf.FloorToInt(totalTime)}";
        }
        else if ((timerSlider != null) && (timerText != null))
        {
            timerSlider.value = remainingTime / totalTime;
            timerText.text = $"{Mathf.FloorToInt(remainingTime)}";
        }
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


    //------------------ ī��Ʈ �г� ----------------
    public float GetReadyTime()
    {

       return GameTimerSingle.Instance.ReadyTime;
    }




    private IEnumerator CountDown()
    {
        float readyTimeFloat = GetReadyTime();
        int remainingSeconds = Mathf.CeilToInt(readyTimeFloat);
        countPanel.SetActive(true);

        while (remainingSeconds > 0)
        {
            if (remainingSeconds == Mathf.CeilToInt(readyTimeFloat))
            {
                countText.text = "START";
                yield return new WaitForSeconds(1f);
            }
            else
            {

                countText.text = remainingSeconds.ToString();
                yield return new WaitForSeconds(1f);
            }

                remainingSeconds--;
            
        }
        countPanel.SetActive(false);
        yield break;
    }



}