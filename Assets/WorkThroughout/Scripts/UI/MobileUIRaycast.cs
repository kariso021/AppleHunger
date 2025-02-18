using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
public class MobileUIRaycast : MonoBehaviour
{
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += onFingerDown;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= onFingerDown;
    }

    private void onFingerDown(Finger finger)
    {
        GameObject touchedUI = getUIElementUnderTouch(finger.screenPosition);

        if (touchedUI != null)
        {
            // UI 타입에 따라 다른 이벤트 실행
            if (touchedUI.TryGetComponent(out Button button))
            {
                button.onClick.Invoke();
                Debug.Log($"[UI] 버튼 클릭됨: {button.name}");
            }
            else if (touchedUI.TryGetComponent(out Scrollbar scrollbar))
            {
                Debug.Log($"[UI] 스크롤바 클릭됨: {scrollbar.name}");
            }
            else if (touchedUI.TryGetComponent(out Slider slider))
            {
                Debug.Log($"[UI] 슬라이더 클릭됨: {slider.name}");
            }
            else if (touchedUI.TryGetComponent(out Toggle toggle))
            {
                toggle.isOn = !toggle.isOn;
                Debug.Log($"[UI] 토글 변경됨: {toggle.name} → {toggle.isOn}");
            }
        }
    }

    private GameObject getUIElementUnderTouch(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Button>() ||
                result.gameObject.GetComponent<Scrollbar>() ||
                result.gameObject.GetComponent<Slider>() ||
                result.gameObject.GetComponent<Toggle>())
            {
                return result.gameObject;
            }
        }

        return null; // 감지된 UI 요소가 없으면 null 반환
    }
}