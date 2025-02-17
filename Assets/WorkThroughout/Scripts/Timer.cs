using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Slider timerSlider; // 타이머 슬라이더 UI

    private GameServer gameServer; // 🟢 GameServer에서 시간 가져오기

    private void Start()
    {
        gameServer = FindObjectOfType<GameServer>(); // 🟢 GameServer 찾기

        if (gameServer == null)
        {
            Debug.LogError("🚨 GameServer를 찾을 수 없습니다! Hierarchy에 추가했는지 확인하세요.");
            return;
        }

        timerSlider.maxValue = gameServer.gameTime; // 🟢 GameServer의 게임 시간 설정
        timerSlider.value = gameServer.gameTime;
    }

    public void UpdateTimerUI(float currentTime)
    {
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;
        }
        else
        {
            Debug.LogError("🚨 TimerSlider가 연결되지 않았습니다! Inspector에서 확인하세요.");
        }
    }
}