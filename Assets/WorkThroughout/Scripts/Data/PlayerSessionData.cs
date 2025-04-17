using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Table("playerSession")]
public class PlayerSessionData
{
    [PrimaryKey]
    public int playerId { get; set; }

    public string ticketId { get; set; }

    public string serverIp { get; set; }

    public int serverPort { get; set; }

    public bool isInGame { get; set; }
    public bool isConnected { get; set; }

    public string timestamp { get; set; }

    public PlayerSessionData() { }

    public PlayerSessionData(int playerId, string ticketId, string serverIp, int serverPort, bool isInGame, bool isConnected)
    {
        this.playerId = playerId;
        this.ticketId = ticketId;
        this.serverIp = serverIp;
        this.serverPort = serverPort;
        this.isInGame = isInGame;
        this.isConnected = isConnected;
    }
}
