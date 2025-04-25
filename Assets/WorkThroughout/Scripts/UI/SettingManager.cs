using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text deviceIdText;

    [Header("Buttons")]
    public Button copyButton;
    public Button loginButton; // 일단 구글 로그인? or guest?
    public Button creditButton;
    public Button supportButton;
    public Button deleteButton;
    public Button closeButton;
    // Start is called before the first frame update
    void Awake()
    {
        if (deviceIdText != null)
        {
            if (SQLiteManager.Instance.player != null)
                deviceIdText.text = SQLiteManager.Instance.player.deviceId;
            else
                deviceIdText.text = SystemInfo.deviceUniqueIdentifier;
        }

        // Button Bind
        copyButton.onClick.AddListener(() =>
        CopyTextToClipboard());
        creditButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.creditPopup));
        closeButton.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());
    }
    void CopyTextToClipboard()
    {
        GUIUtility.systemCopyBuffer = deviceIdText.text;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
