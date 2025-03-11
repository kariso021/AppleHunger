using System.Collections.Generic;
using UnityEngine;

public class MatchRecordsManager : MonoBehaviour
{
    public GameObject matchDataListHolder; // 🔹 캔버스에서 생성할 목록의 부모 객체
    private List<GameObject> activeMatchRecords = new List<GameObject>(); // 🔹 활성화된 매치 UI 오브젝트 저장 리스트

    private void Start()
    {
        DataSyncManager.Instance.OnMatchHistoryChanged += UpdateMatchRecords;
    }

    private void OnDestroy()
    {
        // ✅ 이벤트 구독 해제 (메모리 누수 방지)
        DataSyncManager.Instance.OnMatchHistoryChanged -= UpdateMatchRecords;
    }

    // ✅ 전적 기록 UI 업데이트
    private void UpdateMatchRecords()
    {
        Debug.Log("🔄 [MatchRecordsManager] 전적 데이터 변경 감지 → UI 갱신");

        // ✅ SQLite 데이터가 최신화된 후 UI 갱신 실행
        Invoke(nameof(CreateMatchRecords), 0.5f);
    }

    // ✅ 프로필 팝업이 열릴 때 실행 (최대 10개 유지)
    public void CreateMatchRecords()
    {
        // 🔹 기존 오브젝트 초기화 (오래된 데이터 제거)
        ResetMatchRecords();

        List<MatchHistoryData> matchHistoryList = SQLiteManager.Instance.LoadMatchHistory();
        if (matchHistoryList.Count == 0) return;

        int maxRecords = 10; // 🔹 최대 표시 개수 (Match Record)
        int recordCount = 0;

        foreach (var match in matchHistoryList)
        {
            if (recordCount >= maxRecords) break; // ✅ 최대 개수 제한

            int opponentPlayerId = match.player1Id == SQLiteManager.Instance.player.playerId ? match.player2Id : match.player1Id;
            string opponentPlayerName = match.player1Id == opponentPlayerId ? match.player1Name : match.player2Name;
            int opponentPlayerRating = match.player1Id == opponentPlayerId ? match.player1Rating : match.player2Rating;
            string opponentPlayerIconUniqueId = match.player1Id == opponentPlayerId ? match.player1Icon : match.player2Icon;

            // ✅ Object Pool에서 가져오기
            GameObject matchInstance = ObjectPoolManager.Instance.GetFromPool("MatchRecord", Vector3.zero, Quaternion.identity, matchDataListHolder.transform);
            if (matchInstance == null) continue; // 오브젝트 풀에서 가져올 수 없으면 continue

            // ✅ 데이터 설정
            MatchData matchData = matchInstance.GetComponent<MatchData>();
            if (matchData != null)
            {
                matchData.SetMatchData(
                    match.winnerId,
                    opponentPlayerRating,
                    opponentPlayerName,
                    opponentPlayerIconUniqueId
                );
            }

            AddressableManager.Instance.matchIconObj.Add(matchData.gameObject);
            activeMatchRecords.Add(matchInstance); // 🔹 활성화된 오브젝트 리스트에 추가
            recordCount++;
        }

        AddressableManager.Instance.LoadMatchIconFromGroup();
    }

    // ✅ 기존 활성화된 매치 데이터 초기화 (비활성화 처리)
    private void ResetMatchRecords()
    {
        foreach (var obj in activeMatchRecords)
        {
            ObjectPoolManager.Instance.ReturnToPool("MatchRecord", obj);
        }
        activeMatchRecords.Clear();
    }
}
