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

        // ���� ���� ũ�� ���
        float cameraSizeByWidth = gridWorldWidth / screenAspect / 2f;
        float cameraSizeByHeight = gridWorldHeight / 2f;

        // �� �� �� ū �� ��� (�׸��尡 �� ���̵���)
        Camera.main.orthographicSize = Mathf.Max(cameraSizeByWidth, cameraSizeByHeight);
    }
}