using UnityEngine;
using Unity.Netcode;
using System;
using static UnityEngine.Rendering.DebugUI.Table;
using static GameEnding;
using System.Linq;
using System.Collections;

public class GameTimer : NetworkBehaviour
{
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private float totalGameTime = 60f;
    [SerializeField] private float readyGameTime = 5f; // 준비 시간

    // 준비 시간 후 한 번만 사과 스폰 여부 플래그
    private bool hasSpawnedApples = false;

    private float startTime;
    private float endTime;

    private bool isGameEnded = false;
    private bool isInExtension = false;
    private float extensionDuration = 0f;
    private float extensionStartTime = 0f;

    private bool isPaused = false;
    private bool isIndefinitePause = false;

    private float pauseStartTime = 0f; // 일시 정지 시작 시간
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
        remainingTime.Value = totalGameTime + readyGameTime;
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
            startTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
            endTime = startTime + totalGameTime+ readyGameTime;
            remainingTime.Value = totalGameTime + readyGameTime;
            isGameEnded = false;
            isInExtension = false;

            hasSpawnedApples = false;

            // 자동 시작
            isPaused = false;
            isIndefinitePause = false;

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
            float pausedDuration = pauseEndTime - pauseStartTime;
            endTime += pausedDuration;
            if (isInExtension)
                extensionStartTime += pausedDuration; // 연장 시간도 일시 정지 동안 늘려줌
            ResumeTimer();



        }

        // 남은 시간 계산
        float nowTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        float newTime = isInExtension
            ? Mathf.Max(0f, extensionDuration - (nowTime - extensionStartTime))
            : Mathf.Max(0f, endTime - nowTime);

        // ── 준비 시간이 지난 뒤 단 한 번만 사과 스폰 ──
        // now >= startTime + readyGameTime 이고, 아직 스폰 안 했다면
        if (!hasSpawnedApples && nowTime >= startTime + readyGameTime)
        {
            AppleManager.Instance.SpawnApplesInGrid();
            hasSpawnedApples = true;
        }



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
                // 이부분 extension 에서 꺼줬음 그래서 판넬 다시 나올거임
                isInExtension = false;
                isGameEnded = true;
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
        isInExtension = false;
    }

    public void PauseTimer(float seconds)
    {
        isPaused = true;
        isIndefinitePause = false;
        //이부분이 잘못됐군
        pauseStartTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        pauseEndTime = pauseStartTime + seconds;
    }

    public void ResumeTimer()
    {
        isPaused = false;
        isIndefinitePause = false;
    }

 




}
