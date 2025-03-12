using Unity.Netcode;
using System;

[Serializable]
public struct MatchHistoryData : INetworkSerializable
{
    public int matchId;
    public int player1Id;
    public string player1Name;
    public int player1Rating;
    public string player1Icon;

    public int player2Id;
    public string player2Name;
    public int player2Rating;
    public string player2Icon;

    public int winnerId;
    public string matchDate;

    // 🔥 사용자 정의 생성자 추가 (모든 필드 초기화)
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

    // 🔥 Netcode 직렬화 지원 (INetworkSerializable 구현)
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref matchId);
        serializer.SerializeValue(ref player1Id);
        serializer.SerializeValue(ref player1Name);
        serializer.SerializeValue(ref player1Rating);
        serializer.SerializeValue(ref player1Icon);
        serializer.SerializeValue(ref player2Id);
        serializer.SerializeValue(ref player2Name);
        serializer.SerializeValue(ref player2Rating);
        serializer.SerializeValue(ref player2Icon);
        serializer.SerializeValue(ref winnerId);
        serializer.SerializeValue(ref matchDate);
    }
}
