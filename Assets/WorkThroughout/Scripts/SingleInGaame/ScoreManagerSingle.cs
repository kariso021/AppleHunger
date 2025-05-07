using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class ScoreManagerSingle : MonoBehaviour
{
    public static ScoreManagerSingle Instance { get; private set; }

    [Header("Combo Settings")]
    [SerializeField] private float comboDuration = 2f;        // 콤보 유지 시간
    [SerializeField] private float comboScoreMultiplier = 0.2f; // 콤보당 추가 배수
    [SerializeField] private int maxCombo = 5;                // 최대 콤보

    private int totalScore = 0;
    public float lastCollectTime = 0f;
    private int comboCount = 0;

    public int TotalScore => totalScore;
    public int ComboCount => comboCount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 처음 UI에 0점 표시
        UpdateScoreUI();
    }

    /// <summary>
    /// 사과 수집 시 호출합니다.
    /// </summary>
    /// <param name="appleCount">수집한 사과 개수</param>
    /// <param name="appleScoreValue">각 사과당 기본 점수</param>
    public void AddScore(int appleCount, int appleScoreValue)
    {
        float now = Time.time;

        // 콤보 계산
        if (now - lastCollectTime <= comboDuration)
            comboCount = Mathf.Min(comboCount + 1, maxCombo);
        else
            comboCount = 1;

        lastCollectTime = now;

        // 점수 계산
        int baseScore = appleCount * appleScoreValue;
        float multiplier = 1f + (comboCount - 1) * comboScoreMultiplier;
        int finalScore = Mathf.FloorToInt(baseScore * multiplier);

        totalScore += finalScore;
        Debug.Log($"[ScoreManagerSingle] +{finalScore} (콤보 x{comboCount}), 총점: {totalScore}");

        UpdateScoreUI();
    }

    /// <summary>
    /// UI에 점수 및 콤보를 반영합니다.
    /// PlayerUI 쪽에 맞춰서 호출 방식을 수정해주세요.
    /// </summary>
    private void UpdateScoreUI()
    {
        // 예시: PlayerUI.Instance.UpdateScore(totalScore, comboCount);
        PlayerUISingle.Instance.UpdateScoreUI(totalScore, comboCount);
    }
}