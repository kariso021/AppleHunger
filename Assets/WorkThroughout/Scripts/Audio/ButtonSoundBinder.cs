using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundBinder : MonoBehaviour
{
    [Header("부착할 클릭 사운드 인덱스")]
    public int clickSoundIndex = 0;

    private void Awake()
    {
        // 모든 Button 컴포넌트 찾기 (비활성화 포함)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (var button in allButtons)
        {
            // 에디터에서만 존재하거나 씬에 없는 Button은 무시
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

    // 씬에 존재하는 오브젝트인지 체크 (Hierarchy에 있는지)
    private bool IsInScene(GameObject go)
    {
        return go.hideFlags == HideFlags.None && go.scene.IsValid();
    }
}
