using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // playerId 기준 점수, 콤보, 타이머 저장
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private Dictionary<int, float> lastCollectTime = new Dictionary<int, float>();
    private Dictionary<int, int> comboCounts = new Dictionary<int, int>();

    [Header("Combo Settings")]
    [SerializeField] private float comboDuration = 2f;   // 콤보 유지 시간
    [SerializeField] private float comboScoreMultiplier = 0.2f; // 콤보당 추가 배수
    [SerializeField] private int maxCombo = 5;    // 최대 콤보 수

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 클라이언트가 사과를 먹을 때 호출하는 RPC.
    /// playerId를 넘겨 줍니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestAddScoreServerRpc(int playerId, int appleCount, int appleScoreValue)
    {
        if (!IsServer) return;
        AddScore(playerId, appleCount, appleScoreValue);
    }

    /// <summary>
    /// 서버에서 콤보/타이머 로직을 처리하고 점수를 갱신합니다.
    /// </summary>
    public void AddScore(int playerId, int appleCount, int appleScoreValue)
    {
        float now = Time.time;

        // 초기화
        if (!playerScores.ContainsKey(playerId))
            playerScores[playerId] = 0;
        if (!lastCollectTime.ContainsKey(playerId))
            lastCollectTime[playerId] = 0f;
        if (!comboCounts.ContainsKey(playerId))
            comboCounts[playerId] = 0;

        // 콤보 타이머: 마지막 수집 시점으로부터 comboDuration 초 이내라면 콤보 유지
        if (now - lastCollectTime[playerId] <= comboDuration)
        {
            comboCounts[playerId] = Mathf.Min(comboCounts[playerId] + 1, maxCombo);
        }
        else
        {
            comboCounts[playerId] = 1;
        }

        lastCollectTime[playerId] = now;

        // 점수 계산
        int baseScore = appleCount * appleScoreValue;
        float multiplier = 1f + (comboCounts[playerId] - 1) * comboScoreMultiplier;
        int finalScore = Mathf.FloorToInt(baseScore * multiplier);

        // 누적 점수 업데이트
        playerScores[playerId] += finalScore;
        Debug.Log($"[ScoreManager] 점수 추가: {playerId} = {finalScore} (콤보)");

        // 모든 클라이언트에 브로드캐스트
        UpdateScoreClientRpc(playerId, playerScores[playerId]);
    }

    /// <summary>
    /// 모든 클라이언트에서 playerId에 해당하는 UI를 업데이트합니다.
    /// </summary>
    [ClientRpc]
    private void UpdateScoreClientRpc(int playerId, int newScore)
    {
        Debug.Log($"[ScoreManager] 점수 업데이트: {playerId} = {newScore}");
        PlayerUI.Instance.UpdateScoreUIByPlayerId(playerId, newScore);
    }

    /// <summary>
    /// 재접속 시 clientId 대신 playerId로만 관리하므로 이관 로직이 필요 없습니다.
    /// </summary>
    public Dictionary<int, int> GetAllScores()
    {
        return new Dictionary<int, int>(playerScores);
    }

    //점수 싱크로나이징

    public void SyncAllScoresToClient(ulong clientId)
    {
        foreach (var kv in playerScores)
        {
            // TargetClientIds 로 한 명에게만 보냄
            SyncScoreClientRpc(
                kv.Key,
                kv.Value,
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { clientId }
                    }
                }
            );
        }
    }

    [ClientRpc]
    private void SyncScoreClientRpc(
        int playerId,
        int score,
        ClientRpcParams rpcParams = default
    )
    {
        // 클라이언트는 기존 UpdateScoreUIByPlayerId 로 화면만 덮어씁니다.
        PlayerUI.Instance.UpdateScoreUIByPlayerId(playerId, score);
    }
}