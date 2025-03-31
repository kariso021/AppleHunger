using System.Collections;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    private float interval = 10f; //60f * 60f * 3; // 테스트용 70초 마다 주기 //60f * 10f; // 10분
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
    //
    //즉, 구조 이해는 이렇다
    //먼저 여기서 interval = 말그대로 서버에 야야 업데이트 하고 얼마나 지났어? 물어보는거고
    //웹에 접속해서 물어보면 마지막 업데이트 시간으로부터 api서버에서 지정한 인터벌 만큼 지났는지
    //체크하고 만약 지났다면 true를 반환해 랭킹 업데이트가 이뤄진다
    //랭킹 업데이트가 이뤄지면 마지막 갱신 시간이 최신화된다
    //server.js에서 cron으로 업데이트 하는 갱신 시간은 말 그대로 갱신 시간만을 체크하는 느낌

}
