using UnityEngine;
using Unity.Netcode;
using System;

public class GameTimer : NetworkBehaviour
{
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float totalGameTime = 10f;
    private double startTime;
    private bool isGameEnded = false; // 🔥 게임 종료가 한 번만 실행되도록 플래그 추가

    public static event Action OnGameEnded;
    public static event Action<float> OnTimerUpdated;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            startTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
            remainingTime.Value = totalGameTime;
            isGameEnded = false; // 🔹 게임 시작 시 플래그 초기화
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
            float elapsedTime = (float)(NetworkManager.Singleton.ServerTime.TimeAsFloat - startTime);
            float newRemainingTime = Mathf.Max(0, totalGameTime - elapsedTime);

            // 🔹 remainingTime을 업데이트
            if (Mathf.Abs(newRemainingTime - remainingTime.Value) > 0.1f)
            {
                remainingTime.Value = newRemainingTime;
                Debug.Log($"{remainingTime.Value}");
            }

            if (!isGameEnded && newRemainingTime <= 0)
            {
                isGameEnded = true; 
                OnGameEnded?.Invoke();
            }
        }
    }

    private void HandleTimerUpdated(float oldTime, float newTime)
    {
        OnTimerUpdated?.Invoke(newTime);
    }
}
