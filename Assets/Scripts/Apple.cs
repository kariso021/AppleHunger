using UnityEngine;
using TMPro;

public class Apple : MonoBehaviour
{
    public int value; // ��� ��
    public int scorevalue; //��� �Ѱ��� ������ �ִ� ���ھ� ��ġ
    public TextMeshPro numberText; //3d ��ǥ��

    private void Start()
    {
        value = Random.Range(1, 10); // 1~9 ������ ���� �� ����
        numberText.text = value.ToString(); // UI�� ���� ǥ��
    }
}