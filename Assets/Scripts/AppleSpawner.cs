using UnityEngine;
using System.Collections.Generic;

public class AppleSpawner : MonoBehaviour
{
    public GameObject applePrefab; // ��� ������
    public int gridWidth = 17; // ���� 17ĭ
    public int gridHeight = 10; // ���� 10ĭ
    public float spacing = 1.1f; // ��� ����

    private List<GameObject> spawnedApples = new List<GameObject>(); // ������ ��� ����

    private void Start()
    {
        SpawnApplesInGrid(); // ���� ���� �� ��� ����
    }

    private void SpawnApplesInGrid()
    {
        float xOffset = (gridWidth - 1) * spacing / 2; // X�� �߾� ����
        float yOffset = (gridHeight - 1) * spacing / 2; // Y�� �߾� ����

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // �߾� �������� ��ġ ����
                Vector3 spawnPosition = new Vector3((x * spacing) - xOffset, -(y * spacing) + yOffset, 0);
                GameObject newApple = Instantiate(applePrefab, spawnPosition, Quaternion.identity, transform);
                spawnedApples.Add(newApple);
            }
        }
    }
}