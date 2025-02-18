//구버전 콤보 매니저 여기있는 로직을 ScoreManager에 넣을것임


using System.Collections;
using UnityEngine;

/// <summary>
/// 콤보 시스템에 관련된 매니저
/// </summary>
/// 
// 사과를 만들면 콤보 시스템 시작, 
public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    public int comboBasicScore = 1; // 임시로 설정, 수치는 알아서 조정하면 됨
    public int comboCount { get; set; } = 0; // Max = 5
    private int timeToMaintainCombo = 5; // 이 시간 내에 사과를 만들어야 콤보 유지 가능
    public bool isAppleMaking { get; set; } = false;
    private Coroutine comboCoroutine = null; // 현재 실행 중인 코루틴을 저장할 변수

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
        float timer = 0f; // 타이머   

        while(timer <= timeToMaintainCombo)
        {
            timer += Time.deltaTime;
            if(isAppleMaking)
            {
                timer = 0f; // 콤보 유지 시간을 초기화하여 연장
                isAppleMaking = false;
            }
            yield return null;
        }
        comboCount = 0; // 콤보 초기화
        comboCoroutine = null;
        yield return null;

    }

    public void StartCombo()
    {
        if(comboCoroutine == null)
            comboCoroutine = StartCoroutine(OnCombo());
    }

}
