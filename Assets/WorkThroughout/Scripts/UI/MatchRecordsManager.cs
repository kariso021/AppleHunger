using System.Collections.Generic;
using UnityEngine;

public class MatchRecordsManager : MonoBehaviour
{
    public GameObject matchDataPrefab;
    public GameObject matchDataListHolder; // 캔버스에서 생성할 목록의 부모 객체

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// 프로필 팝업창에 전적 리스트를 만드는 함수
    /// </summary>
    // 필요한 변수? playerId -> players -> playerName,rating,icon
    public void CreateMatchRecords()
    {
        List<MatchHistoryData> matchHistoryList = SQLiteManager.Instance.LoadMatchHistory();

        if (matchHistoryList.Count == 0) return;

        foreach (var match in matchHistoryList)
        {
            // 상대 플레이어 ID 찾기
            int opponentPlayerId = match.player1Id == SQLiteManager.Instance.player.playerId ? match.player2Id : match.player1Id;

            // 상대 플레이어 정보 설정
            string opponentPlayerName = match.player1Id == opponentPlayerId ? match.player1Name : match.player2Name;
            int opponentPlayerRating = match.player1Id == opponentPlayerId ? match.player1Rating : match.player2Rating;
            string opponentPlayerIconUniqueId = match.player1Id == opponentPlayerId ? match.player1Icon : match.player2Icon;

            // 🔹 프리팹 인스턴스 생성
            GameObject matchInstance = Instantiate(matchDataPrefab, matchDataListHolder.transform);

            // 🔹 MatchData 스크립트 가져와서 데이터 설정
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
        }
    }

}
