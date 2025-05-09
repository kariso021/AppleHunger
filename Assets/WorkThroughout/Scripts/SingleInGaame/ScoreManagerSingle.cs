using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class ScoreManagerSingle : MonoBehaviour
{
    public static ScoreManagerSingle Instance { get; private set; }

    [Header("Combo Settings")]
    [SerializeField] private float comboDuration = 2f;        // �޺� ���� �ð�
    [SerializeField] private float comboScoreMultiplier = 0.2f; // �޺��� �߰� ���
    [SerializeField] private int maxCombo = 5;                // �ִ� �޺�

    private int totalScore = 0;
    public float lastCollectTime = 0f;
    private int comboCount = 0;

    public int TotalScore => totalScore;
    public int ComboCount => comboCount;
    public int MaxCombo => maxCombo;
    public float ComboDuration => comboDuration;

    // �޺� ����Ʈ��
    public delegate void ComboChangedHandler(int newCombo);
    public event ComboChangedHandler OnComboChanged;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ó�� UI�� 0�� ǥ��
        UpdateScoreUI();
    }

    /// <summary>
    /// ��� ���� �� ȣ���մϴ�.
    /// </summary>
    /// <param name="appleCount">������ ��� ����</param>
    /// <param name="appleScoreValue">�� ����� �⺻ ����</param>
    public void AddScore(int appleCount, int appleScoreValue)
    {
        float now = Time.time;

        // �޺� ���
        if (now - lastCollectTime <= comboDuration)
            comboCount = Mathf.Min(comboCount + 1, maxCombo);
        else
            comboCount = 1;

        lastCollectTime = now;

        // ���� ���
        int baseScore = appleCount * appleScoreValue;
        float multiplier = 1f + (comboCount - 1) * comboScoreMultiplier;
        int finalScore = Mathf.FloorToInt(baseScore * multiplier);

        totalScore += finalScore;
        Debug.Log($"[ScoreManagerSingle] +{finalScore} (�޺� x{comboCount}), ����: {totalScore}");

        UpdateScoreUI();

        // �޺� ����Ʈ Ʈ����
        PlayerUISingle.Instance?.TryStartComboEffect();
    }

    /// <summary>
    /// UI�� ���� �� �޺��� �ݿ��մϴ�.
    /// PlayerUI �ʿ� ���缭 ȣ�� ����� �������ּ���.
    /// </summary>
    private void UpdateScoreUI()
    {
        // ����: PlayerUI.Instance.UpdateScore(totalScore, comboCount);
        PlayerUISingle.Instance.UpdateScoreUI(totalScore, comboCount);
    }
}