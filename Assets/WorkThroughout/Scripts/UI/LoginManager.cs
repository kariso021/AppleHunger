using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    private DownManager downManager;

    [Header("Login Buttons")]
    public Button guestLoginButton;
    public Button googleLoginButton;
    // Start is called before the first frame update
    void Start()
    {
        downManager = FindAnyObjectByType<DownManager>();

        guestLoginButton.onClick.AddListener(() => OnGuestLoginButtonClick());
        googleLoginButton.onClick.AddListener(() => OnGoogleLoginButtonClick());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGuestLoginButtonClick()
    {
        TransDataClass.deviceIdToApply = SystemInfo.deviceUniqueIdentifier;
        NavManager.LoadScene("Lobby");
    }

    public void OnGoogleLoginButtonClick()
    {
        TransDataClass.deviceIdToApply = SystemInfo.deviceUniqueIdentifier; // ���۷� �α����ص� ����̽� ���̵�� �׻� ������ �̷������ ��
        //TransDataClass.googleIdToApply = ���۾��̵�?

        NavManager.LoadScene("Lobby");
    }
}
