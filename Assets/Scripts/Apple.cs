using UnityEngine;
using TMPro;

public class Apple : MonoBehaviour
{
    public int value; // 사과 값
    public int scorevalue; //사과 한개가 가지고 있는 스코어 가치
    public TextMeshPro numberText; //3d 로표시

    private void Start()
    {
        value = Random.Range(1, 10); // 1~9 사이의 랜덤 값 설정
        numberText.text = value.ToString(); // UI에 숫자 표시
    }
}