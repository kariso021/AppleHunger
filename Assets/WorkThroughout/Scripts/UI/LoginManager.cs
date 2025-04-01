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
        TransDataClass.deviceIdToApply = SystemInfo.deviceUniqueIdentifier; // 구글로 로그인해도 디바이스 아이디는 항상 저장이 이루어져야 함
        //TransDataClass.googleIdToApply = 구글아이디?

        NavManager.LoadScene("Lobby");
    }
}
