using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI touchText;

    private void Start()
    {
        StartBlinking();
    }

    private void StartBlinking()
    {
        // 알파값 0 → 1 → 0 반복
        touchText.DOFade(0f, 1f)
            .SetLoops(-1, LoopType.Yoyo) // 무한 반복
            .SetEase(Ease.InOutSine);   // 부드러운 전환
    }
}
