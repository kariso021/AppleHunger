using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    public GameObject currecnyLayout;
    public Image currencyIcon;
    public TMP_Text currencyText;
    private void Start()
    {
        if (currecnyLayout != null)
        {
            currencyIcon = currecnyLayout.GetComponentInChildren<Image>();
            currencyText = currecnyLayout.GetComponentInChildren<TMP_Text>();

            DataSyncManager.Instance.OnPlayerProfileChanged += DelayedCurrencyUpdate;
            Debug.Log("��ȭ ��ȭ ���");

            DelayedCurrencyUpdate(); // 20250331 �ϴ� ������ ó���� �صα� ����. �Ƹ� �� ������Ʈ���� �ε� ���������� ����������
            // ���� �ذ��ϱ⿣ �� ����
        }
    }

    private void OnDestroy()
    {
        if (DataSyncManager.Instance != null)
            DataSyncManager.Instance.OnPlayerProfileChanged -= DelayedCurrencyUpdate;

        CancelInvoke(nameof(currencyChanged));
    }

    private void DelayedCurrencyUpdate()
    {
        Invoke(nameof(currencyChanged), 0.5f); // ������ ����
    }

    private void currencyChanged()
    {
        Debug.Log($"��ȭ ��ȭ : {SQLiteManager.Instance.player.currency}");
        if (currencyText == null) Debug.Log("�ֳ�?");
        currencyText.text = SQLiteManager.Instance.player.currency.ToString();
    }

}
