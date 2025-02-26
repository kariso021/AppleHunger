using UnityEngine;
using System.Collections.Generic;

public class ClientDataManager : MonoBehaviour
{
    public static ClientDataManager Instance { get; private set; }

    public PlayerData playerData;
    public PlayerStatsData playerStatsData;
    public LoginData loginData;
    public Dictionary<int, MatchHistoryData> matchRecordsDataDictionary = new Dictionary<int, MatchHistoryData>();
    public List<PlayerItemData> playerItemDataList = new List<PlayerItemData>();
    public Dictionary<int, PlayerItemData> playerItemDataDictionary = new Dictionary<int, PlayerItemData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🔹 플레이어 데이터 저장
    public void SetPlayerData(PlayerData data)
    {
        playerData = data;
        Debug.Log($"[ClientDataManager] 플레이어 데이터 업데이트 완료: {playerData.playerName}");
        Debug.Log($"[ClientDataManager] 플레이어 데이터 업데이트 완료: {playerData.playerId}");
    }

    // 🔹 매치 기록 저장
    public void AddMatchHistory(MatchHistoryData matchData)
    {
        if (!matchRecordsDataDictionary.ContainsKey(matchData.matchId))
        {
            matchRecordsDataDictionary.Add(matchData.matchId, matchData);
            Debug.Log($"[ClientDataManager] 매치 기록 추가: Match ID {matchData.matchId}");
        }
    }

    // 🔹 아이템 데이터 저장
    public void AddPlayerItem(PlayerItemData itemData)
    {
        if (!playerItemDataDictionary.ContainsKey(itemData.itemUniqueId))
        {
            playerItemDataDictionary.Add(itemData.itemUniqueId, itemData);
            playerItemDataList.Add(itemData);
            Debug.Log($"[ClientDataManager] 아이템 추가: {itemData.itemUniqueId}");
        }
    }

    // 🔹 로그인 데이터 저장
    public void SetLoginData(LoginData login)
    {
        loginData = login;
        Debug.Log($"[ClientDataManager] 로그인 데이터 업데이트 완료");
    }

    // 🔹 플레이어 스탯 저장
    public void SetPlayerStats(PlayerStatsData stats)
    {
        playerStatsData = stats;
        Debug.Log($"[ClientDataManager] 플레이어 스탯 업데이트 완료");
    }
}
