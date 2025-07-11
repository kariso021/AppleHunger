﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using appleHunger;
public class ClientNetworkManager : MonoBehaviour
{
    private static ClientNetworkManager instance;
    public static ClientNetworkManager Instance => instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임이 진행하는 동안엔 삭제가 일어나면 안되므로
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    #region DataSync
    // ✅ 플레이어 데이터 동기화
    private void SyncPlayerData()
    {
        Debug.Log("🔄 [Client] 플레이어 데이터 자동 동기화 시작...");
        StartCoroutine(GetPlayerData("playerId", SQLiteManager.Instance.player.playerId.ToString(),false));
    }

    // ✅ 플레이어 아이템 동기화
    private void SyncPlayerItems()
    {
        Debug.Log("🔄 [Client] 플레이어 아이템 자동 동기화 시작...");
        StartCoroutine(GetPlayerItems(SQLiteManager.Instance.player.playerId));
    }

    // ✅ 플레이어 스탯 동기화
    private void SyncPlayerStats()
    {
        Debug.Log("🔄 [Client] 플레이어 스탯 자동 동기화 시작...");
        StartCoroutine(GetPlayerStats(SQLiteManager.Instance.player.playerId));
    }

    // ✅ 랭킹 동기화
    private void SyncPlayerRanking()
    {
        Debug.Log("🔄 [Client] 랭킹 데이터 자동 동기화 시작...");
        StartCoroutine(GetRankingList());
    }

    // ✅ 매치 기록 동기화
    private void SyncMatchHistory()
    {
        Debug.Log("🔄 [Client] 매치 기록 자동 동기화 시작...");
        StartCoroutine(GetMatchRecords(SQLiteManager.Instance.player.playerId));
    }
    #endregion
    #region Player Data
    // 🔹 플레이어 데이터 요청
    public IEnumerator GetPlayerData(string idType, string idValue,bool isFirstTime,Action<bool> onComplete = null)
    {
        if(onComplete != null)
            yield return ServerToAPIManager.Instance.GetPlayer(idType, idValue,isFirstTime, onComplete);
        else
            yield return ServerToAPIManager.Instance.GetPlayer(idType, idValue, isFirstTime);
    }

    // 0403 임시
    public IEnumerator GetPlayerCurrency()
    {
        yield return ServerToAPIManager.Instance.GetCurrency();
    }

    public IEnumerator TargetReceivePlayerDataClientRpc(string jsonData)
    {
        
        var parsed = JsonConvert.DeserializeObject<PlayerAddResponse>(jsonData);
        Debug.Log($"플레이어 데이터 세이브 직전 : {parsed.playerData.playerId}");
        SQLiteManager.Instance.SavePlayerData(parsed.playerData);
        Debug.Log($"Player Data : {SQLiteManager.Instance.LoadPlayerData()}");
        yield return null;
    }

    public IEnumerator TargetReceiveAsPlayerDataClientRpc(string jsonData)
    {
        var playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData);
        Debug.Log($"[Client] Player Data Saved as {playerData}");
        SQLiteManager.Instance.SavePlayerData(playerData);
        yield return null;
    }
    // 🔹 플레이어 추가
    public IEnumerator AddPlayer()
    {
        yield return ServerToAPIManager.Instance.AddPlayer();
    }

    // 🔹 플레이어 삭제
    public IEnumerator DeletePlayer(int playerId)
    {
        yield return ServerToAPIManager.Instance.DeletePlayer(playerId);
    }

    // 🔹 플레이어 정보 업데이트
    public IEnumerator UpdatePlayerData()
    {
        yield return ServerToAPIManager.Instance.UpdatePlayerData(SQLiteManager.Instance.player);
        AddressableManager.Instance.LoadProfileIconFromGroup();
        AddressableManager.Instance.LoadMyRankingIconFromGroup();
        PopupManager.Instance.HideLoading(0.5f);
    }

    public IEnumerator UpdatePlayerNickname(string nickname)
    {
        yield return ServerToAPIManager.Instance.UpdateNicknameOnServer(nickname);
    }

    public IEnumerator UpdatePlayerGoogleId(int playerId, string googleId)
    {
        yield return ServerToAPIManager.Instance.UpdatePlayerGoogleId(playerId, googleId);
    }

    public IEnumerator AddAuthMapping(string deviceId, string googleId)
    {
        yield return ServerToAPIManager.Instance.AddAuthMapping(deviceId, googleId);
    }
    #endregion
    #region Player Items
    // 🔹 플레이어 아이템 요청
    public IEnumerator GetPlayerItems(int playerId)
    {
        yield return ServerToAPIManager.Instance.GetPlayerItems(playerId);
    }

    public void TargetReceivePlayerItemsClientRpc(string jsonData)
    {
        SQLiteManager.Instance.SavePlayerItem(JsonConvert.DeserializeObject<PlayerItemData>(jsonData));
    }

    // 플레이어 아이템 구매 요청
    public IEnumerator PurchasePlayerItem(int playerId, int itemUniqueId)
    {
        if (ServerToAPIManager.Instance != null)
            yield return ServerToAPIManager.Instance.PurchaseItem(playerId, itemUniqueId);
    }
    #endregion
    #region Player Stats
    // 🔹 플레이어 스탯 요청
    public IEnumerator GetPlayerStats(int playerId)
    {
        yield return ServerToAPIManager.Instance.GetPlayerStat(playerId);
    }

    public void TargetReceivePlayerStatsClientRpc(string jsonData)
    {
        PlayerStatsResponse playerStatsResponse = JsonConvert.DeserializeObject<PlayerStatsResponse>(jsonData);
        //Debug.Log($"{playerStatsResponse.playerStats.playerId} , Total : {playerStatsResponse.playerStats.totalGames} , Winrate : {playerStatsResponse.playerStats.winRate}");
        SQLiteManager.Instance.SavePlayerStats(playerStatsResponse.playerStats);
    }
    #endregion
    #region Player Matches
    // 매치 업데이트 to DB
    public IEnumerator AddMatchRecords(int winnerId, int loserId)
    {
        if (ServerToAPIManager.Instance != null)
        {
            yield return ServerToAPIManager.Instance.AddMatchResult(winnerId, loserId);
        }
    }

    public IEnumerator GetMatchRecords(int playerId)
    {
        if (ServerToAPIManager.Instance != null)
            yield return ServerToAPIManager.Instance.GetMatchResult(playerId);
    }

    public void TargetReceiveMatchRecordsClientRpc(MatchHistoryData matchHistoryData)
    {
        SQLiteManager.Instance.SaveMatchHistory(matchHistoryData);
    }
    #endregion
    #region Player Login
    // 로그인 정보 업데이트 to DB
    public IEnumerator UpdateLogin(int playerId)
    {
        if (ServerToAPIManager.Instance != null)
        {
            yield return ServerToAPIManager.Instance.UpdateLoginTime(playerId, "::1");
        }
    }
    // 🔹 로그인 요청
    public IEnumerator GetLogin(int playerId)
    {
        yield return ServerToAPIManager.Instance.GetLoginRecords(playerId);
    }

    public void TargetReceiveLoginDataClientRpc(string jsonData)
    {
        LoginResponse response = JsonConvert.DeserializeObject<LoginResponse>(jsonData);

        Debug.Log($"{response.records.playerId} , ip : {response.records.ipAddress} , id : {response.records.loginId}");

        SQLiteManager.Instance.SaveLoginData(response.records);
    }
    #endregion
    #region Player Ranking
    // 랭킹 정보 업데이트 to DB
    public IEnumerator GetRankingList()
    {
        Debug.Log("🔹 [Client] 랭킹 데이터 요청 시작");

        // 상위 50명 랭킹 요청
        yield return StartCoroutine(ServerToAPIManager.Instance.GetTopRankingData());

        // 개별 플레이어 랭킹 요청
        yield return StartCoroutine(ServerToAPIManager.Instance.GetMyRankingData(SQLiteManager.Instance.LoadPlayerData().playerId));
    }

    // ✅ 서버에서 받은 상위 50명 랭킹 저장
    public void TargetReceiveTopRankingDataClientRpc(string jsonData)
    {
        Debug.Log($"✅ [Client] 상위 50명 랭킹 데이터 수신: {jsonData}");

        RankingDataResponse rankingListResponse = JsonConvert.DeserializeObject<RankingDataResponse>(jsonData);
        // SQLite에 저장
        foreach (var rankingData in rankingListResponse.topRankings)
        {
            SQLiteManager.Instance.SaveRankingData(rankingData);
        }

        Debug.Log($"📌 상위 50명 랭킹 저장 완료 (총 {rankingListResponse.topRankings.Length}명)");
    }

    // ✅ 서버에서 받은 내 개별 랭킹 저장
    public void TargetReceiveMyRankingDataClientRpc(string jsonData)
    {
        Debug.Log($"✅ [Client] 개별 랭킹 데이터 수신: {jsonData}");

        MyRankingData myRankingData = JsonConvert.DeserializeObject<MyRankingData>(jsonData);

        // SQLite에 저장
        SQLiteManager.Instance.SaveMyRankingData(myRankingData.myRanking);

        Debug.Log($"📌 내 랭킹 저장 완료: {myRankingData.myRanking.playerName} (Rank: {myRankingData.myRanking.rankPosition})");
    }

    public void TargetReceivePlayerDetailsDataClientRpc(string jsonData)
    {
        Debug.Log($"✅ [Client] 상세 정보 수신: {jsonData}");

        PlayerDetailsResponse playerDetailsResponse = JsonConvert.DeserializeObject<PlayerDetailsResponse>(jsonData);
        Debug.Log(playerDetailsResponse.playerDetails.ToString());
        SQLiteManager.Instance.playerDetails = playerDetailsResponse.playerDetails;

        Debug.Log($"📌 상세 정보 저장 완료: {playerDetailsResponse.playerDetails.playerName} (Rank: {playerDetailsResponse.playerDetails.rating})");

        // ✅ 데이터 수신 완료 후 실행
        FindAnyObjectByType<PopupManager>().OnDataReceived();

    }
    public IEnumerator GetPlayerDetalis(int playerId) // 콜백 추가. 
    {
        if (ServerToAPIManager.Instance != null)
        {
            yield return ServerToAPIManager.Instance.GetPlayerDetails(playerId);
        }
    }
    #endregion
    #region Unity Auth
    public async Task SignInWithCustomId(string customId)
    {
        Debug.Log($"[Client] Custom ID로 로그인 시도: {customId}");
        await ServerToAPIManager.Instance.SignInWithCustomId(customId);
    }
    #endregion
    #region Session
    public async Task GetIsInGame(int playerId)
    {
        bool isInGame = await ServerToAPIManager.Instance.GetIsInGame(playerId);
        Debug.Log($"[Client] Get Session player is in Game? : {isInGame}");
    }
    #endregion
}
