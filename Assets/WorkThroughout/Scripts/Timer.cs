using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public int roomId;
    public Slider timerSlider; // 🎯 직접 참조

    private float maxTime;
    private float currentTime;
    private bool isRunning = false;

    public void InitializeTimer(float time)
    {
        maxTime = time;
        currentTime = maxTime;
        isRunning = false;

        UpdateSlider(); // UI 초기화
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        UpdateSlider(); // 🎯 UI 업데이트

        if (currentTime <= 0)
        {
            currentTime = 0;
            StopTimer();
            Debug.Log($"Room {roomId} 타이머 종료!");
        }
    }

    private void UpdateSlider()
    {
        if (timerSlider != null)
        {
            timerSlider.maxValue = maxTime;
            timerSlider.value = currentTime;
        }
        else
        {
            Debug.LogError($"Room {roomId}의 TimerSlider가 설정되지 않음!");
        }
    }
}
