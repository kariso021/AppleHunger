using GooglePlayGames;
using GooglePlayGames.BasicApi;
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
    public GameObject downCheck;
    public GameObject loginPanel;
    // Start is called before the first frame update
    private void Awake()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }
    void Start()
    {
        downManager = FindAnyObjectByType<DownManager>();

        guestLoginButton.onClick.AddListener(() => OnGuestLoginButtonClick());
        googleLoginButton.onClick.AddListener(() => OnGoogleLoginButtonClick());


        // 이미 로그인 되어있는 경우
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.Log("Already Logined");
            toDownCheck();
        }
        else
        {
            Debug.Log("Not yet");
        }
    }

    // Update is called once per frame
    public void GoogleSignIn()
    {
        Debug.Log("Google Login Start");
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }
    internal void ProcessAuthentication(SignInStatus status)
    {
        Debug.Log($"[Google Sign-In Response Status] {status}");

        if (status == SignInStatus.Success)
        {
            string googleId = PlayGamesPlatform.Instance.GetUserId();
            TransDataClass.googleIdToApply = googleId;
            TransDataClass.deviceIdToApply = SystemInfo.deviceUniqueIdentifier;

            Debug.Log($" Google Sign-In Success: {googleId}");

            toDownCheck();
        }
        else
        {
            if (status == SignInStatus.InternalError)
            {
                Debug.LogWarning("[Internal Error] Internal Error - Google Sign-In failed.\n" +
                               "Please check the following:\n" +
                               "- Missing SHA-1 in Firebase/Google Cloud Console\n" +
                               "- Incorrect OAuth2 configuration\n" +
                               "- Google Play Games not published");
            }
            else if (status == SignInStatus.Canceled)
            {
                Debug.LogWarning("[Cancled] Google Sign-In canceled by the user.");
            }
            else
            {
                Debug.LogWarning($" Sign-In failed - Unknown error: {status.ToString()}");
            }

            // General failure handling
            Debug.Log("Google Sign-In failure handling completed.");
        }
    }


    public void OnGuestLoginButtonClick()
    {
        TransDataClass.deviceIdToApply = SystemInfo.deviceUniqueIdentifier;
        toDownCheck();
    }

    public void OnGoogleLoginButtonClick()
    {
        Debug.Log("Google Login Button Clicked");
        GoogleSignIn();
    }

    private void toDownCheck()
    {
        loginPanel.SetActive(false);
        downCheck.SetActive(true);
        StartCoroutine(downManager.CheckUpdateFiles());
    }
}