using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePopupBinder : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("�̰͵� �� �Ǵ°���?");
        AddressableManager.Instance.profilePopupIcon =
            this.gameObject.GetComponent<Image>();
    }

}
