using System;
[Serializable]
public class PlayerData
{
    public int playerId;       // 플레이어 ID
    public string playerName;  // 닉네임
    public string profileIcon; // 프로필 아이콘 (경로)
    public string boardImage;  // 인게임 보드 이미지 (경로)
    public int totalGames;     // 총 판수
    public int wins;           // 승리 수
    public int losses;         // 패배 수
    public float winRate;      // 승률
    public int rating;         // 레이팅 점수
    public int currency;       // 보유 재화
    public int icons;          // 아이콘 보유 수
    public int boards;         // 보드 보유 수

    //기본 생성자 추가 (JsonUtility와 Fish-Net이 필요로 함)

    public PlayerData() { }
    public PlayerData(int id, string name, string icon, string board, int games, int wins , int losses, int rating, int currency, int icons, int boards)
    {
        this.playerId = id;
        this.playerName = name;
        this.profileIcon = icon;
        this.boardImage = board;
        this.totalGames = games;
        this.wins = wins;
        this.losses = losses;
        this.winRate = GetWinRate();
        this.rating = rating;
        this.currency = currency;
        this.icons = icons;
        this.boards = boards;
    }

    public float GetWinRate()
    {
        return totalGames > 0 ? (float)wins / totalGames * 100f : 0f;
    }
}
