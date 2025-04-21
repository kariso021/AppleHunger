using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerAdjuster : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private float standardWidth = 1080f;
    [SerializeField] private float standardHeight = 1920f;

    private void Awake()
    {
        if (canvasScaler == null)
            canvasScaler = GetComponent<CanvasScaler>();

        AdjustCanvasScaler();
    }

    private void AdjustCanvasScaler()
    {
        float deviceRatio = (float)Screen.width / (float)Screen.height;
        float referenceRatio = standardWidth / standardHeight;

        // 디바이스가 더 길쭉한 경우 -> Height 기준
        if (deviceRatio < referenceRatio)
        {
            Debug.LogWarning("가로가 길다");
            canvasScaler.matchWidthOrHeight = 0f; // Height
        }
        // 디바이스가 더 납작한 경우 -> Width 기준
        else
        {
            Debug.LogWarning("세로가 길다");
            canvasScaler.matchWidthOrHeight = 1f; // Width
        }
    }
}
