using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuckingFinder : MonoBehaviour
{
    // DataCreateManager GameObj
    MatchRecordsManager matchRecordsManager;
    RankingRecordsManager rankingRecordsManager;
    ItemManager itemManager;

    // AddressableManager GameObj
    AddressableManager addressableManager;

    private void Awake()
    {
        Debug.Log("씨빠아아아아아");
        InitFinder();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void InitFinder()
    {
        matchRecordsFinder();
        rankingRecordsFinder();
        itemManagerFinder();
        addressableManagerFinder();
    }
    private void matchRecordsFinder()
    {
        matchRecordsManager = FindAnyObjectByType<MatchRecordsManager>();
        if(matchRecordsManager != null )
        {
            matchRecordsManager.matchDataListHolder =
                GameObject.Find("MatchDataListHolderGameObject");
        }
    }
    private void rankingRecordsFinder()
    {
        rankingRecordsManager = FindAnyObjectByType<RankingRecordsManager>();
        if (rankingRecordsManager != null)
        {
            rankingRecordsManager.myRankingData =
                GameObject.Find("MyRankingDataGameObject");
            rankingRecordsManager.rankingDataListHolder =
                GameObject.Find("RankingDataListHolderGameObject");
            rankingRecordsManager.rankProfilePopupGameObject =
                GameObject.Find("RankProfilePopupPanel");
        }
    }
    private void itemManagerFinder()
    {
        itemManager = FindAnyObjectByType<ItemManager>();
        if (itemManager != null)
        {
            itemManager.itemDataIconListHolder =
                GameObject.Find("ItemDataIconListHolderGameObject");
            itemManager.itemDataBoardListHolder =
                GameObject.Find("ItemDataBoardListHolderGameObject");
            itemManager.currentItemIcon =
                GameObject.Find("CurrentIconGameObject");
            itemManager.currentItemBoard =
                GameObject.Find("CurrentBoardGameObject");
        }
    }
    private void addressableManagerFinder()
    {
        addressableManager = AddressableManager.Instance;
        if (addressableManager != null)
        {
            addressableManager.profileIcon =
                GameObject.Find("ProfileIconButtonGameObject").GetComponent<Image>();
            addressableManager.profilePopupIcon =
                GameObject.Find("ProfilePopupIconGameObject").GetComponent<Image>();
            addressableManager.rankProfilePopupIcon =
                GameObject.Find("RankProfilePopupIconGameObject").GetComponent<Image>();
            addressableManager.myRankProfileIcon =
                GameObject.Find("MyRankProfileIconGameObject").GetComponent<Image>();
        }

    }

}
