using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class AppleSingle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshPro numberText;
    // 20250608 Select 아웃라인 설정
    public SpriteRenderer selectOutline;

    private int value;
    private int scoreValue = 10;

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public int Value => value;
    public int ScoreValue => scoreValue;

    private Animator animator;



    public float detectSize;

    // 20250505 탐지범위
    //public Bounds AppleBounds => new Bounds(
    //transform.position,
    //new Vector3(detectSize, detectSize, 1f)
    //);


    
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

    public void OnSelect()
    {
        selectOutline.color = new Color(255f, 255f, 255f, 255f);
    }
    public void OnDeselect()
    {
        selectOutline.color = new Color(255f, 255f, 255f, 0f);
    }

    public bool OverlapsBox(Bounds box)
    {
        // 원형 중심과 가장 가까운 box 위의 점을 구함
        Vector3 closest = box.ClosestPoint(transform.position);

        float radius = detectSize;
        float sqrDist = (transform.position - closest).sqrMagnitude;

        return sqrDist <= radius * radius;
    }
    private void OnDrawGizmosSelected()
    {
        float radius = detectSize;
        Vector3 center = transform.position;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < 3; i++)
        {
            Gizmos.DrawWireSphere(center, radius); // 굵게 보이게
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.05f); // 중심점 강조

        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + Vector3.right * radius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + Vector3.up * radius);
    }


}