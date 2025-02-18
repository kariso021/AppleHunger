using FishNet.Object;
using UnityEngine;

public class GameServer : NetworkBehaviour
{
    public float gameTime = 60f; // 전체 게임 시간
    private bool isGameOver = false;

    private AppleManager appleManager; // 사과 관리

    public override void OnStartServer() // ✅ FishNet의 서버 시작 이벤트 활용
    {
        base.OnStartServer();

        // 서버에서만 실행되도록 보장
        if (!IsServer)
        {
            enabled = false; // 🛑 서버가 아니면 비활성화
            return;
        }

        appleManager = GetComponent<AppleManager>();

        if (appleManager == null)
        {
            Debug.LogError("🚨 GameServer에서 AppleManager를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (!IsServer || isGameOver) return;

        gameTime -= Time.deltaTime;

        if (gameTime <= 0)
        {
            EndGame();
        }

        UpdateTimerObserversRpc(gameTime);
    }

    [ObserversRpc] // 🔹 모든 클라이언트에게 타이머 UI 업데이트
    private void UpdateTimerObserversRpc(float time)
    {
       
        Timer timer = FindObjectOfType<Timer>();
        if (timer != null)
        {
            timer.UpdateTimerUI(time);
        }
        else
        {
            Debug.LogError("Timer is Null!");
        }
    }

    private void EndGame()
    {
        isGameOver = true;
        ShowGameOverScreenObserversRpc();
    }

    [ObserversRpc] // 🔹 모든 클라이언트에게 게임 종료 알림
    private void ShowGameOverScreenObserversRpc()
    {
        UIManager.Instance.ShowGameOverScreen();
    }

 

    public AppleManager GetAppleManager()
    {
        return appleManager;
    }
}
