using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet;
using FishNet.Observing;

public class AppleManager : NetworkBehaviour
{
    public GameObject applePrefab;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float spacing = 1.1f;

    private List<Apple> spawnedApples = new List<Apple>(); // Apple 컴포넌트를 직접 저장

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnApplesInGrid(); // 서버에서 Apple 스폰
    }

    private void SpawnApplesInGrid()
    {
        Debug.Log("🚀 Server: Spawning Apples...");
        float xOffset = (gridWidth - 1) * spacing / 2;
        float yOffset = (gridHeight - 1) * spacing / 2;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 spawnPosition = new Vector3((x * spacing) - xOffset, -(y * spacing) + yOffset, 0);
                GameObject newApple = Instantiate(applePrefab, spawnPosition, Quaternion.identity);

                if (newApple.TryGetComponent(out Apple apple))
                {
                    InstanceFinder.ServerManager.Spawn(newApple); // ✅ 서버에서 스폰
                    spawnedApples.Add(apple); // ✅ Apple 컴포넌트 저장
                }
            }
        }
    }

    // 🔹 클라이언트가 접속할 때 기존 Apple의 Value 값 요청
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsClient)
        {
            RequestAppleDataServerRpc(); // ✅ 서버에 기존 Apple 데이터 요청
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAppleDataServerRpc(NetworkConnection conn = null)
    {
        if (IsServer)
        {
            SendAppleDataToClientRpc(conn, GetAppleValues()); // ✅ 기존 Apple들의 Value 값 전송
        }
    }

    [TargetRpc] // 특정 클라이언트에게만 Value 값 전송
    private void SendAppleDataToClientRpc(NetworkConnection conn, int[] appleValues)
    {
        Debug.Log($"🍏 Client: Syncing {appleValues.Length} Apple Values from Server.");

        // 클라이언트 측에서 Apple의 Value 값 업데이트
        for (int i = 0; i < spawnedApples.Count; i++)
        {
            if (i < appleValues.Length)
            {
                spawnedApples[i].SetValue(appleValues[i]); // 클라이언트에서 Apple 값 업데이트
            }
        }
    }

    // Apple들의 Value 값 배열을 가져오는 함수
    private int[] GetAppleValues()
    {
        int[] values = new int[spawnedApples.Count];
        for (int i = 0; i < spawnedApples.Count; i++)
        {
            values[i] = spawnedApples[i].Value;
        }
        return values;
    }
}
