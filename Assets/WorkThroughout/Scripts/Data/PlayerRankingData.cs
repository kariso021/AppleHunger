using System;
using System.Collections.Generic;

[Serializable]
public class PlayerRankingData
{
    public int playerId;       // �÷��̾� ID
    public string playerName;  // �÷��̾� �̸�
    public int rating;         // ������ ����
    public int rankPosition;   // ��ŷ ����

    public PlayerRankingData(int playerId, string playerName, int rating, int rankPosition)
    {
        this.playerId = playerId;
        this.playerName = playerName;
        this.rating = rating;
        this.rankPosition = rankPosition;
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