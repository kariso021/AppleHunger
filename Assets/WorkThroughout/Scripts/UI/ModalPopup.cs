using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ModalPopup : MonoBehaviour
{
    [Header("Config")]
    public TMP_Text title;
    public TMP_Text config;
    public Button btn_confirm;
    public Button btn_cancel;
    public Button btn_touchEvent;

    // Start is called before the first frame update
    void Start()
    {
        btn_cancel.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());

        title.text = "°æ°í";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
