using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

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


    // 20250505 추가
    [Header("Apple Detect Size")]
    private float appleDetectSize = 0.225f;

    [Header("Temp Buttons")]
    public Button add;
    public Button sub;
    public TMP_Text curText;


    //BackGroudnRender
    [Header("Background Settings")]
    public SpriteRenderer backgroundRenderer;

    [Header("Base Settings for 4×4")]
    [Tooltip("4×4 보드일 때 적용할 배경 스케일")]
    public float baseScale = 0.6f;
    [Tooltip("4×4 보드일 때 실제 그리드 폭 = (4-1)*spacing+1")]
    public Vector2 baseGrid = new Vector2(4f, 4f);


    //
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
        // 그리드 초기화
        appleValues = new int[gridHeight, gridWidth];
        appleGrid = new AppleSingle[gridHeight, gridWidth];
        SpawnApplesInGrid();

        // temp
        curText.text = appleDetectSize.ToString();
    }

    private void ResizeBackground()
    {
        float width = (gridWidth - 1) * spacing + 1f;
        float height = (gridHeight - 1) * spacing + 1f;

  
        float sx = width / baseGrid.x;
        float sy = height / baseGrid.y;

        float finalScale = baseScale * Mathf.Max(sx, sy);


        backgroundRenderer.transform.localScale = new Vector3(finalScale, finalScale, 1f);

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
                apple.detectSize = appleDetectSize; // 사과 탐지 범위 자율 설정, 20250505
                if (apple == null) continue;

                appleValues[y, x] = apple.Value;
                appleGrid[y, x] = apple;
                apple.SetGridPosition(y, x);
            }
        }
    }

    /// <summary>
    /// 사과 제거
    /// </summary>
    public void RemoveApple(AppleSingle apple)
    {
        int x = apple.GridX;
        int y = apple.GridY;

        if (appleGrid[y, x] != apple)
        {
            Debug.LogWarning($"위치 불일치: appleGrid[{y},{x}] != 대상 사과");
            return;
        }

        // 파티클 분리 및 재생
        ParticleSystem ps = apple.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.transform.SetParent(null); // 부모에서 분리
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration);
        }

        // 사과 제거
        Destroy(apple.gameObject);
        appleGrid[y, x] = null;
        appleValues[y, x] = 0;
        Debug.Log($"Apple removed at ({x}, {y})");

        if (!HasCombinationLeft())
        {
            Debug.Log("조합 불가 → 그리드 리셋");
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

        PlayerUISingle.Instance.ShowNotifyPanelForSeconds(2f);
        GameTimerSingle.Instance.PauseTimerForSeconds(2f);


        SpawnApplesInGrid();
    }

    /// <summary>
    /// 10 합계 조합이 남아있는지 검사
    /// </summary>
    private bool HasCombinationLeft()
    {
        //디버그 체크용도
        //DebugCheckGridValues();

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

        // 부분 직사각형 합 검사
        for (int r1 = 0; r1 < rows; r1++)
            for (int c1 = 0; c1 < cols; c1++)
                for (int r2 = r1; r2 < rows; r2++)
                    for (int c2 = c1; c2 < cols; c2++)
                    {
                        if (r1 == r2 && c1 == c2) continue; // 단일 칸 제외

                        int sum = ps[r2, c2]
                            - (r1 > 0 ? ps[r1 - 1, c2] : 0)
                            - (c1 > 0 ? ps[r2, c1 - 1] : 0)
                            + (r1 > 0 && c1 > 0 ? ps[r1 - 1, c1 - 1] : 0);

                        if (sum == 10) return true;
                    }
        return false;
    }

    private void DebugCheckGridValues()
    {
        // 1) appleGrid 에서 읽어온 값 vs appleValues 비교
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int gridVal = appleGrid[y, x] != null
                    ? appleGrid[y, x].Value
                    : 0;
                int arrVal = appleValues[y, x];
                if (gridVal != arrVal)
                {
                    Debug.LogWarning(
                        $"[Mismatch] 위치 ({y},{x}) → " +
                        $"gridVal={gridVal}, appleValues={arrVal}"
                    );
                }
            }
        }

        // 2) appleValues 전체 출력 (Top → Bottom)
        Debug.Log("▶ appleValues (Top → Bottom):");
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            string line = "";
            for (int x = 0; x < gridWidth; x++)
            {
                line += appleValues[y, x] + " ";
            }
            Debug.Log(line.TrimEnd());
        }
    }

    // 20250505
    public void AddDetectSize()
    {
        if (appleDetectSize < 0.75f)
        {
            appleDetectSize += 0.075f;
            var apples = FindObjectsByType<AppleSingle>(FindObjectsSortMode.None);
            foreach(var apple in apples)
            {
                apple.detectSize = appleDetectSize;
            }

        }
        curText.text = appleDetectSize.ToString();
    }
    public void SubDetectSize()
    {
        if (appleDetectSize > 0.15f)
        {
            appleDetectSize -= 0.075f;
            var apples = FindObjectsByType<AppleSingle>(FindObjectsSortMode.None);
            foreach (var apple in apples)
            {
                apple.detectSize = appleDetectSize;
            }

        }
        curText.text = appleDetectSize.ToString();
    }
}
