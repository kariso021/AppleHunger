using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // clientId 기준 점수 및 콤보/타이머 저장
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private Dictionary<ulong, float> lastCollectTime = new Dictionary<ulong, float>();
    private Dictionary<ulong, int> comboCounts = new Dictionary<ulong, int>();

    [SerializeField] private float comboDuration = 2f;
    [SerializeField] private float comboScoreMultiplier = 0.2f;
    [SerializeField] private int maxCombo = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        PlayerController.OnAppleCollected += HandleAppleCollected;
    }

    private void OnDestroy()
    {
        if (IsServer) PlayerController.OnAppleCollected -= HandleAppleCollected;
    }

    private void HandleAppleCollected(int appleCount, int appleScoreValue, ulong clientId)
    {
        AddScore(clientId, appleCount, appleScoreValue);
    }

    /// <summary>
    /// 서버에서 점수 계산 후 저장 및 클라 UI 업데이트
    /// </summary>
    public void AddScore(ulong clientId, int appleCount, int appleScoreValue)
    {
        if (!IsServer) return;

        float now = Time.time;
        if (!playerScores.ContainsKey(clientId))
            playerScores[clientId] = 0;

        // 콤보 타이머
        if (!lastCollectTime.ContainsKey(clientId) || now - lastCollectTime[clientId] > comboDuration)
            comboCounts[clientId] = 1;
        else
            comboCounts[clientId] = Mathf.Min(comboCounts[clientId] + 1, maxCombo);

        lastCollectTime[clientId] = now;

        int baseScore = appleCount * appleScoreValue;
        float multiplier = 1f + (comboCounts[clientId] - 1) * comboScoreMultiplier;
        int finalScore = Mathf.FloorToInt(baseScore * multiplier);

        playerScores[clientId] += finalScore;
        UpdateScoreClientRpc(clientId, playerScores[clientId]);
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(ulong clientId, int newScore)
    {
        PlayerUI.Instance.UpdateScoreUI(clientId, newScore);
    }

    /// <summary>
    /// 재접속 시 oldCid 데이터를 newCid로 이관
    /// </summary>
    public void HandleReconnect(ulong oldCid, ulong newCid)
    {
        if (!IsServer) return;

        // 점수 이관
        if (playerScores.TryGetValue(oldCid, out var score))
        {
            playerScores.Remove(oldCid);
            playerScores[newCid] = score;
            UpdateScoreClientRpc(newCid, score);
        }
        // 콤보/타이머 이관
        if (lastCollectTime.TryGetValue(oldCid, out var t))
        {
            lastCollectTime.Remove(oldCid);
            lastCollectTime[newCid] = t;
        }
        if (comboCounts.TryGetValue(oldCid, out var c))
        {
            comboCounts.Remove(oldCid);
            comboCounts[newCid] = c;
        }
    }

    /// <summary>
    /// 현재 저장된 모든 clientId→점수 사전 복사본
    /// </summary>
    public Dictionary<ulong, int> GetScores()
    {
        return new Dictionary<ulong, int>(playerScores);
    }

    // 초기 점수 등록용 RPC (필요 시 호출)
    [ServerRpc(RequireOwnership = false)]
    public void InitializePlayerScoreServerRpc(ulong clientId)
    {
        if (!playerScores.ContainsKey(clientId))
            playerScores[clientId] = 0;
    }

    // 클라이언트에서 점수 추가 요청용 RPC
    [ServerRpc(RequireOwnership = false)]
    public void RequestAddScoreServerRpc(ulong clientId, int appleCount, int appleScoreValue)
    {
        AddScore(clientId, appleCount, appleScoreValue);
    }
}
