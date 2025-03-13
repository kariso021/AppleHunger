using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI myScoreText;
    [SerializeField] private TextMeshProUGUI opponentScoreText;

    [Header("Game Timer UI")]
    [SerializeField] private Slider timerSlider;

    private ulong myPlayerId;
    private float maxTime = 60f; // 전체 게임 시간 (서버에서 가져옴)

    private void Start()
    {
        myPlayerId = NetworkManager.Singleton.LocalClientId;
    }

    private void OnEnable()
    {
        ScoreManager.OnScoreUpdated += UpdateScoreUI;
        GameTimer.OnTimerUpdated += UpdateTimerUI;
    }

    private void OnDisable()
    {
        ScoreManager.OnScoreUpdated -= UpdateScoreUI;
        GameTimer.OnTimerUpdated -= UpdateTimerUI;
    }

    /// ✅ 점수 업데이트
    private void UpdateScoreUI(ulong playerId, int newScore, int newCombo)
    {
        if (playerId == myPlayerId)
        {
            myScoreText.text = $"My Score: {newScore} (Combo: {newCombo})";
        }
        else
        {
            opponentScoreText.text = $"Opponent Score: {newScore}";
        }
    }

    /// ✅ 타이머 업데이트 (서버에서 값 받음)
    private void UpdateTimerUI(float newTime)
    {
        timerSlider.value = newTime / maxTime; // 0~1로 정규화
    }
}
