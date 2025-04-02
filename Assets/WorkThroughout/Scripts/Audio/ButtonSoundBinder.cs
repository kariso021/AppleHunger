using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundBinder : MonoBehaviour
{
    [Header("������ Ŭ�� ���� �ε���")]
    public int clickSoundIndex = 0;

    private void Awake()
    {
        // ��� Button ������Ʈ ã�� (��Ȱ��ȭ ����)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (var button in allButtons)
        {
            // �����Ϳ����� �����ϰų� ���� ���� Button�� ����
            if (!IsInScene(button.gameObject)) continue;

            var existing = button.GetComponent<ButtonClickSound>();
            if (existing == null)
            {
                var sound = button.gameObject.AddComponent<ButtonClickSound>();
                sound.clickSoundIndex = clickSoundIndex;
            }
            else
            {
                existing.clickSoundIndex = clickSoundIndex;
            }
        }
    }

    // ���� �����ϴ� ������Ʈ���� üũ (Hierarchy�� �ִ���)
    private bool IsInScene(GameObject go)
    {
        return go.hideFlags == HideFlags.None && go.scene.IsValid();
    }
}
