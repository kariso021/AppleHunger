using System;
using System.Collections.Generic;
using UnityEngine.Experimental.GlobalIllumination;

[Serializable]
public class PlayerRankingData
{
    public int playerId;       // �÷��̾� ID
    public string playerName;  // �÷��̾� �̸�
    public int rating;         // ������ ����
    public int rankPosition;   // ��ŷ ����
    public string profileIcon;

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