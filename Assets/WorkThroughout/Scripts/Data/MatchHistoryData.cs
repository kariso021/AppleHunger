using System;

[Serializable]
public class MatchHistoryData
{
    public int matchId;       // ��ġ ID (������)
    public int player1Id;     // �÷��̾� 1 ID
    public int player2Id;     // �÷��̾� 2 ID
    public int winnerId;      // �¸��� �÷��̾� ID
    public string matchDate;  // ��ġ ��¥ (JSON ��ȯ�� ���� ���ڿ�)

    public MatchHistoryData() { } // �⺻ �����ڰ� �־�� json�� ���� ��ȯ�� ������
    public MatchHistoryData(int matchId, int player1Id, int player2Id, int winnerId, string matchDate)
    {
        this.matchId = matchId;
        this.player1Id = player1Id;
        this.player2Id = player2Id;
        this.winnerId = winnerId;
        this.matchDate = matchDate;
    }

}
