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

    // 20250505 탐지범위
    public Bounds AppleBounds => new Bounds(
    transform.position,
    new Vector3(detectSize, detectSize, 1f)
    );


    
    private void Awake()
    {
        // 1~9 랜덤 값 설정
        value = Random.Range(1, 10);
        scoreValue = 10;

        // UI 업데이트
        UpdateText(); 
    }
    /// <summary>
    /// 값 수동 설정 (필요 시)
    /// </summary>
    public void SetValue(int someValue)
    {
        value = someValue;
        UpdateText();
    }

    /// <summary>
    /// 그리드 좌표 설정
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
            Debug.LogError("numberText가 할당되지 않았습니다! Inspector에서 확인하세요.");
    }

}