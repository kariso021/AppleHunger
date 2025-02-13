using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float gameTime = 60f; // ��ü ���� �ð�
    public int score = 0;
    private bool isGameOver = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Timer.Instance == null)
        {
            Debug.LogError("Timer Instance�� �������� �ʽ��ϴ�. Timer ������Ʈ�� ���� �ִ��� Ȯ���ϼ���!");
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        gameTime -= Time.deltaTime;

        // Timer �ν��Ͻ��� �����ϴ� ��쿡�� ������Ʈ ����
        if (Timer.Instance != null)
        {
            Timer.Instance.UpdateTimerUI(gameTime); // ���⼭ ���� �߻��߾��� (���� ���� �۵�)
        }

        if (gameTime <= 0)
        {
            EndGame();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UIManager.Instance.UpdateScore(score);
    }

    private void EndGame()
    {
        isGameOver = true;
        Debug.Log("���� ����!");
        UIManager.Instance.ShowGameOverScreen();
    }
}
