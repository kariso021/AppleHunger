using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet;

public class AppleManager : NetworkBehaviour
{
    public GameObject applePrefab;

    // ✅ Room별 Apple 정보를 저장
    private Dictionary<int, List<Apple>> roomApples = new Dictionary<int, List<Apple>>();
    private Dictionary<int, List<NetworkConnection>> roomPlayers = new Dictionary<int, List<NetworkConnection>>();


    public int rows = 5; // ✅ Apple 배치 행 개수
    public int cols = 5; // ✅ Apple 배치 열 개수
    public float spacing = 1.1f; // ✅ Apple 간격

    /// <summary>
    /// AppleManager 초기화 - Room ID 설정 및 Room에 속한 플레이어 목록 저장
    /// </summary>
    public void Initialize(int roomId, List<NetworkConnection> players)
    {
        if (!roomPlayers.ContainsKey(roomId))
        {
            roomPlayers[roomId] = new List<NetworkConnection>();
        }
        roomPlayers[roomId].AddRange(players);
        Debug.Log($"🍏 AppleManager initialized for Room {roomId} with {players.Count} players.");
    }

    /// <summary>
    /// 특정 Room에서 Apple을 생성하고, Room 내 모든 플레이어가 공유할 수 있도록 설정
    /// </summary>
    [Server]
    public void SpawnApplesForRoom(int roomId)
    {
        if (!roomPlayers.ContainsKey(roomId) || roomPlayers[roomId].Count == 0)
        {
            Debug.Log($"🍏 Generating Apples for Room {roomId}");
            return;
        }

   

        List<Apple> spawnedApples = new List<Apple>();


        float xOffset = (cols - 1) * spacing / 2; // 중앙 정렬
        float yOffset = (rows - 1) * spacing / 2; // 중앙 정렬

        for (int row = 0; row < rows; row++) // 행 루프
        {
            for (int col = 0; col < cols; col++) // 열 루프
            {
                Vector3 position = new Vector3(
                    (col * spacing) - xOffset,
                    (row * spacing) - yOffset,
                    0
                );

                int value = Random.Range(1, 10);

                GameObject appleObj = Instantiate(applePrefab, position, Quaternion.identity);

                if (appleObj.TryGetComponent(out Apple apple))
                {
                    apple.Initialize(roomId, value);
                    spawnedApples.Add(apple);

                    // ✅ 모든 플레이어가 동일한 Apple을 공유하도록 Spawn 실행
                    foreach (NetworkConnection player in roomPlayers[roomId])
                    {
                        InstanceFinder.ServerManager.Spawn(appleObj, player);
                    }
                }
            }
        }

        // ✅ 생성된 Apple을 Room 별로 저장
        if (!roomApples.ContainsKey(roomId))
        {
            roomApples[roomId] = new List<Apple>();
        }
        roomApples[roomId].AddRange(spawnedApples);
    }


    /// <summary>
    /// 특정 Room의 모든 Apple 제거
    /// </summary>
    [Server]
    public void ClearRoomApples(int roomId)
    {
        Debug.Log($"🗑️ Clearing Apples for Room {roomId}");

        if (roomApples.ContainsKey(roomId))
        {
            foreach (Apple apple in roomApples[roomId])
            {
                if (apple != null)
                {
                    InstanceFinder.ServerManager.Despawn(apple.gameObject); // ✅ 네트워크에서 Apple 제거
                    Destroy(apple.gameObject);
                }
            }

            roomApples.Remove(roomId);
        }
    }

    /// <summary>
    /// 특정 Apple이 사라질 때 호출 (예: 플레이어가 Apple을 먹었을 때)
    /// </summary>
    [Server]
    public void RemoveApple(int roomId, Apple apple)
    {
        if (roomApples.ContainsKey(roomId) && roomApples[roomId].Contains(apple))
        {
            roomApples[roomId].Remove(apple);
            InstanceFinder.ServerManager.Despawn(apple.gameObject); // ✅ 네트워크에서 Apple 제거
            Destroy(apple.gameObject);
        }
    }
}
