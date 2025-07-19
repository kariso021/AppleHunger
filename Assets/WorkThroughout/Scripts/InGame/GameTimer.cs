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
    [SerializeField] private float initialDelay;

    bool hasTimerStarted = false; // 타이머 시작 여부

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


    //시작전 일시정지 로직
    public bool IsControlEnabled => NetworkManager.Singleton.ServerTime.TimeAsFloat >= (startTime + initialDelay);

    private NetworkVariable<bool> canControl = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);
    public bool CanControl => canControl.Value;



    public static GameTimer Instance { get; private set; }
    public bool IsInExtension => isInExtension;
    public float CurrentRemainingTime => remainingTime.Value;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    //시작 로직 타이머
    public void StartTimerWithDelay(float introduration, float countduration)
    {
        float now = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        hasTimerStarted = true;                // (선택) Update() 활성화 플래그
        initialDelay = introduration+countduration;        // 패널 표시 시간

        startTime = now;
        endTime = now + initialDelay + totalGameTime;
        remainingTime.Value = initialDelay + totalGameTime;

        // 딜레이 모드
        isPaused = true;
        isIndefinitePause = false;
        pauseStartTime = now;                 // ← 꼭 설정
        pauseEndTime = now + introduration + countduration;  // 패널 표시가 끝나는 시점
    }


    public override void OnNetworkSpawn()
    {
    
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
        if (isIndefinitePause || !hasTimerStarted)
        {
            remainingTime.Value = 60.0f;
            return;
        }


        float now = NetworkManager.Singleton.ServerTime.TimeAsFloat;

        // 1) 패널 딜레이 페이즈
        if (isPaused)
        {
            if (now < pauseEndTime) return;
            isPaused = false;
        }

        Debug.Log("Pause 벗어남");

        // 2) 사과 스폰 & 컨트롤 허용 (한 번만)
        if (!hasSpawnedApples)
        {
            AppleManager.Instance.SpawnApplesInGrid();
            hasSpawnedApples = true;
            canControl.Value = true;
            GameEnding.Instance.RestictControllerWhenStartClientRpc(true);
        }

        // 3) 남은 시간 계산 & 동기화
        float newTime = Mathf.Max(0f, endTime - now);
        remainingTime.Value = newTime;

        // 4) 타임업 시 한 번만 종료
        if (newTime <= 0f)
        {
            OnGameEnded?.Invoke();
            enabled = false;
        }
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
