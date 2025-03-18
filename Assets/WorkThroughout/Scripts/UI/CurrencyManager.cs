using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    public GameObject currecnyLayout;
    private Image currencyIcon;
    private TMP_Text currencyText;
    void Start()
    {
        if(currecnyLayout != null)
        {
            currencyIcon = currecnyLayout.GetComponentInChildren<Image>();
            currencyText = currecnyLayout.GetComponentInChildren<TMP_Text>();

            DataSyncManager.Instance.OnPlayerProfileChanged += currencyChanged;
        }
    }


    private void currencyChanged()
    {
        currencyText.text = SQLiteManager.Instance.player.currency.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
