using UnityEngine;

public class AppleManagerSingle : MonoBehaviour
{
    public static AppleManagerSingle Instance { get; private set; }

    [Header("Grid Settings")]
    public GameObject applePrefab;
    public int gridWidth = 5;
    public int gridHeight = 6;
    public float spacing = 1.1f;

    private AppleSingle[,] appleGrid;
    private int[,] appleValues;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // �׸��� �ʱ�ȭ
        appleValues = new int[gridHeight, gridWidth];
        appleGrid = new AppleSingle[gridHeight, gridWidth];
        SpawnApplesInGrid();
    }

    private void SpawnApplesInGrid()
    {
        float xOffset = (gridWidth - 1) * spacing / 2f;
        float yOffset = (gridHeight - 1) * spacing / 2f;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 pos = new Vector3(
                    x * spacing - xOffset,
                    -y * spacing + yOffset,
                    0f
                );

                GameObject obj = Instantiate(applePrefab, pos, Quaternion.identity);
                AppleSingle apple = obj.GetComponent<AppleSingle>();
                if (apple == null) continue;

                appleValues[y, x] = apple.Value;
                appleGrid[y, x] = apple;
                apple.SetGridPosition(y, x);
            }
        }
    }

    /// <summary>
    /// ��� ����
    /// </summary>
    public void RemoveApple(AppleSingle apple)
    {
        int x = apple.GridX;
        int y = apple.GridY;

        if (appleGrid[y, x] != apple)
        {
            Debug.LogWarning($"��ġ ����ġ: appleGrid[{y},{x}] != ��� ���");
            return;
        }

        Destroy(apple.gameObject);
        appleGrid[y, x] = null;
        appleValues[y, x] = 0;

        Debug.Log($"?? Apple removed at ({x}, {y})");

        if (!HasCombinationLeft())
        {
            Debug.Log("���� �Ұ� �� �׸��� ����");
            ResetGrid();
        }
    }

    private void ResetGrid()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (appleGrid[y, x] != null)
                {
                    Destroy(appleGrid[y, x].gameObject);
                    appleGrid[y, x] = null;
                    appleValues[y, x] = 0;
                }
            }
        }
        SpawnApplesInGrid();
    }

    /// <summary>
    /// 10 �հ� ������ �����ִ��� �˻�
    /// </summary>
    private bool HasCombinationLeft()
    {
        // ����� �α�
        Debug.Log("Apple Grid ���� (Top��Bottom):");
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            string line = "";
            for (int x = 0; x < gridWidth; x++)
                line += appleValues[y, x] + " ";
            Debug.Log(line);
        }
        return CheckSum10(appleValues);
    }

    private bool CheckSum10(int[,] grid)
    {
        int rows = grid.GetLength(0), cols = grid.GetLength(1);
        int[,] ps = new int[rows, cols];

        // prefix-sum
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                ps[r, c] = grid[r, c]
                    + (r > 0 ? ps[r - 1, c] : 0)
                    + (c > 0 ? ps[r, c - 1] : 0)
                    - (r > 0 && c > 0 ? ps[r - 1, c - 1] : 0);
            }
        }

        // �κ� ���簢�� �� �˻�
        for (int r1 = 0; r1 < rows; r1++)
            for (int c1 = 0; c1 < cols; c1++)
                for (int r2 = r1; r2 < rows; r2++)
                    for (int c2 = c1; c2 < cols; c2++)
                    {
                        if (r1 == r2 && c1 == c2) continue; // ���� ĭ ����

                        int sum = ps[r2, c2]
                            - (r1 > 0 ? ps[r1 - 1, c2] : 0)
                            - (c1 > 0 ? ps[r2, c1 - 1] : 0)
                            + (r1 > 0 && c1 > 0 ? ps[r1 - 1, c1 - 1] : 0);

                        if (sum == 10) return true;
                    }
        return false;
    }
}
