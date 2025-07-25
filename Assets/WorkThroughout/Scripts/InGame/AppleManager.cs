﻿using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class AppleManager : NetworkBehaviour
{
    public static AppleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [Header("Grid Settings")]
    public GameObject applePrefab;
    public int gridWidth = 5;
    public int gridHeight = 6;
    public float spacing = 1.1f;

    private Apple[,] appleGrid;

    private int[,] appleValues; // Grid의 value 값만 저장

    [Header("Effect Settings")]
    [SerializeField] private GameObject effectPrefab;    // 파티클 프리팹

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            appleValues = new int[gridHeight, gridWidth];
            appleGrid = new Apple[gridHeight, gridWidth];
         
        }
    }

    public void SpawnApplesInGrid()
    {
        float xOffset = (gridWidth - 1) * spacing / 2f;
        float yOffset = (gridHeight - 1) * spacing / 2f;


        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 pos = new Vector3((x * spacing) - xOffset, -(y * spacing) + yOffset, 0f);

                // 1) Apple 스폰 (기존)
                GameObject appleObj = Instantiate(applePrefab, pos, Quaternion.identity);
                appleObj.GetComponent<NetworkObject>().Spawn(true);
                var apple = appleObj.GetComponent<Apple>();
                apple.SetGridPosition(y, x);
                appleGrid[y, x] = apple;
                appleValues[y, x] = apple.Value;
            }
        }
    }

    public void DespawnApple(Apple apple)
    {
        if (!IsServer) return;

        int x = apple.GridX;
        int y = apple.GridY;

        if (appleGrid[y, x] == apple)
        {
            Vector3 worldPos = apple.transform.position;
            apple.GetComponent<NetworkObject>().Despawn();
            appleGrid[y, x] = null;
            appleValues[y, x] = 0;

            Debug.Log($"🍎 Apple despawned at ({x}, {y})");

            //사과프리펩 제거 후 이펙트 생성
            PlayRemoveEffectClientRpc(worldPos);
        }
        else
        {
            Debug.LogWarning($"❗ Apple 위치 불일치: appleGrid[{y},{x}] != apple");
        }
    }

    private void ResetAppleGrid()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Apple apple = appleGrid[y, x];
                if (apple != null)
                {
                    apple.GetComponent<NetworkObject>().Despawn();
                    appleGrid[y, x] = null;
                    appleValues[y, x] = 0;
                }
            }
        }

        SpawnApplesInGrid();
    }

    //Reset될때 NotifyPanel 띄우고 게임시간 정지하는 부분
    private void WhenResetAndDoNotify(float seconds)
    {
        if(!IsServer) return;
        GameTimer.Instance.PauseTimer(seconds);
        NotifyResetPanelClientRpc(seconds); // NotifyPanel 띄우기

        // 콤보 시간들 일시정지
        ScoreManager.Instance.WhenResetExtendComboDuration(seconds);

        var pc = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        pc.RestrictDragForWhile(seconds);


    }

    [ClientRpc]
    public void NotifyResetPanelClientRpc(float seconds)
    { 
        PlayerUI.Instance.ToggleNotifyResetPanel(seconds); // NotifyPanel 띄우기    
    }


    public void CanAnyAppleBeRemoved()
    {
        //10의 결과가 반환되지 않을시
        if (!CheckSubRectWithSum10(appleValues))
        {
            WhenResetAndDoNotify(2f);
            ResetAppleGrid();
        }
    }

    private bool CheckSubRectWithSum10(int[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        int[,] prefixSum = new int[rows, cols];

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

        for (int r1 = 0; r1 < rows; r1++)
        {
            for (int c1 = 0; c1 < cols; c1++)
            {
                for (int r2 = r1; r2 < rows; r2++)
                {
                    for (int c2 = c1; c2 < cols; c2++)
                    {
                        int height = r2 - r1 + 1;
                        int width = c2 - c1 + 1;
                        if (height == 1 && width == 1) continue;

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

    [ClientRpc]
    private void PlayRemoveEffectClientRpc(Vector3 worldPos)
    {
        if (effectPrefab == null) return;
        // 받은 worldPos 위치에 이펙트 생성
        var go = Instantiate(effectPrefab, worldPos, Quaternion.identity);
        var ps = go.GetComponent<ParticleSystem>();
        ps.Play();
        // duration + maxLifetime 만큼 뒤에 제거
        float life = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(go, life);
    }

}
