using SQLite;
using System;
[Serializable]
[Table("players")]
public class PlayerData
{
    [PrimaryKey]
    public int playerId { get; set; }
    public string deviceId { get; set; }
    public string googleId { get; set; }
    public string playerName { get; set; }
    public string profileIcon { get; set; }
    public string boardImage { get; set; }
    public int rating { get; set; }
    public int currency { get; set; }
    public string createdAt { get; set; }

    //기본 생성자 추가 (
    //
    //와 Fish-Net이 필요로 함)

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
