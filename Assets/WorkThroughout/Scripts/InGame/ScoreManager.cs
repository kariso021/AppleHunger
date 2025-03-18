using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        PlayerController.OnAppleCollected += HandleAppleCollected;
    }

    private void OnDestroy()
    {
        if (IsServer)
        {
            PlayerController.OnAppleCollected -= HandleAppleCollected;
        }
    }

    /// ✅ 사과를 먹었을 때 점수 처리 (서버에서 실행)
    private void HandleAppleCollected(int appleCount, int appleScoreValue, ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"[Server] HandleAppleCollected - ClientID: {clientId}, AppleCount: {appleCount}, AppleScoreValue: {appleScoreValue}");
        AddScore(clientId, appleCount, appleScoreValue);
    }

    /// ✅ 점수 추가 (서버에서 실행)
    public void AddScore(ulong playerId, int appleCount, int appleScoreValue)
    {
        if (!IsServer) return;

        if (!playerScores.ContainsKey(playerId))
        {
            playerScores[playerId] = 0;
        }

        int finalScore = appleCount * appleScoreValue;
        playerScores[playerId] += finalScore;

        Debug.Log($"[Server] AddScore - PlayerID: {playerId}, New Score: {playerScores[playerId]}");

        UpdateScoreClientRpc(playerId, playerScores[playerId]);
    }

    /// ✅ 점수를 클라이언트 UI에 반영 (ClientRpc)
    [ClientRpc]
    private void UpdateScoreClientRpc(ulong playerId, int newScore)
    {
        Debug.Log($"[ClientRpc] UpdateScoreClientRpc - PlayerID: {playerId}, Score: {newScore}");
        PlayerUI.UpdateScoreUI(playerId, newScore);
    }
}
