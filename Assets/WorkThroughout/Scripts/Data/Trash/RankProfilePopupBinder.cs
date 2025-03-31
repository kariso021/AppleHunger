using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankProfilePopupBinder : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        AddressableManager.Instance.rankProfilePopupIcon =
            this.gameObject.GetComponent<Image>();
    }
}
