using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;

public class RoomManager : NetworkBehaviour
{

    private Dictionary<int, Room> rooms = new Dictionary<int, Room>();
    private List<NetworkConnection> waitingPlayers = new List<NetworkConnection>();
    private int nextRoomId = 1;

    // ✅ AppleManager와 Timer를 인스펙터에서 직접 할당
    public AppleManager appleManager;
    public Timer timer; 

    private void Awake()
    {
       

        // ✅ 인스펙터에서 직접 할당된 AppleManager와 Timer가 있는지 체크
        if (appleManager == null || timer == null)
        {
            Debug.LogError("❌ AppleManager 또는 Timer가 RoomManager에 연결되지 않았습니다! 인스펙터에서 설정해주세요.");
        }
    }

    /// <summary>
    /// 플레이어를 Room에 배정하는 함수
    /// </summary>
    [Server]
    public void AssignPlayerToRoom(NetworkConnection sender)
    {
        // ✅ 중복 체크: 이미 추가된 플레이어인지 확인
        foreach (var room in rooms.Values)
        {
            if (room.ContainsPlayer(sender.ClientId))
            {
                Debug.LogWarning($"⚠️ Player {sender.ClientId} is already assigned to a room.");
                return;
            }
        }

        Debug.Log($"👤 Assigning Player {sender.ClientId} to a room...");

        waitingPlayers.Add(sender);

        if (waitingPlayers.Count >= 1) // ✅ 2명 필요
        {
            CreateRoomWithPlayers();
        }
        else
        {
            Debug.Log($"⏳ Player {sender.ClientId} is waiting for an opponent...");
        }
    }


    /// <summary>
    /// 2명의 플레이어가 필요하여 Room을 생성하고 입장
    /// </summary>
    [Server]
    private void CreateRoomWithPlayers()
    {
        if (waitingPlayers.Count < 1)
        {
            Debug.LogWarning("⚠️ 대기 중인 플레이어가 부족하여 Room을 생성할 수 없습니다. 현재 대기 중인 플레이어 수: " + waitingPlayers.Count);
            return;
        }

        List<NetworkConnection> roomConnections = new List<NetworkConnection>
    {
        waitingPlayers[0]
        //waitingPlayers[1]
    };

        // ✅ 대기열에서 2명 제거
        waitingPlayers.RemoveAt(0);

        int roomId = nextRoomId++;

        // ✅ 새로운 Room을 RoomManager에서 생성된 AppleManager 및 Timer와 함께 초기화
        Room newRoom = new Room(roomId, appleManager, timer);
        rooms.Add(roomId, newRoom);

        Debug.Log($"🆕 Room {roomId} created for Players {roomConnections[0].ClientId} ");

        foreach (var player in roomConnections)
        {
            newRoom.AddPlayer(player.ClientId, player);
        }

        // ✅ Room 초기화 및 AppleManager 활성화
        newRoom.InitializeRoom(roomId, roomConnections);

        // ✅ Room 시작
        newRoom.StartGame();
    }

}
