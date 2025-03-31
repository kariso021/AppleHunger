using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyRankProfilePopupBinder : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        AddressableManager.Instance.myRankProfileIcon =
            this.gameObject.GetComponent<Image>();
    }
}
