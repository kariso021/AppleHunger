using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using System.Collections.Generic;
using FishNet;

public class AppleManager : NetworkBehaviour
{
    public GameObject applePrefab;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float spacing = 1.1f;

    private List<GameObject> spawnedApples = new List<GameObject>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnApplesInGrid(); // ✅ 직접 호출하여 서버에서만 실행
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
                GameObject newApple = Instantiate(applePrefab, spawnPosition, Quaternion.identity, transform);

                if (newApple.TryGetComponent(out NetworkObject netObj))
                {
                    InstanceFinder.ServerManager.Spawn(newApple); // ✅ 공식 문서 적용
                    Debug.Log($"✅ Server: Apple spawned at {spawnPosition}");
                }
                else
                {
                    Debug.LogError("🚨 applePrefab에 NetworkObject가 없습니다!");
                }

                spawnedApples.Add(newApple);
            }
        }
    }


    [ObserversRpc] // 모든 클라이언트에게 Apple이 정상적으로 생성되었는지 확인
    private void NotifyClientsAppleSpawned()
    {
        Debug.Log("🍏 Client: Apple has been received and spawned.");
    }

    [ServerRpc]
    public void RemoveApplesServerRpc()
    {
        if (!IsServer) return; // 🛑 서버에서만 실행

        foreach (GameObject apple in spawnedApples)
        {
            if (apple != null)
            {
                if (apple.TryGetComponent(out NetworkObject netObj))
                {
                    InstanceFinder.ServerManager.Despawn(apple); // ✅ 공식 문서 적용 (기존의 netObj.Despawn() 제거)
                }
                Destroy(apple);
            }
        }
        spawnedApples.Clear();
    }
}
