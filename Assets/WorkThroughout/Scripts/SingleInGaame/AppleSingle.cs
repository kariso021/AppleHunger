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

    // 20250505 Ž������
    public Bounds AppleBounds => new Bounds(
    transform.position,
    new Vector3(detectSize, detectSize, 1f)
    );


    
    private void Awake()
    {
        // 1~9 ���� �� ����
        value = UnityEngine.Random.Range(1, 10);
        scoreValue = 10;

        // UI ������Ʈ
        UpdateText();

        animator = GetComponent<Animator>();

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

    //�ִϸ��̼� ���ú� �۾�

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
            yield return new WaitForSeconds(0.3f); // �ִϸ��̼� ���̿� ����
        }

        onComplete?.Invoke();
    }



    private void UpdateText()
    {
        if (numberText != null)
            numberText.text = value.ToString();
        else
            Debug.LogError("numberText�� �Ҵ���� �ʾҽ��ϴ�! Inspector���� Ȯ���ϼ���.");
    }

}