﻿using System;
using System.Collections;
using UnityEngine;

public class DataSyncManager : MonoBehaviour
{
    private static DataSyncManager instance;
    public static DataSyncManager Instance => instance;

    // ✅ 이벤트 정의
    public event Action OnMatchHistoryChanged;
    public event Action OnPlayerRankingChanged;
    public event Action OnPlayerItemsChanged;
    public event Action OnPlayerProfileChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        // ✅ Start()에서 SQLiteManager의 이벤트 등록 (Awake() 순서 문제 해결)
        if (SQLiteManager.Instance != null)
        {
            SQLiteManager.Instance.OnSQLiteDataLoaded += InvokeUIUpdateEvents;
        }
        else
        {
            Debug.LogError("[DataSyncManager] SQLiteManager 인스턴스를 찾을 수 없음!");
        }
    }
    // 🔹 플레이어 데이터가 변경되었을 때 호출 (예: 이름, 프로필, 재화 등)
    public void PlayerDataUpdated()
    {
        Debug.Log("[DataSyncManager] 플레이어 데이터 변경 감지 → MySQL에서 최신 데이터 가져오기");
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerData("playerId", SQLiteManager.Instance.player.playerId.ToString(), false));

        // ✅ 동기화가 완료된 후, SQLite에 반영
        PopupManager.Instance.Invoke(nameof(PopupManager.Instance.ClosePopup), 1.0f);
        Invoke(nameof(SyncSQLite), 1.0f); // 1초 후 SQLite 갱신
    }

    // 🔹 플레이어 아이템이 변경되었을 때 호출 (예: 아이템 구매, 해금)
    public void PlayerItemsUpdated()
    {
        Debug.Log("[DataSyncManager] 플레이어 아이템 변경 감지 → MySQL에서 최신 데이터 가져오기");
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerItems(SQLiteManager.Instance.player.playerId)); 

        // ✅ 동기화가 완료된 후, SQLite에 반영
        Invoke(nameof(SyncSQLite), 1.0f);
    }

    // 🔹 매치 기록이 변경되었을 때 호출 (예: 경기 종료 후 업데이트)
    public void MatchHistoryUpdated()
    {
        Debug.Log("[DataSyncManager] 매치 기록 변경 감지 → MySQL에서 최신 데이터 가져오기");
        StartCoroutine(ClientNetworkManager.Instance.GetMatchRecords(SQLiteManager.Instance.player.playerId));

        // ✅ 동기화가 완료된 후, SQLite에 반영
        //Invoke(nameof(SyncSQLite), 1.0f);
    }

    // 🔹 플레이어 스탯이 변경되었을 때 호출 (예: 승/패 증가)
    public void PlayerStatsUpdated()
    {
        Debug.Log("[DataSyncManager] 플레이어 스탯 변경 감지 → MySQL에서 최신 데이터 가져오기");
        StartCoroutine(ClientNetworkManager.Instance.GetPlayerStats(SQLiteManager.Instance.player.playerId));

        // ✅ 동기화가 완료된 후, SQLite에 반영
        //Invoke(nameof(SyncSQLite), 1.0f);
    }

    // 🔹 랭킹 정보가 변경되었을 때 호출 (예: 레이팅 변화)
    public IEnumerator PlayerRankingUpdated()
    {
        Debug.Log("[DataSyncManager] 플레이어 랭킹 변경 감지 → MySQL에서 최신 데이터 가져오기");
        yield return ClientNetworkManager.Instance.GetRankingList();
  

        // ✅ 동기화가 완료된 후, SQLite에 반영
        Invoke(nameof(SyncSQLite), 1.0f);
    }

    // 🔹 SQLite 데이터 최신화 실행
    private void SyncSQLite()
    {
        Debug.Log("[DataSyncManager] SQLite 최신 데이터 동기화 실행...");
        SQLiteManager.Instance.LoadAllData();
    }

    private void InvokeUIUpdateEvents()
    {
        Debug.Log("[DataSyncManager] SQLite 동기화 완료 → UI 업데이트 시작");

        // ✅ UI 갱신 이벤트 실행 (모든 UI 업데이트 트리거)
        OnMatchHistoryChanged?.Invoke();
        //OnPlayerRankingChanged?.Invoke();
        OnPlayerItemsChanged?.Invoke(); // ✅ 아이템 UI 업데이트 트리거
        OnPlayerProfileChanged?.Invoke(); // 프로필 UI 업데이트 트리거
    }


    public void InvokeUIRankingUpdateEvent()
    {
        Debug.Log("[DataSyncManager] Ranking UI Update");

        OnPlayerRankingChanged?.Invoke();
    }
}
