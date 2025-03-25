using System.Collections.Generic;
using UnityEngine;

public class RankingRecordsManager : MonoBehaviour
{
    public GameObject myRankingData;
    public GameObject rankingDataListHolder;
    public GameObject rankProfilePopupGameObject; // 🔹 프로필 팝업 창 추가

    private int maxRankRecords = 50; // 🔹 최대 랭킹 표시 개수
    private Dictionary<int, RankingData> rankingObjects = new Dictionary<int, RankingData>(); // 🔹 기존 오브젝트 저장


    // 동일 등수에 대한 표기
    int recordCount = 0;
    private void Start()
    {
        // ✅ 랭킹 변경 이벤트 구독 (자동 갱신)
        DataSyncManager.Instance.OnPlayerRankingChanged += UpdateRankRecords;
        
    }

    // ✅ 랭킹 UI 업데이트
    public void CreateRankRecords()
    {
        List<PlayerRankingData> playerRankList = SQLiteManager.Instance.LoadRankings();
        if (playerRankList.Count == 0) return;

        // 🔹 기존 UI 숨기기 (재사용을 위해)
        foreach (var item in rankingObjects.Values)
        {
            item.gameObject.SetActive(false);
        }

        int recordCount = 0;

        foreach (var rankData in playerRankList)
        {
            if (recordCount >= maxRankRecords) break; // 최대 개수 제한

            RankingData rankingData;

            // 🔹 기존 오브젝트 재사용 (playerId 기준)
            if (rankingObjects.TryGetValue(rankData.playerId, out rankingData))
            {
                rankingData.gameObject.SetActive(true); // 기존 오브젝트 활성화
            }
            else
            {
                // 🔹 Object Pool에서 새 오브젝트 가져오기
                GameObject rankInstance = ObjectPoolManager.Instance.GetFromPool("RankingData", Vector3.zero, Quaternion.identity, rankingDataListHolder.transform);

                // 🔹 null 체크 (오브젝트 풀에서 가져오지 못한 경우 대비)
                if (rankInstance == null)
                {
                    Debug.LogError("❌ Object Pool에서 RankingData를 가져올 수 없습니다. Pool이 가득 찼을 가능성이 있음.");
                    continue;
                }

                rankingData = rankInstance.GetComponent<RankingData>();

                // 🔹 rankingData null 체크
                if (rankingData == null)
                {
                    Debug.LogError("❌ RankingData 컴포넌트를 찾을 수 없습니다.");
                    continue;
                }

                // 🔹 딕셔너리에 추가 (재사용을 위해)
                rankingObjects[rankData.playerId] = rankingData;

                AddressableManager.Instance.rankingIconObj.Add(rankingData.gameObject);
            }

            int displayRank = recordCount + 1;

            // 🔹 데이터 설정
            rankingData.SetRankingData(
                rankData.playerId,
                rankData.playerName,
                rankData.rating,
                rankData.rankPosition,
                rankData.profileIcon
            );

            

            recordCount++;
        }

        // 🔹 리스트를 랭킹 순서대로 정렬 후 UI에서 배치
        List<RankingData> sortedRankingObjects = new List<RankingData>(rankingObjects.Values);
        sortedRankingObjects.Sort((a, b) => int.Parse(a.rankText.text).CompareTo(int.Parse(b.rankText.text)));

        for (int i = 0; i < sortedRankingObjects.Count; i++)
        {
            sortedRankingObjects[i].transform.SetSiblingIndex(i);
        }

        // ✅ 내 랭킹 데이터 갱신
        myRankingData.GetComponent<RankingData>().SetRankingData(
            SQLiteManager.Instance.myRankingData.playerId,
            SQLiteManager.Instance.myRankingData.playerName,
            SQLiteManager.Instance.myRankingData.rating,
            SQLiteManager.Instance.myRankingData.rankPosition,
            SQLiteManager.Instance.myRankingData.profileIcon
        );

        if (!AddressableManager.Instance.rankingIconObj.Contains(myRankingData))
            AddressableManager.Instance.rankingIconObj.Add(myRankingData.gameObject);

        // 랭킹 프로필 이미지 갱신
        AddressableManager.Instance.LoadRankingIconFromGroup();
    }

    // ✅ 데이터 변경 시 자동 갱신
    private void UpdateRankRecords()
    {
        Debug.Log("🔄 [RankingRecordsManager] 랭킹 데이터 변경 감지 → UI 갱신");
        CreateRankRecords();
    }

    public void UpdateMyRankingRecords()
    {
        // ✅ 내 랭킹 데이터 갱신
        myRankingData.GetComponent<RankingData>().SetRankingData(
            SQLiteManager.Instance.myRankingData.playerId,
            SQLiteManager.Instance.myRankingData.playerName,
            SQLiteManager.Instance.myRankingData.rating,
            SQLiteManager.Instance.myRankingData.rankPosition,
            SQLiteManager.Instance.myRankingData.profileIcon
        );

        if (!AddressableManager.Instance.rankingIconObj.Contains(myRankingData))
            AddressableManager.Instance.rankingIconObj.Add(myRankingData.gameObject);
    }
}
