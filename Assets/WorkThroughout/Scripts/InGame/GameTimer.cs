using UnityEngine;
using Unity.Netcode;
using System;

public class GameTimer : NetworkBehaviour
{
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private float totalGameTime = 60f;
    private double startTime;
    private bool isGameEnded = false;
    private bool isDrawGame = false;

    public static event Action OnGameEnded;
    public static event Action<float> OnTimerUpdated;


    public static GameTimer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



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
            }

            if (newRemainingTime <= 0 && !isGameEnded)
            {
                HandleGameEndLogic();
            }
        }
    }

    private void HandleGameEndLogic()
    {
        var result = GameEnding.Instance.DetermineWinner(
         out int winnerId,
         out int loserId,
         out int winnerRating,
         out int loserRating
     );

        switch (result)
        {
            case GameEnding.GameResultType.Extend:
                Debug.Log("무승부 → 연장됨, isGameEnded 유지");
                OnGameEnded?.Invoke(); // 무승부 UI 알림용
                break;

            case GameEnding.GameResultType.Draw:
                Debug.Log("연장 이후 무승부 확정 → 게임 종료");
                isDrawGame = true;
                isGameEnded = true;
                OnGameEnded?.Invoke();
                break;

            case GameEnding.GameResultType.Win:
            default:
                Debug.Log("승패 결정 → 게임 종료");
                isDrawGame = false;
                isGameEnded = true;
                OnGameEnded?.Invoke();
                break;
        }
    }

    private bool CheckDrawCondition()
    {
        throw new NotImplementedException();
    }

    private void HandleTimerUpdated(float oldTime, float newTime)
    {
        OnTimerUpdated?.Invoke(newTime);
    }

    public void ExtendTime(float extraSeconds)
    {
        if (!IsServer)
        {
            return;
        }

        totalGameTime += extraSeconds;
    }
}
