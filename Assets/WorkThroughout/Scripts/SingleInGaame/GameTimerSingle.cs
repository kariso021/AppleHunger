using UnityEngine;
using System;
using UnityEditor.Rendering;

public class GameTimerSingle : MonoBehaviour
{
    public static GameTimerSingle Instance { get; private set; }

    /// <summary>
    /// 게임 시작부터의 총 플레이 시간 (초)
    /// </summary>
    /// 
    [SerializeField] private bool hasReleasedDrag = false; // 드래그 제한 해제 여부
    [SerializeField] private float totalGameTime = 60f;
    [SerializeField] private float readyGameTime = 5f; // 준비 시간

    public float ReadyTime
    {
        get => readyGameTime;
        set => readyGameTime = Mathf.Max(0f, value);
    }

    public float totalGameTimeInSeconds
    {
        get => totalGameTime;
        set => totalGameTime = Mathf.Max(0f, value);
    }


    /// <summary>
    /// 남은 시간 (초)
    /// </summary>
    private float remainingTime;

    /// <summary>
    /// 한 번만 종료 이벤트를 내리기 위한 플래그
    /// </summary>
    private bool isGameEnded = false;

    /// <summary>
    /// 타이머 업데이트 시마다 전달 (remainingTime)
    /// </summary>
    public static event Action<float> OnTimerUpdated;

    /// <summary>
    /// 시간이 0이 되어 게임이 끝났을 때 전달
    /// </summary>
    public static event Action OnGameEnded;


    private bool isPaused = false;
    private float pauseTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 타이머 초기화 및 첫 UI 업데이트
        remainingTime = totalGameTime + readyGameTime;
        OnTimerUpdated?.Invoke(remainingTime);
    }

    private void Update()
    {
        if (isGameEnded) return;

        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
            }
            return; // 멈춰있는 동안 시간 감소 안 시킴
        }

        if(!hasReleasedDrag &&remainingTime <= totalGameTime)
        {
            hasReleasedDrag = true;
            PlayerControllerSingle.Instance.RestrictAndRelease_When_Start_And_End(true);
        }


        // 남은 시간 감소
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            PlayerControllerSingle.Instance.RestrictAndRelease_When_Start_And_End(false);
           remainingTime = 0f;
            OnTimerUpdated?.Invoke(remainingTime);
            isGameEnded = true;
            OnGameEnded?.Invoke();
        }
        else
        {
            OnTimerUpdated?.Invoke(remainingTime);
        }
    }

    /// 남은 시간에 추가 시간을 더함
    /// <param name="extraSeconds">추가할 초 단위 시간</param>
    public void ExtendTime(float extraSeconds)
    {
        if (isGameEnded) return;

        remainingTime += extraSeconds;
        OnTimerUpdated?.Invoke(remainingTime);
    }

    /// <summary>
    /// 남은 시간을 가져옵니다.
    /// </summary>
    public float GetRemainingTime() => remainingTime;

    //------------------------------------PauseTiemr
    public void PauseTimerForSeconds(float seconds)
    {
        isPaused = true;
        pauseTimer = seconds;
    }
}