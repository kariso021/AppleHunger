using UnityEngine;
using Unity.Netcode;
using System;

public class GameTimer : NetworkBehaviour
{
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float totalGameTime = 60f; // 60초 게임 타이머
    private double startTime; // 서버 기준 게임 시작 시간

    public static event Action<float> OnTimerUpdated; // UI 갱신 이벤트

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            startTime = NetworkManager.Singleton.ServerTime.TimeAsFloat; // 서버 기준 시작 시간 저장
            remainingTime.Value = totalGameTime; // 초기값 설정
        }

        if (IsClient)
        {
            remainingTime.OnValueChanged += HandleTimerUpdated;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            remainingTime.OnValueChanged -= HandleTimerUpdated;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            float elapsedTime = (float)(NetworkManager.Singleton.ServerTime.TimeAsFloat - startTime); // 경과 시간 계산
            float newRemainingTime = Mathf.Max(0, totalGameTime - elapsedTime); // 남은 시간 계산

            if (Mathf.Abs(newRemainingTime - remainingTime.Value) > 0.1f) // 너무 자주 동기화되지 않도록 체크
            {
                remainingTime.Value = newRemainingTime;
            }

            if (remainingTime.Value <= 0)
            {
                Debug.Log("[Server] 게임 종료!");
            }
        }
    }

    /// ✅ 클라이언트에서 UI 업데이트 이벤트 호출
    private void HandleTimerUpdated(float oldTime, float newTime)
    {
        OnTimerUpdated?.Invoke(newTime);
    }
}
