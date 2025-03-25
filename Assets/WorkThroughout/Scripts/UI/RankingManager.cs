using System.Collections;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    [SerializeField] private float interval = 70f; // 테스트용 70초 마다 주기 //60f * 10f; // 10분
    private Coroutine updateRoutine;

    private void OnEnable()
    {
        updateRoutine = StartCoroutine(WaitForInitThenStartRoutine());
    }

    private void OnDisable()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }
    private IEnumerator WaitForInitThenStartRoutine()
    {
        // ✅ ServerToAPIManager가 준비될 때까지 대기
        while (ServerToAPIManager.Instance == null || SQLiteManager.Instance == null || SQLiteManager.Instance.player == null)
        {
            Debug.Log("[RankingManager] ServerToAPIManager 초기화 대기 중...");
            yield return null;
        }

        Debug.Log(" ServerToAPIManager 초기화 완료 -> 랭킹 폴링 시작");

        // 이제 안전하게 루틴 실행
        yield return StartCoroutine(AutoUpdateRankingRoutine());
    }
    private IEnumerator AutoUpdateRankingRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(ServerToAPIManager.Instance.CheckRankingShouldUpdate());
            yield return new WaitForSeconds(interval);
        }
    }


}
