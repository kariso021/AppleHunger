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
        // ���İ� 0 �� 1 �� 0 �ݺ�
        touchText.DOFade(0f, 1f)
            .SetLoops(-1, LoopType.Yoyo) // ���� �ݺ�
            .SetEase(Ease.InOutSine);   // �ε巯�� ��ȯ
    }
}
