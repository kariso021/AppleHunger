using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float gameTime = 60f; // 전체 게임 시간
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
            Debug.LogError("Timer Instance가 존재하지 않습니다. Timer 오브젝트가 씬에 있는지 확인하세요!");
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        gameTime -= Time.deltaTime;

        // Timer 인스턴스가 존재하는 경우에만 업데이트 실행
        if (Timer.Instance != null)
        {
            Timer.Instance.UpdateTimerUI(gameTime); // 여기서 오류 발생했었음 (이제 정상 작동)
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
        Debug.Log("게임 종료!");
        UIManager.Instance.ShowGameOverScreen();
    }
}
