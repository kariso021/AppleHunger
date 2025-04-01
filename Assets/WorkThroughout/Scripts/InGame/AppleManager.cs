using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.BossRoom.Infrastructure;

public class AppleManager : NetworkBehaviour
{
    public GameObject applePrefab;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float spacing = 1.1f;

    private List<Apple> spawnedApples = new List<Apple>(); // Apple 컴포넌트 직접 저장

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            Debug.Log("🟢 Server Started: Spawning Apples...");
            SpawnApplesInGrid(); // 서버에서만 Apple 스폰
        }
        else
        {
            Debug.Log("🔵 Client Joined: Requesting Apple Data...");
            RequestAppleDataServerRpc(NetworkManager.LocalClientId);
        }
    }

    private void SpawnApplesInGrid()
    {
        float xOffset = (gridWidth - 1) * spacing / 2;
        float yOffset = (gridHeight - 1) * spacing / 2;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 spawnPosition = new Vector3((x * spacing) - xOffset, -(y * spacing) + yOffset, 0);

                // 오브젝트 풀에서 Apple 가져오기
                NetworkObject pooledObject = NetworkObjectPool.Singleton.GetNetworkObject(applePrefab, spawnPosition, Quaternion.identity);

                if (pooledObject != null)
                {
                    pooledObject.transform.position = spawnPosition;
                    pooledObject.gameObject.SetActive(true);
                    pooledObject.Spawn(true); // 서버에서 스폰

                    if (pooledObject.TryGetComponent(out Apple apple))
                    {
                        spawnedApples.Add(apple); // Apple 리스트에 추가
                        apple.SetValue(Random.Range(1, 9)); // 랜덤 값 할당
                    }
                }
                else
                {
                    Debug.LogError("❌ AppleManager: NetworkObjectPool에서 Apple을 가져오지 못했습니다.");
                }
            }
        }
    }

    // 클라이언트가 접속할 때 기존 Apple의 Value 값 요청
    [ServerRpc(RequireOwnership = false)]
    private void RequestAppleDataServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        Debug.Log($"🟡 Server: Sending Apple Data to Client {clientId}");
        SendAppleDataToClientRpc(clientId, GetAppleValues());
    }

    [ClientRpc]
    private void SendAppleDataToClientRpc(ulong clientId, int[] appleValues)
    {
        if (!IsClient) return;

        Debug.Log($"🔵 Client: Syncing {appleValues.Length} Apple Values from Server.");

        for (int i = 0; i < spawnedApples.Count; i++)
        {
            if (i < appleValues.Length)
            {
                spawnedApples[i].SetValue(appleValues[i]); // 클라이언트 Apple 값 동기화
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

    // Apple을 제거할 때 오브젝트 풀에 반환
    public void DespawnApple(Apple apple)
    {
        if (!IsServer) return;

        if (spawnedApples.Contains(apple))
        {
            spawnedApples.Remove(apple);
            apple.GetComponent<NetworkObject>().Despawn();
            NetworkObjectPool.Singleton.ReturnNetworkObject(apple.GetComponent<NetworkObject>(), applePrefab);

            // 모든 사과 제거 후 새로운 조합이 가능한지 검사
            if (!CanAnyAppleBeRemoved())
            {
                Debug.Log("🔁 가능한 조합이 없음 → 사과 리셋");
                ResetAppleGrid();
            }
        }
    }

    private void ResetAppleGrid()
    {
        // 1. 기존 사과 전부 제거
        foreach (var apple in spawnedApples)
        {
            if (apple != null)
            {
                apple.GetComponent<NetworkObject>().Despawn();
                NetworkObjectPool.Singleton.ReturnNetworkObject(apple.GetComponent<NetworkObject>(), applePrefab);
            }
        }
        spawnedApples.Clear();

        // 2. 새로 사과 생성
        SpawnApplesInGrid();
    }

    private bool CheckSubRectWithSum10(int[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        int[,] prefixSum = new int[rows, cols];

        // PrefixSum 계산
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                prefixSum[r, c] = grid[r, c];
                if (r > 0) prefixSum[r, c] += prefixSum[r - 1, c];
                if (c > 0) prefixSum[r, c] += prefixSum[r, c - 1];
                if (r > 0 && c > 0) prefixSum[r, c] -= prefixSum[r - 1, c - 1];
            }
        }

        // 모든 사각형 구간 검사
        for (int r1 = 0; r1 < rows; r1++)
        {
            for (int c1 = 0; c1 < cols; c1++)
            {
                for (int r2 = r1; r2 < rows; r2++)
                {
                    for (int c2 = c1; c2 < cols; c2++)
                    {
                        int sum = prefixSum[r2, c2];
                        if (r1 > 0) sum -= prefixSum[r1 - 1, c2];
                        if (c1 > 0) sum -= prefixSum[r2, c1 - 1];
                        if (r1 > 0 && c1 > 0) sum += prefixSum[r1 - 1, c1 - 1];

                        if (sum == 10)
                            return true;
                    }
                }
            }
        }

        return false;
    }

    private bool CanAnyAppleBeRemoved()
    {
        int[,] grid = new int[gridHeight, gridWidth];

        foreach (var apple in spawnedApples)
        {
            Vector3 pos = apple.transform.position;

            int x = Mathf.RoundToInt((pos.x + (gridWidth - 1) * spacing / 2) / spacing);
            int y = Mathf.RoundToInt(((gridHeight - 1) * spacing / 2 - pos.y) / spacing);

            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                grid[y, x] = apple.Value;
        }

        return CheckSubRectWithSum10(grid);
    }
}
