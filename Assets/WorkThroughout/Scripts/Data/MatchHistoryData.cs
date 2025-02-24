using System;

[Serializable]
public class MatchHistoryData
{
    public int matchId;       // 매치 ID (고유값)
    public int player1Id;     // 플레이어 1 ID
    public int player2Id;     // 플레이어 2 ID
    public int winnerId;      // 승리한 플레이어 ID
    public string matchDate;  // 매치 날짜 (JSON 변환을 위해 문자열)

    public MatchHistoryData() { } // 기본 생성자가 있어야 json을 통한 변환이 가능함
    public MatchHistoryData(int matchId, int player1Id, int player2Id, int winnerId, string matchDate)
    {
        this.matchId = matchId;
        this.player1Id = player1Id;
        this.player2Id = player2Id;
        this.winnerId = winnerId;
        this.matchDate = matchDate;
    }

}
