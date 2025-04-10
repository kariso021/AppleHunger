using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float spacing = 1.1f;

    private void Start()
    {
        FitCameraToGrid();
    }

    private void FitCameraToGrid()
    {
        float gridWorldWidth = gridWidth * spacing;
        float gridWorldHeight = gridHeight * spacing;

        float screenAspect = (float)Screen.width / Screen.height;

        // 가로 기준 크기 계산
        float cameraSizeByWidth = gridWorldWidth / screenAspect / 2f;
        float cameraSizeByHeight = gridWorldHeight / 2f;

        // 둘 중 더 큰 값 사용 (그리드가 다 보이도록)
        Camera.main.orthographicSize = Mathf.Max(cameraSizeByWidth, cameraSizeByHeight);
    }
}