using FishNet.Object;
using TMPro;
using UnityEngine;
using System.Collections;

public class ScoreManager : NetworkBehaviour
{
    private int score = 0;
    private int comboCount = 0;
    private int maxCombo = 5;
    private float comboTimeLimit = 5f;
    private Coroutine comboCoroutine;

    [SerializeField] private TextMeshPro scoreText; // 점수 UI 표시

    public int Score => score;

    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateScoreUI(); // 클라이언트가 시작할 때 점수 UI 업데이트
    }

    [ServerRpc]
    public void AddScoreServerRpc(int amount)
    {
        score += amount * (comboCount + 1); // ✅ 콤보를 반영하여 점수 증가
        comboCount = Mathf.Min(comboCount + 1, maxCombo);
        UpdateScoreClientRpc(score, comboCount);

        // 콤보 타이머 리셋
        if (comboCoroutine != null)
            StopCoroutine(comboCoroutine);
        comboCoroutine = StartCoroutine(ResetComboTimer());
    }

    [TargetRpc]
    private void UpdateScoreClientRpc(int newScore, int newCombo)
    {
        score = newScore;
        comboCount = newCombo;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score} (Combo: {comboCount})";
        }
    }

    private IEnumerator ResetComboTimer()
    {
        float timer = 0f;
        while (timer <= comboTimeLimit)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        comboCount = 0; // 콤보 초기화
        UpdateScoreClientRpc(score, comboCount);
    }
}
