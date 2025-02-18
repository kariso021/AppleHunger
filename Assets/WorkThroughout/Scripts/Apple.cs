using FishNet.Object;
using TMPro;
using UnityEngine;

public class Apple : NetworkBehaviour
{
    private int value; // 내부에서만 변경 가능
    private int scorevalue;

    public int Value => value; // Getter를 사용하여 외부에서 읽기 가능
    public int ScoreValue => scorevalue; // 점수 값도 읽기 가능

    [SerializeField] private TextMeshPro numberText;

    public override void OnStartServer()
    {
        base.OnStartServer();
        value = Random.Range(1, 10); // 서버에서 랜덤 값 설정
        scorevalue = 10;
        UpdateAppleObserversRpc(value, scorevalue); // ✅ 모든 기존 클라이언트에 값 전송
    }

    [ObserversRpc(BufferLast = true)] // *******새로운 클라이언트도 최신 값 받도록 설정******** BufferLast = true 로 해주면 됨
    private void UpdateAppleObserversRpc(int newValue, int newScoreValue)
    {
        value = newValue;
        scorevalue = newScoreValue;
        UpdateText(); // ✅ UI 업데이트
    }

    public void SetValue(int newValue) // 클라이언트가 Apple의 Value 값을 업데이트할 수 있도록 설정
    {
        value = newValue;
        UpdateText();
    }

    private void UpdateText()
    {
        if (numberText != null)
        {
            numberText.text = value.ToString();
        }
        else
        {
            Debug.LogError("🚨 numberText가 할당되지 않았습니다! Inspector에서 확인하세요.");
        }
    }
}
