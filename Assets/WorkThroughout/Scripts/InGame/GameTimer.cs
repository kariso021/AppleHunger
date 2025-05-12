using UnityEngine;
using Unity.Netcode;
using System;

public class GameTimer : NetworkBehaviour
{
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private float totalGameTime = 60f;

    private float startTime;
    private float endTime;

    private bool isGameEnded = false;
    private bool isInExtension = false;
    private float extensionDuration = 0f;
    private float extensionStartTime = 0f;

    private bool isPaused = false;
    private bool isIndefinitePause = false;
    private float pauseEndTime = 0f;

    public static event Action OnGameEnded;
    public static event Action<float> OnTimerUpdated;

    public static GameTimer Instance { get; private set; }
    public bool IsInExtension => isInExtension;
    public float CurrentRemainingTime => remainingTime.Value;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    //시작 로직 타이머

    [ServerRpc(RequireOwnership = false)]
    public void StartTimerWithDelayServerRpc(float delaySeconds)
    {
        // 1) 타이머 초기화
        startTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        endTime = startTime + totalGameTime;
        remainingTime.Value = totalGameTime;
        isGameEnded = false;
        isInExtension = false;

        // 2) delaySeconds 만큼 일시 정지 → 이후 ResumeTimer() 호출
        isPaused = true;
        isIndefinitePause = false;
        pauseEndTime = startTime + delaySeconds;
    }


    public override void OnNetworkSpawn()
    {
        
        if (IsServer)
        {
            //자동시작 로직 다 제거해줘야함
            //-------------------------------------------------------------------------
            //startTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
            //endTime = startTime + totalGameTime;
            //remainingTime.Value = totalGameTime;
            //isGameEnded = false;
            //isInExtension = false;
            //// 자동 시작
            //isPaused = false;
            //isIndefinitePause = false;

            //-------------------------------------------------------------------------
        }

        if (IsClient)
        {
            remainingTime.OnValueChanged += HandleTimerUpdated;
            // 최초 값 즉시 UI 반영
            HandleTimerUpdated(remainingTime.Value, remainingTime.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
            remainingTime.OnValueChanged -= HandleTimerUpdated;
    }

    private void Update()
    {
        if (!IsServer) return;

        // 일시/무한 정지 처리
        if (isPaused)
        {
            if (isIndefinitePause) return;
            float now = NetworkManager.Singleton.ServerTime.TimeAsFloat;
            if (now < pauseEndTime) return;
            ResumeTimer();
        }

        // 남은 시간 계산
        float nowTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        float newTime = isInExtension
            ? Mathf.Max(0f, extensionDuration - (nowTime - extensionStartTime))
            : Mathf.Max(0f, endTime - nowTime);

        // 임계치 없이 매 틱 업데이트
        remainingTime.Value = newTime;

        if (newTime <= 0f && !isGameEnded)
            HandleGameEndLogic();
    }

    private void HandleTimerUpdated(float oldVal, float newVal)
    {
        OnTimerUpdated?.Invoke(newVal);
    }

    private void HandleGameEndLogic()
    {
        var scores = ScoreManager.Instance.GetAllScores();
        var (type, winnerpid, loserpid) = GameEnding.Instance.EvaluateScoresByPlayer(scores);

        switch (type)
        {
            case GameEnding.GameResultType.Extend:
                ExtendTime(totalGameTime * 0.25f);
                break;
            case GameEnding.GameResultType.Draw:
                isGameEnded = true;
                OnGameEnded?.Invoke();
                break;
            default:
                isGameEnded = true;
                OnGameEnded?.Invoke();
                break;
        }
    }

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
