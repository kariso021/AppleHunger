using System;
using UnityEngine;

[Serializable]
public class PlayerStatsData
{
    public int playerId;    // ÇÃ·¹ÀÌ¾î ID
    public int totalGames;  // ÃÑ °ÔÀÓ ¼ö
    public int wins;        // ½Â¸® ¼ö
    public int losses;      // ÆÐ¹è ¼ö
    public float winRate;   // ½Â·ü (ÀÚµ¿ °è»êµÊ)

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
