using UnityEngine;
using Unity.Netcode;
using System;

public class GameTimer : NetworkBehaviour
{
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private float totalGameTime = 60f; // 기본 게임 시간
    private float startTime;
    private float endTime;  // 일반 게임 종료 시각 (startTime + totalGameTime)
    private bool isGameEnded = false;
    private bool isDrawGame = false;

    // 연장 타이머 관련 변수
    private bool isInExtension = false;
    private float extensionDuration = 0f;
    private float extensionStartTime = 0f;

    public static event Action OnGameEnded;
    public static event Action<float> OnTimerUpdated;

    public static GameTimer Instance { get; private set; }

    // 외부에서 연장 상태를 읽을 수 있도록 프로퍼티 추가
    public bool IsInExtension => isInExtension;

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
            endTime = startTime + totalGameTime;
            remainingTime.Value = totalGameTime;
            isGameEnded = false;
            isInExtension = false;
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
        if (!IsServer)
            return;

        float currentTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        float newRemainingTime = 0f;

        if (isInExtension)
        {
            newRemainingTime = Mathf.Max(0, extensionDuration - (currentTime - extensionStartTime));
        }
        else
        {
            newRemainingTime = Mathf.Max(0, (endTime - currentTime));
        }

        if (Mathf.Abs(newRemainingTime - remainingTime.Value) > 0.1f)
        {
            remainingTime.Value = newRemainingTime;
        }


        // 남은 시간이 0이 되었고 아직 게임 종료 처리가 안 되었으면 실행
        if (newRemainingTime <= 0 && !isGameEnded)
        {
            if (isInExtension)
            {
                // 연장 시간이 다 소진된 경우 연장 모드 종료 후 최종 게임 처리
                isInExtension = false;
                HandleGameEndLogic();
            }
            else
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
                ExtendTime(17f);
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

    private void HandleTimerUpdated(float oldTime, float newTime)
    {
        OnTimerUpdated?.Invoke(newTime);
    }

    /// <summary>
    /// 연장 타이머를 시작하거나 추가하는 함수
    /// </summary>
    public void ExtendTime(float extraSeconds)
    {
        if (!IsServer)
            return;

        float currentTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;

        if (!isInExtension)
        {
            isInExtension = true;
            extensionDuration = extraSeconds;
            extensionStartTime = currentTime;
        }
        else
        {
            float remainingExtension = extensionDuration - (currentTime - extensionStartTime);
            extensionDuration = remainingExtension + extraSeconds;
            extensionStartTime = currentTime;
        }
    }
}
