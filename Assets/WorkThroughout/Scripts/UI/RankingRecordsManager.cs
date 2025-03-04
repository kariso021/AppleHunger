using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingRecordsManager : MonoBehaviour
{
    public GameObject myRankingData;
    public GameObject rankingDataPrefab;
    public GameObject rankingDataListHoler;

    public GameObject rankProfilePopupGameObject;
    /// <summary>
    /// 랭킹 탭에 랭킹을 나열하는 함수
    /// </summary>
    // 필요한 변수? playerId -> players -> playerName,rating,icon
    public void CreateRankRecords()
    {
        List<PlayerRankingData> playerRankList = SQLiteManager.Instance.LoadRankings();

        if (playerRankList.Count == 0) return;

        foreach (var rankData in playerRankList)
        {
            
            // 🔹 프리팹 인스턴스 생성
            GameObject rankInstance = Instantiate(rankingDataPrefab, rankingDataListHoler.transform);

            rankInstance.GetComponent<RankingData>().SetRankingData(
                rankData.playerId,
                rankData.playerName,
                rankData.rating,
                rankData.rankPosition,
                rankData.profileIcon
                );
        }

        myRankingData.GetComponent<RankingData>().SetRankingData(
            SQLiteManager.Instance.myRankingData.playerId,
            SQLiteManager.Instance.myRankingData.playerName,
            SQLiteManager.Instance.myRankingData.rating,
            SQLiteManager.Instance.myRankingData.rankPosition,
            SQLiteManager.Instance.myRankingData.profileIcon
            );

    }
}
