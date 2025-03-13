using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class ScoreManager : NetworkBehaviour
{
    private NetworkVariable<int> playerScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> playerCombo = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Dictionary<ulong, Coroutine> comboCoroutines = new Dictionary<ulong, Coroutine>();
    private int maxCombo = 5;
    private float comboTimeLimit = 5f;
    private int comboMultiplier = 1;

    public static event Action<ulong, int, int> OnScoreUpdated; // (ClientId, Score, Combo)

    public override void OnNetworkSpawn()
    {
        if (IsServer) // 서버에서만 점수 관리
        {
            PlayerController.OnAppleCollected += HandleAppleCollected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            PlayerController.OnAppleCollected -= HandleAppleCollected;
        }
    }

    /// ✅ 이벤트 기반 점수 계산
    private void HandleAppleCollected(int appleCount, int appleScoreValue, ulong clientId)
    {
        if (!IsServer) return; // 서버에서만 실행

        int currentCombo = playerCombo.Value;
        int totalComboValue = comboMultiplier * currentCombo;
        int finalScore = appleCount * (totalComboValue + appleScoreValue);

        playerScore.Value += finalScore;
        playerCombo.Value = Mathf.Min(playerCombo.Value + 1, maxCombo);

        // ✅ 클라이언트 UI 업데이트 이벤트 호출
        OnScoreUpdated?.Invoke(clientId, playerScore.Value, playerCombo.Value);

        StartComboTimer(clientId);
    }

    /// ✅ 콤보 타이머 (서버에서 실행)
    private void StartComboTimer(ulong clientId)
    {
        if (comboCoroutines.ContainsKey(clientId) && comboCoroutines[clientId] != null)
        {
            StopCoroutine(comboCoroutines[clientId]);
        }
        comboCoroutines[clientId] = StartCoroutine(ResetComboTimer(clientId));
    }

    private IEnumerator ResetComboTimer(ulong clientId) // ✅ 제네릭 <T> 제거
    {
        yield return new WaitForSeconds(comboTimeLimit);
        playerCombo.Value = 0;
        OnScoreUpdated?.Invoke(clientId, playerScore.Value, 0);
    }
}