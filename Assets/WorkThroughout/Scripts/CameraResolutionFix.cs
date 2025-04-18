using UnityEngine;

public class CameraResolutionFix : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        float screenAspect = (float)Screen.width / (float)Screen.height;
        float targetAspect = 9f / 16f; // 예시: 9:16 세로 게임 기준 (1080x1920 기준)

        if (screenAspect < targetAspect)
        {
            // 가로가 좁은 기기 (필요하면 높이를 줄인다)
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
            // 가로가 넓은 기기 → 높이 1 고정
            Rect rect = cam.rect;
            rect.width = targetAspect / screenAspect;
            rect.height = 1.0f;
            rect.x = (1.0f - rect.width) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}
