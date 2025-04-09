using SQLite;
using System;
using System.Collections.Generic;

[Serializable]
[Table("rankings")]
public class PlayerRankingData
{
    [PrimaryKey]
    public int playerId { get; set; }       // 플레이어 ID

    public string playerName { get; set; } // 플레이어 이름
    public int rating { get; set; }         // 레이팅 점수
    public int rankPosition { get; set; }   // 랭킹 순위
    public string profileIcon { get; set; }

    public PlayerRankingData() { }
    public PlayerRankingData(int playerId, string playerName, int rating, int rankPosition, string profileIcon)
    {
        this.playerId = playerId;
        this.playerName = playerName;
        this.rating = rating;
        this.rankPosition = rankPosition;
        this.profileIcon = profileIcon;
    }
}
[Serializable]
public class RankingList
{
    public List<PlayerRankingData> rankings;
}

// 내 랭킹 정보를 따로 저장할 DTO
[System.Serializable]
public class MyRankingData
{
    public PlayerRankingData myRanking;
}