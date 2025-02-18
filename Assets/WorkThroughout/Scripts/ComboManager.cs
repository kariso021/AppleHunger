//������ �޺� �Ŵ��� �����ִ� ������ ScoreManager�� ��������


using System.Collections;
using UnityEngine;

/// <summary>
/// �޺� �ý��ۿ� ���õ� �Ŵ���
/// </summary>
/// 
// ����� ����� �޺� �ý��� ����, 
public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    public int comboBasicScore = 1; // �ӽ÷� ����, ��ġ�� �˾Ƽ� �����ϸ� ��
    public int comboCount { get; set; } = 0; // Max = 5
    private int timeToMaintainCombo = 5; // �� �ð� ���� ����� ������ �޺� ���� ����
    public bool isAppleMaking { get; set; } = false;
    private Coroutine comboCoroutine = null; // ���� ���� ���� �ڷ�ƾ�� ������ ����

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator OnCombo()
    {
        float timer = 0f; // Ÿ�̸�   

        while(timer <= timeToMaintainCombo)
        {
            timer += Time.deltaTime;
            if(isAppleMaking)
            {
                timer = 0f; // �޺� ���� �ð��� �ʱ�ȭ�Ͽ� ����
                isAppleMaking = false;
            }
            yield return null;
        }
        comboCount = 0; // �޺� �ʱ�ȭ
        comboCoroutine = null;
        yield return null;

    }

    public void StartCombo()
    {
        if(comboCoroutine == null)
            comboCoroutine = StartCoroutine(OnCombo());
    }

}
