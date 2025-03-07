using System;
[Serializable]

public class PlayerData
{
    public int playerId;       // �÷��̾� ID, Auto-Increment �迭�̶� �����ڿ��� ���� �������� �ʿ� ����
    public string deviceId;    // �Խ�Ʈ �α��ο� ��� ID
    public string googleId;    // ���� �α��ο� ID
    public string playerName;  // �г���
    public string profileIcon; // ������ ������ (���)
    public string boardImage;  // �ΰ��� ���� �̹��� (���)
    public int rating;         // ������ ����
    public int currency;       // ���� ��ȭ
    public string createdAt;   // ���� ȸ������ ����

    //�⺻ ������ �߰� (JsonUtility�� Fish-Net�� �ʿ�� ��)

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
