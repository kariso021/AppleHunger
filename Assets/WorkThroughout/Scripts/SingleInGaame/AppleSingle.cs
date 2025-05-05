using UnityEngine;
using TMPro;

public class AppleSingle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshPro numberText;

    private int value;
    private int scoreValue = 10;

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public int Value => value;
    public int ScoreValue => scoreValue;


    public float detectSize;

    // 20250505 Ž������
    public Bounds AppleBounds => new Bounds(
    transform.position,
    new Vector3(detectSize, detectSize, 1f)
    );


    
    private void Awake()
    {
        // 1~9 ���� �� ����
        value = Random.Range(1, 10);
        scoreValue = 10;

        // UI ������Ʈ
        UpdateText(); 
    }
    /// <summary>
    /// �� ���� ���� (�ʿ� ��)
    /// </summary>
    public void SetValue(int someValue)
    {
        value = someValue;
        UpdateText();
    }

    /// <summary>
    /// �׸��� ��ǥ ����
    /// </summary>
    public void SetGridPosition(int y, int x)
    {
        GridX = x;
        GridY = y;
    }

    private void UpdateText()
    {
        if (numberText != null)
            numberText.text = value.ToString();
        else
            Debug.LogError("numberText�� �Ҵ���� �ʾҽ��ϴ�! Inspector���� Ȯ���ϼ���.");
    }

}