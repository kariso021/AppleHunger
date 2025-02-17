using FishNet.Object;
using UnityEngine;
using System.Collections.Generic;

public class AppleManager : NetworkBehaviour
{
    public GameObject applePrefab;
    public int gridWidth = 17;
    public int gridHeight = 10;
    public float spacing = 1.1f;

    private List<GameObject> spawnedApples = new List<GameObject>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnApplesInGridServerRpc();
    }

    [ServerRpc]
    public void SpawnApplesInGridServerRpc()
    {
        float xOffset = (gridWidth - 1) * spacing / 2;
        float yOffset = (gridHeight - 1) * spacing / 2;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 spawnPosition = new Vector3((x * spacing) - xOffset, -(y * spacing) + yOffset, 0);
                GameObject newApple = Instantiate(applePrefab, spawnPosition, Quaternion.identity, transform);

                NetworkObject netObj = newApple.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn(applePrefab);

                }
                else
                {
                    Debug.LogError("🚨 applePrefab에 NetworkObject가 없습니다!");
                }

                spawnedApples.Add(newApple);
            }
        }
    }

    [ServerRpc]
    public void RemoveApplesServerRpc(GameObject[] apples)
    {
        if (!IsServer) return; // 🛑 서버에서만 실행

        foreach (GameObject apple in apples)
        {
            if (apple != null)
            {
                spawnedApples.Remove(apple);
                apple.GetComponent<NetworkObject>().Despawn(); // 🟢 네트워크에서 제거
                Destroy(apple);
            }
        }
    }

}

