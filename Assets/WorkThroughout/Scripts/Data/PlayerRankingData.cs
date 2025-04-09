using SQLite;
using System;
using System.Collections.Generic;

[Serializable]
[Table("rankings")]
public class PlayerRankingData
{
    [PrimaryKey]
    public int playerId { get; set; }       // �÷��̾� ID

    public string playerName { get; set; } // �÷��̾� �̸�
    public int rating { get; set; }         // ������ ����
    public int rankPosition { get; set; }   // ��ŷ ����
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

// �� ��ŷ ������ ���� ������ DTO
[System.Serializable]
public class MyRankingData
{
    public PlayerRankingData myRanking;
}