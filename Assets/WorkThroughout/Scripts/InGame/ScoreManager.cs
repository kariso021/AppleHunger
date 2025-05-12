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
    public void RequestAddScoreServerRpc(
      int playerId,
      int appleCount,
      int appleScoreValue,
      ServerRpcParams rpcParams = default    // ← 추가
  )
    {
        if (!IsServer) return;

        // 호출한 클라이언트 ID
        ulong callerClientId = rpcParams.Receive.SenderClientId;

        AddScore(playerId, appleCount, appleScoreValue, callerClientId);
    }

    /// <summary>
    /// 서버에서 콤보/타이머 로직을 처리하고 점수를 갱신합니다.
    /// </summary>
    public void AddScore(int playerId, int appleCount, int appleScoreValue, ulong callerClientId)
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

        int currentCombo = comboCounts[playerId];

        // 누적 점수 업데이트
        playerScores[playerId] += finalScore;
        Debug.Log($"[ScoreManager] 점수 추가: {playerId} = {finalScore} (콤보)");

        // 모든 클라이언트에 브로드캐스트
        UpdateScoreClientRpc(playerId, playerScores[playerId]);

        ShowComboClientRpc(
       currentCombo,
       new ClientRpcParams
       {
           Send = new ClientRpcSendParams
           {
               TargetClientIds = new[] { callerClientId }
           }
       }
        );

        // 콤보 맥스 이팩트
        // maxCombo 도달 시 추가 이펙트 RPC Duration 고려해서 해야함 -> Duration 도 통일하면 좋겠는데?
        if (currentCombo >= maxCombo)
        {
            ShowMaxComboEffectClientRpc(
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { callerClientId } }
                }
            );
        }
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

    //-----------------------------------------------------------------------



    [ClientRpc]
    private void ShowComboClientRpc(int comboCount, ClientRpcParams rpcParams = default)
    {
        // 멀티플레이용 컨트롤러에서 처리
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        var pc = localPlayer.GetComponent<PlayerController>();
        pc.ShowLocalCombo(comboCount);
    }

    //-------------------------------------------------Reset 시 Combo 시간 연장

    public void WhenResetExtendComboDuration(float seconds)
    {
        var playerIds = new List<int>(lastCollectTime.Keys);
        foreach (var pid in playerIds)
        {
            lastCollectTime[pid] += seconds;
        }
    }

    //-------------------------------------------------콤보 맥스 임팩트
    [ClientRpc]
    private void ShowMaxComboEffectClientRpc(ClientRpcParams rpcParams = default)
    {
        // 로컬 플레이어의 UI 매니저를 호출
        PlayerUI.Instance.ShowMaxComboEffect();
    }


}