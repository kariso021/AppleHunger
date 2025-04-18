#if UNITY_ANDROID && !UNITY_SERVER
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
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
#if UNITY_ANDROID && !UNITY_SERVER
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif
    }
    void Start()
    {
        downManager = FindAnyObjectByType<DownManager>();

        guestLoginButton.onClick.AddListener(() => OnGuestLoginButtonClick());
        googleLoginButton.onClick.AddListener(() => OnGoogleLoginButtonClick());

#if UNITY_ANDROID && !UNITY_SERVER
        // 이미 로그인 되어있는 경우
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {           
            toDownCheck();
        }
#endif
    }



    // Update is called once per frame
    public void GoogleSignIn()
    {
#if UNITY_ANDROID && !UNITY_SERVER

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
#endif
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
