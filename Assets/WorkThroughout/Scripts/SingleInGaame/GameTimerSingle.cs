using UnityEngine;
using System;
using UnityEditor.Rendering;

public class GameTimerSingle : MonoBehaviour
{
    public static GameTimerSingle Instance { get; private set; }

    /// <summary>
    /// ���� ���ۺ����� �� �÷��� �ð� (��)
    /// </summary>
    /// 
    [SerializeField] private bool hasReleasedDrag = false; // �巡�� ���� ���� ����
    [SerializeField] private float totalGameTime = 60f;
    [SerializeField] private float readyGameTime = 5f; // �غ� �ð�

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
    /// ���� �ð� (��)
    /// </summary>
    private float remainingTime;

    /// <summary>
    /// �� ���� ���� �̺�Ʈ�� ������ ���� �÷���
    /// </summary>
    private bool isGameEnded = false;

    /// <summary>
    /// Ÿ�̸� ������Ʈ �ø��� ���� (remainingTime)
    /// </summary>
    public static event Action<float> OnTimerUpdated;

    /// <summary>
    /// �ð��� 0�� �Ǿ� ������ ������ �� ����
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
        // Ÿ�̸� �ʱ�ȭ �� ù UI ������Ʈ
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
            return; // �����ִ� ���� �ð� ���� �� ��Ŵ
        }

        if(!hasReleasedDrag &&remainingTime <= totalGameTime)
        {
            hasReleasedDrag = true;
            PlayerControllerSingle.Instance.RestrictAndRelease_When_Start_And_End(true);
        }


        // ���� �ð� ����
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

    /// ���� �ð��� �߰� �ð��� ����
    /// <param name="extraSeconds">�߰��� �� ���� �ð�</param>
    public void ExtendTime(float extraSeconds)
    {
        if (isGameEnded) return;

        remainingTime += extraSeconds;
        OnTimerUpdated?.Invoke(remainingTime);
    }

    /// <summary>
    /// ���� �ð��� �����ɴϴ�.
    /// </summary>
    public float GetRemainingTime() => remainingTime;

    //------------------------------------PauseTiemr
    public void PauseTimerForSeconds(float seconds)
    {
        isPaused = true;
        pauseTimer = seconds;
    }
}