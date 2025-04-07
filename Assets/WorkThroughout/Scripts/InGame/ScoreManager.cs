using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private Dictionary<ulong, float> lastCollectTime = new Dictionary<ulong, float>();
  

    //콤보 점수 계산을 위한 변수
    [SerializeField] private float comboDuration = 2f; // 콤보 지속 시간
    [SerializeField] private float comboScoreMultiplier = 0.2f; // 콤보 점수 배율
    [SerializeField] private Dictionary<ulong , int> comboCounts= new Dictionary<ulong, int>(); // 콤보 점수 저장
    [SerializeField] private int maxCombo = 5; // 최대 콤보 수


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

    /// 사과를 먹었을 때 점수 처리 (서버에서 실행)
    private void HandleAppleCollected(int appleCount, int appleScoreValue, ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"[Server] HandleAppleCollected - ClientID: {clientId}, AppleCount: {appleCount}, AppleScoreValue: {appleScoreValue}");
        AddScore(clientId, appleCount, appleScoreValue);
    }

    /// 점수 추가 (서버에서 실행) -> 콤보점수 계산해서 추가한 버전
    public void AddScore(ulong ClientID, int appleCount, int appleScoreValue)
    {
        if (!IsServer) return;

        float now = Time.time;

        if (!playerScores.ContainsKey(ClientID))
        {
            playerScores[ClientID] = 0;
        }

        if(!lastCollectTime.ContainsKey(ClientID) || now - lastCollectTime[ClientID] > comboDuration)
    {
            comboCounts[ClientID] = 1;
        }
    else
        {
            comboCounts[ClientID] = Mathf.Min(comboCounts.GetValueOrDefault(ClientID, 0) + 1, maxCombo);
        }

        lastCollectTime[ClientID] = now;



        int baseScore = appleCount * appleScoreValue;
        float multiplier = 1f + (comboCounts[ClientID] - 1) * comboScoreMultiplier;
        int finalScore = Mathf.FloorToInt(baseScore * multiplier);


        playerScores[ClientID] += finalScore;

        Debug.Log($"[Server] AddScore - ClientId: {ClientID}, New Score: {playerScores[ClientID]}");

        UpdateScoreClientRpc(ClientID, playerScores[ClientID]);
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(ulong ClientID, int newScore)
    {
        Debug.Log($"[ClientRpc] UpdateScoreClientRpc - ClientID: {ClientID}, Score: {newScore}");
        PlayerUI.Instance.UpdateScoreUI(ClientID, newScore);
    }

    public Dictionary<ulong, int> GetScores()
    {
        return new Dictionary<ulong, int>(playerScores); // 점수 복사하여 반환
    }
}
