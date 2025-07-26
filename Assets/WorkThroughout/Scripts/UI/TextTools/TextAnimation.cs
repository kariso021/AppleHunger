using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnimation : MonoBehaviour
{
    [SerializeField]
    private TMP_Text updateText;

    public string baseText;

    private int dotCount = 0;
    private int maxDots = 5;
    private float dotInterval = 0.5f;

    private Tween loopingTween;
    // Start is called before the first frame update
    void Awake()
    {
        updateText.text = baseText;
    }

    private void Start()
    {
    }

    public void StartDotLoop()
    {
        loopingTween = DOTween.Sequence()
            .AppendCallback(UpdateDotText)
            .AppendInterval(dotInterval)
            .SetLoops(-1);
    }

    private void UpdateDotText()
    {
        dotCount = (dotCount + 1) % (maxDots + 1); // 0 ¡æ maxDots ¼øÈ¯
        string dots = new string('.', dotCount);
        updateText.text = $"{baseText}{dots}";
    }

    public void StopDotLoop()
    {
        if (loopingTween != null && loopingTween.IsActive())
        {
            loopingTween.Kill();
        }
    }
}
