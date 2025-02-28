using System;

[Serializable]
public class MatchHistoryData
{
    public int matchId;         // ��ġ ID (������)
    public int player1Id;       // �÷��̾� 1 ID
    public string player1Name;  // �÷��̾� 1 �̸�
    public int player1Rating;   // �÷��̾� 1 ������
    public string player1Icon;  // �÷��̾� 1 ������ ������

    public int player2Id;       // �÷��̾� 2 ID
    public string player2Name;  // �÷��̾� 2 �̸�
    public int player2Rating;   // �÷��̾� 2 ������
    public string player2Icon;  // �÷��̾� 2 ������ ������

    public int winnerId;        // �¸��� �÷��̾� ID
    public string matchDate;    // ��ġ ��¥ (JSON ��ȯ�� ���� ���ڿ�)

    public MatchHistoryData() { } // �⺻ �����ڰ� �־�� json�� ���� ��ȯ�� ������

    public MatchHistoryData(int matchId, int player1Id, string player1Name, int player1Rating, string player1Icon,
                            int player2Id, string player2Name, int player2Rating, string player2Icon,
                            int winnerId, string matchDate)
    {
        this.matchId = matchId;

        this.player1Id = player1Id;
        this.player1Name = player1Name;
        this.player1Rating = player1Rating;
        this.player1Icon = player1Icon;

        this.player2Id = player2Id;
        this.player2Name = player2Name;
        this.player2Rating = player2Rating;
        this.player2Icon = player2Icon;

        this.winnerId = winnerId;
        this.matchDate = matchDate;
    }
}
