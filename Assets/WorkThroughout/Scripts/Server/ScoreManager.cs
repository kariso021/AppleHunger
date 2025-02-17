using FishNet.Object;
using UnityEngine;

public class ScoreManager : NetworkBehaviour // ✅ 파일명과 클래스명이 동일해야 함!
{
    private int totalScore = 0;
    private int comboCount = 0;
    private int maxCombo = 5;

    public void AddScore(int amount, int removedAppleCount)
    {
        if (!IsServer) return; // 🛑 서버에서만 실행

        totalScore += amount;
        comboCount = Mathf.Min(comboCount + 1, maxCombo);

        UpdateScoreObserversRpc(totalScore, comboCount);
    }

    [ObserversRpc]
    private void UpdateScoreObserversRpc(int newScore, int newComboCount)
    {
        UIManager.Instance.UpdateScore(newScore);
       // UIManager.Instance.UpdateComboUI(newComboCount);
    }
}