using System;

[Serializable]
public class MatchHistoryData
{
    public int matchId;         // 매치 ID (고유값)
    public int player1Id;       // 플레이어 1 ID
    public string player1Name;  // 플레이어 1 이름
    public int player1Rating;   // 플레이어 1 레이팅
    public string player1Icon;  // 플레이어 1 프로필 아이콘

    public int player2Id;       // 플레이어 2 ID
    public string player2Name;  // 플레이어 2 이름
    public int player2Rating;   // 플레이어 2 레이팅
    public string player2Icon;  // 플레이어 2 프로필 아이콘

    public int winnerId;        // 승리한 플레이어 ID
    public string matchDate;    // 매치 날짜 (JSON 변환을 위해 문자열)

    public MatchHistoryData() { } // 기본 생성자가 있어야 json을 통한 변환이 가능함

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
