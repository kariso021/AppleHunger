using UnityEngine;

public class CameraResolutionFix : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        float screenAspect = (float)Screen.width / (float)Screen.height;
        float targetAspect = 9f / 16f; // ����: 9:16 ���� ���� ���� (1080x1920 ����)

        if (screenAspect < targetAspect)
        {
            // ���ΰ� ���� ��� (�ʿ��ϸ� ���̸� ���δ�)
            float scaleHeight = screenAspect / targetAspect;

            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else
        {
            // ���ΰ� ���� ��� �� ���� 1 ����
            Rect rect = cam.rect;
            rect.width = targetAspect / screenAspect;
            rect.height = 1.0f;
            rect.x = (1.0f - rect.width) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}
