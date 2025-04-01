using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonClickSound : MonoBehaviour, IPointerClickHandler
{
    [Header("VFX ���� �ε���")]
    public int clickSoundIndex = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlayVFX(clickSoundIndex,0);
    }
}
