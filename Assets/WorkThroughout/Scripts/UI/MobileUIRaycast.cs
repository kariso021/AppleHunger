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
            // UI Ÿ�Կ� ���� �ٸ� �̺�Ʈ ����
            if (touchedUI.TryGetComponent(out Button button))
            {
                button.onClick.Invoke();
                Debug.Log($"[UI] ��ư Ŭ����: {button.name}");
            }
            else if (touchedUI.TryGetComponent(out Scrollbar scrollbar))
            {
                Debug.Log($"[UI] ��ũ�ѹ� Ŭ����: {scrollbar.name}");
            }
            else if (touchedUI.TryGetComponent(out Slider slider))
            {
                Debug.Log($"[UI] �����̴� Ŭ����: {slider.name}");
            }
            else if (touchedUI.TryGetComponent(out Toggle toggle))
            {
                toggle.isOn = !toggle.isOn;
                Debug.Log($"[UI] ��� �����: {toggle.name} �� {toggle.isOn}");
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

        return null; // ������ UI ��Ұ� ������ null ��ȯ
    }
}