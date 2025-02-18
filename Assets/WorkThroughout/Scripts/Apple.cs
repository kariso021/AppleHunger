using FishNet.Object;
using TMPro;
using UnityEngine;

public class Apple : NetworkBehaviour
{
    private int value; // 내부에서만 변경 가능
    private int scorevalue;

    public int Value => value; // Getter를 사용하여 외부에서 읽기 가능
    public int ScoreValue => scorevalue; // 점수 값도 읽기 가능

    public TextMeshPro numberText;

    public override void OnStartServer()
    {
        base.OnStartServer();
        value = Random.Range(1, 10); // 서버에서 랜덤 값 설정
        scorevalue = value * 10; // 점수 값 계산 후 동기화
        UpdateAppleObserversRpc(value, scorevalue); // 모든 클라이언트에 값 전송
    }

    [ObserversRpc] // 클라이언트들에게 값을 전달
    private void UpdateAppleObserversRpc(int newValue, int newScoreValue)
    {
        value = newValue;
        scorevalue = newScoreValue;

        // 🔍 numberText가 null인지 체크
        if (numberText != null)
        {
            numberText.text = value.ToString(); // UI 업데이트
        }
        else
        {
            Debug.LogError("🚨 numberText가 할당되지 않았습니다! Inspector에서 확인하세요.");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log($"🍏 Client: Apple spawned with value {value}");

        if (numberText != null)
        {
            numberText.text = value.ToString();
        }
        else
        {
            Debug.LogError("🚨 Client: numberText is null! Check Inspector.");
        }
    }

}
