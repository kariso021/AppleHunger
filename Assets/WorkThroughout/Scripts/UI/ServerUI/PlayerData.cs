using System;
[Serializable]
public class PlayerData
{
    public int playerId;       // �÷��̾� ID
    public string playerName;  // �г���
    public string profileIcon; // ������ ������ (���)
    public string boardImage;  // �ΰ��� ���� �̹��� (���)
    public int totalGames;     // �� �Ǽ�
    public int wins;           // �¸� ��
    public int losses;         // �й� ��
    public float winRate;      // �·�
    public int rating;         // ������ ����
    public int currency;       // ���� ��ȭ
    public int icons;          // ������ ���� ��
    public int boards;         // ���� ���� ��

    //�⺻ ������ �߰� (JsonUtility�� Fish-Net�� �ʿ�� ��)

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
