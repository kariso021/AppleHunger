using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public static Timer Instance;
    public Slider timerSlider; // 타이머 슬라이더 UI

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        timerSlider.maxValue = GameManager.Instance.gameTime; // 최대값 설정
        timerSlider.value = GameManager.Instance.gameTime; // 시작 값 설정
    }



    public void UpdateTimerUI(float currentTime)
    {
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;
        }
        else
        {
            Debug.LogError(" TimerSlider가 연결되지 않았습니다! Inspector에서 확인하세요.");
        }
    }
}