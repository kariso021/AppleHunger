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
            Debug.Log("재화 변화 등록");

            DelayedCurrencyUpdate(); // 20250331 일단 강제로 처리를 해두긴 했음. 아마 각 오브젝트들의 로드 순서에따른 문제같은데
            // 당장 해결하기엔 좀 힘듬
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
        Invoke(nameof(currencyChanged), 0.5f); // 딜레이 적용
    }

    private void currencyChanged()
    {
        Debug.Log($"재화 변화 : {SQLiteManager.Instance.player.currency}");
        if (currencyText == null) Debug.Log("있냐?");
        currencyText.text = SQLiteManager.Instance.player.currency.ToString();
    }

}
