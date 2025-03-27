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
    public void AddScore(ulong ClientID, int appleCount, int appleScoreValue)
    {
        if (!IsServer) return;

        if (!playerScores.ContainsKey(ClientID))
        {
            playerScores[ClientID] = 0;
        }

        int finalScore = appleCount * appleScoreValue;
        playerScores[ClientID] += finalScore;

        Debug.Log($"[Server] AddScore - ClientId: {ClientID}, New Score: {playerScores[ClientID]}");

        UpdateScoreClientRpc(ClientID, playerScores[ClientID]);
    }

    /// ✅ 점수를 클라이언트 UI에 반영 (ClientRpc)
    [ClientRpc]
    private void UpdateScoreClientRpc(ulong ClientID, int newScore)
    {
        Debug.Log($"[ClientRpc] UpdateScoreClientRpc - ClientID: {ClientID}, Score: {newScore}");
        PlayerUI.UpdateScoreUI(ClientID, newScore);
    }

    public Dictionary<ulong, int> GetScores()
    {
        return new Dictionary<ulong, int>(playerScores); // 점수 복사하여 반환
    }
}
