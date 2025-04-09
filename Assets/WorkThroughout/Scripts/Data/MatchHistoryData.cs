using System;
using SQLite;
[Serializable]
[Table("matchRecords")]
public class MatchHistoryData
{
    [PrimaryKey]
    public int matchId { get; set; }
    public int player1Id { get; set; }
    public string player1Name { get; set; }
    public int player1Rating { get; set; }
    public string player1Icon { get; set; }
    public int player2Id { get; set; }
    public string player2Name { get; set; }
    public int player2Rating { get; set; }
    public string player2Icon { get; set; }
    public int winnerId { get; set; }
    public string matchDate { get; set; }

    public MatchHistoryData() { }

    // 커스텀 생성자
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
