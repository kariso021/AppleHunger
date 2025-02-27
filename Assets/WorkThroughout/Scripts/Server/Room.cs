using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;

public class Room
{
    public int RoomId { get; private set; }
    public int MaxPlayers { get; private set; } = 2;

    private Dictionary<int, NetworkConnection> players = new Dictionary<int, NetworkConnection>();

    public AppleManager AppleManager { get; private set; }
    public Timer Timer { get; private set; }

    public bool IsGameActive { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    private float gameTime = 60f;
    private float currentTime;

    public Room(int roomId, AppleManager appleManagerInstance, Timer timerInstance)
    {
        RoomId = roomId;
        AppleManager = appleManagerInstance;
        Timer = timerInstance;
    }

    public bool ContainsPlayer(int playerId)
    {
        return players.ContainsKey(playerId);
    }



    /// <summary>
    /// Room 초기화 (Timer 및 게임 설정)
    /// </summary>
    public void InitializeRoom(int roomId, List<NetworkConnection> roomPlayers)
    {
        this.RoomId = roomId;
        Debug.Log($"✅ Initializing Room {RoomId} with {roomPlayers.Count} players...");

        AppleManager.Initialize(roomId, roomPlayers);
        Timer.InitializeTimer(gameTime);
        currentTime = gameTime;
    }

    public int PlayerCount => players.Count;

    /// <summary>
    /// 플레이어를 Room에 추가
    /// </summary>
    public bool AddPlayer(int playerId, NetworkConnection conn)
    {
        if (players.Count >= MaxPlayers)
        {
            Debug.Log($"❌ Room {RoomId} is full.");
            return false;
        }

        if (!players.ContainsKey(playerId))
        {
            players.Add(playerId, conn);
            Debug.Log($"👤 Player {playerId} added to Room {RoomId}");
            return true;
        }
        else
        {
            Debug.Log($"⚠️ Player {playerId} is already in Room {RoomId}");
            return false;
        }
    }

    /// <summary>
    /// Room의 게임 시작 (Apple 생성 및 Timer 시작)
    /// </summary>
    public void StartGame()
    {
        //if (players.Count < MaxPlayers)
        //{
        //    Debug.LogError($"⚠️ Room {RoomId} does not have enough players to start.");
        //    return;
        //}

        IsGameActive = true;
        IsGameOver = false;
        currentTime = gameTime;

        Debug.Log($"🎮 Game started in Room {RoomId}");
        Timer?.StartTimer();

        // ✅ Apple을 서버에서 직접 생성하여 모든 클라이언트에 공유
        AppleManager.SpawnApplesForRoom(RoomId);
    }

    /// <summary>
    /// 게임 종료 및 Room 정리
    /// </summary>
    public void EndGame()
    {
        IsGameActive = false;
        IsGameOver = true;

        Debug.Log($"🏁 Room {RoomId} game ended.");

        // ✅ Room의 Apple 제거
        AppleManager.ClearRoomApples(RoomId);
    }
}
