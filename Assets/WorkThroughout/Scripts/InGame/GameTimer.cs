using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

public class GameTimer : NetworkBehaviour
{
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private float totalGameTime = 60f;
    [SerializeField] private float extensionNoticeDuration = 2f;

    private float startTime;
    private float endTime;
    private bool isGameEnded = false;
    private bool isDrawGame = false;

    private bool isInExtension = false;
    private float extensionDuration = 0f;
    private float extensionStartTime = 0f;

    public static event Action OnGameEnded;
    public static event Action<float> OnTimerUpdated;

    private bool isPaused;                // 타이머 일시 정지 여부
    private bool isIndefinitePause;       // 무한 정지 모드 플래그
    private float pauseEndTime;           // 일시 정지 종료 시각 (서버 타임 기준)


    public static GameTimer Instance { get; private set; }
    public bool IsInExtension => isInExtension;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
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
            isPaused = true;
            isIndefinitePause = true;
        }
        if (IsClient)
            remainingTime.OnValueChanged += HandleTimerUpdated;
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
            remainingTime.OnValueChanged -= HandleTimerUpdated;
    }

    private void Update()
    {
        if (!IsServer) return;

        // ■ 일시/무한 정지 중이면 타이머 동작 스킵
        if (isPaused)
        {
            if (isIndefinitePause)
                return;

            // 일시 정지 모드 종료 시점 도달했으면 해제
            float now = NetworkManager.Singleton.ServerTime.TimeAsFloat;
            if (now >= pauseEndTime)
                ResumeTimer();
            else
                return;
        }

        // ■ 평상시 타이머 갱신
        float nowTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        float newTime = isInExtension
            ? Mathf.Max(0, extensionDuration - (nowTime - extensionStartTime))
            : Mathf.Max(0, endTime - nowTime);

        if (Mathf.Abs(newTime - remainingTime.Value) > 0.1f)
            remainingTime.Value = newTime;

        if (newTime <= 0 && !isGameEnded)
            HandleGameEndLogic();
    }

    // 게임 종료 로직: 연장/승패 판별을 GameEnding의 EvaluateScores로 대체
    private void HandleGameEndLogic()
    {
        var scores = ScoreManager.Instance.GetAllScores();
        var (type, winnerpid, loserpid) = GameEnding.Instance.EvaluateScoresByPlayer(scores);

        switch (type)
        {
            case GameEnding.GameResultType.Extend:
                Debug.Log("연장 모드 시작");
                ExtendTime(totalGameTime * 0.25f);
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

    private void HandleTimerUpdated(float oldVal, float newVal)
    {
        OnTimerUpdated?.Invoke(newVal);
    }

    // 연장 시간 설정
    public void ExtendTime(float extraSeconds)
    {
        if (!IsServer) return;

        float now = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        if (!isInExtension)
        {
            isInExtension = true;
            extensionDuration = extraSeconds;
            extensionStartTime = now;
        }
        else
        {
            float left = extensionDuration - (now - extensionStartTime);
            extensionDuration = left + extraSeconds;
            extensionStartTime = now;
        }
    }

    public void StopForEndTimer()
    {
        isPaused = true;
        isIndefinitePause = true;
    }

    public void PauseTimer(float seconds)
    {
        isPaused = true;
        isIndefinitePause = false;
        pauseEndTime = NetworkManager.Singleton.ServerTime.TimeAsFloat + seconds;
    }

    public void ResumeTimer()
    {
        isPaused = false;
        isIndefinitePause = false;
    }

}
