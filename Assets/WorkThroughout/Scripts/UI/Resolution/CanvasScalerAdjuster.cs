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

        // ����̽��� �� ������ ��� -> Height ����
        if (deviceRatio < referenceRatio)
        {
            Debug.LogWarning("���ΰ� ���");
            canvasScaler.matchWidthOrHeight = 0f; // Height
        }
        // ����̽��� �� ������ ��� -> Width ����
        else
        {
            Debug.LogWarning("���ΰ� ���");
            canvasScaler.matchWidthOrHeight = 1f; // Width
        }
    }
}
