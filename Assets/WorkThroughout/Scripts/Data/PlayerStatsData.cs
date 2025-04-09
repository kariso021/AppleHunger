using SQLite;
using System;
using UnityEngine;

[Serializable]
[Table("playerStats")]
public class PlayerStatsData
{
    [PrimaryKey]
    public int playerId { get; set; }
    public int totalGames { get; set; }
    public int wins { get; set; }
    public int losses { get; set; }
    public float winRate { get; set; } // 쿼리로 계산해서 넣는 용도로 사용

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
