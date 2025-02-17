using UnityEngine;
using System.Collections.Generic;

public class AppleSpawner : MonoBehaviour
{
    public GameObject applePrefab; // 사과 프리팹
    public int gridWidth = 17; // 가로 17칸
    public int gridHeight = 10; // 세로 10칸
    public float spacing = 1.1f; // 사과 간격

    private List<GameObject> spawnedApples = new List<GameObject>(); // 생성된 사과 저장

    private void Start()
    {
        SpawnApplesInGrid(); // 게임 시작 시 사과 생성
    }

    private void SpawnApplesInGrid()
    {
        float xOffset = (gridWidth - 1) * spacing / 2; // X축 중앙 정렬
        float yOffset = (gridHeight - 1) * spacing / 2; // Y축 중앙 정렬

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // 중앙 기준으로 위치 조정
                Vector3 spawnPosition = new Vector3((x * spacing) - xOffset, -(y * spacing) + yOffset, 0);
                GameObject newApple = Instantiate(applePrefab, spawnPosition, Quaternion.identity, transform);
                spawnedApples.Add(newApple);
            }
        }
    }
}