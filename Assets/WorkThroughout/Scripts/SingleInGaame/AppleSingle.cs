using UnityEngine;
using TMPro;
using System;
using System.Collections;

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

    private Animator animator;



    public float detectSize;

    // 20250505 탐지범위
    public Bounds AppleBounds => new Bounds(
    transform.position,
    new Vector3(detectSize, detectSize, 1f)
    );


    
    private void Awake()
    {
        // 1~9 랜덤 값 설정
        value = UnityEngine.Random.Range(1, 10);
        scoreValue = 10;

        // UI 업데이트
        UpdateText();

        animator = GetComponent<Animator>();

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

    //애니메이션 관련부 작업

    public void PlaySpawnAnimation()
    {
        if (animator != null)
        {
            animator.Play("AppleSpawn", 0, 0f);
        }
    }

    public void PlayDestroyAnimation(Action onComplete)
    {
        StartCoroutine(DestroyAfterAnimation(onComplete));
    }

    private IEnumerator DestroyAfterAnimation(Action onComplete)
    {
        if (animator != null)
        {
            animator.Play("AppleDestroy", 0, 0f);
            yield return new WaitForSeconds(0.3f); // 애니메이션 길이와 동일
        }

        onComplete?.Invoke();
    }



    private void UpdateText()
    {
        if (numberText != null)
            numberText.text = value.ToString();
        else
            Debug.LogError("numberText가 할당되지 않았습니다! Inspector에서 확인하세요.");
    }

}