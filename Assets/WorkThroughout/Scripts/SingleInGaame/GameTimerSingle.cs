using UnityEngine;
using System;

public class GameTimerSingle : MonoBehaviour
{
    public static GameTimerSingle Instance { get; private set; }

    /// <summary>
    /// ���� ���ۺ����� �� �÷��� �ð� (��)
    /// </summary>
    [SerializeField] private float totalGameTime = 60f;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ÿ�̸� �ʱ�ȭ �� ù UI ������Ʈ
        remainingTime = totalGameTime;
        OnTimerUpdated?.Invoke(remainingTime);
    }

    private void Update()
    {
        if (isGameEnded) return;

        // ���� �ð� ����
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
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

    /// <summary>
    /// ���� �ð��� �߰� �ð��� ���մϴ�.
    /// </summary>
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
}