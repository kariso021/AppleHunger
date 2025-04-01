using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonClickSound : MonoBehaviour, IPointerClickHandler
{
    [Header("VFX 사운드 인덱스")]
    public int clickSoundIndex = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlayVFX(clickSoundIndex,0);
    }
}
