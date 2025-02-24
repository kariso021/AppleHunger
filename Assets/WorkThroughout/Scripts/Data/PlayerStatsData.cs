using System;
using UnityEngine;

[Serializable]
public class PlayerStatsData
{
    public int playerId;    // �÷��̾� ID
    public int totalGames;  // �� ���� ��
    public int wins;        // �¸� ��
    public int losses;      // �й� ��
    public float winRate;   // �·� (�ڵ� ����)

    public PlayerStatsData() { }
    public PlayerStatsData(int playerId, int totalGames, int wins, int losses, float winRate)
    {
        this.playerId = playerId;
        this.totalGames = totalGames;
        this.wins = wins;
        this.losses = losses;
        this.winRate = winRate;
    }
}
