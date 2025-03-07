using System;
[Serializable]

public class PlayerData
{
    public int playerId;       // 플레이어 ID, Auto-Increment 계열이라 생성자에서 따로 지정해줄 필요 없음
    public string deviceId;    // 게스트 로그인용 기기 ID
    public string googleId;    // 구글 로그인용 ID
    public string playerName;  // 닉네임
    public string profileIcon; // 프로필 아이콘 (경로)
    public string boardImage;  // 인게임 보드 이미지 (경로)
    public int rating;         // 레이팅 점수
    public int currency;       // 보유 재화
    public string createdAt;   // 유저 회원가입 시점

    //기본 생성자 추가 (JsonUtility와 Fish-Net이 필요로 함)

    public PlayerData() { }
    public PlayerData(string deviceId,string googleId,string name, string icon, string board, int rating, int currency)
    {
        this.deviceId = deviceId;
        this.googleId = googleId;   
        this.playerName = name;
        this.profileIcon = icon;
        this.boardImage = board;
        this.rating = rating;
        this.currency = currency;
    }
    public PlayerData(int playerId,string deviceId, string googleId, string name, string icon, string board, int rating, int currency, string createdAt)
    {
        this.playerId = playerId;
        this.deviceId = deviceId;
        this.googleId = googleId;
        this.playerName = name;
        this.profileIcon = icon;
        this.boardImage = board;
        this.rating = rating;
        this.currency = currency;
        this.createdAt = createdAt;
    }
    public override string ToString()
    {
        return $"PlayerData: {{ " +
               $"playerId: {playerId}, " +
               $"deviceId: {deviceId}, " +
               $"googleId: {googleId}, " +
               $"playerName: {playerName}, " +
               $"profileIcon: {profileIcon}, " +
               $"boardImage: {boardImage}, " +
               $"rating: {rating}, " +
               $"currency: {currency}, " +
               $"createdAt: {createdAt} }}";
    }

}
