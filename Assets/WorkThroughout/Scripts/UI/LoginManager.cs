#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Login UI")]
    public Button guestLoginButton;
    public Button googleLoginButton;
    public GameObject loginPanel;
    public GameObject downCheck;

    private DownManager downManager;
    private void Awake()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        // 로딩패널 하나 setactive true 로
#endif
    }

    private void Start()
    {
        downManager = FindAnyObjectByType<DownManager>();

        guestLoginButton.onClick.AddListener(OnGuestLoginButtonClick);
        googleLoginButton.onClick.AddListener(OnGoogleLoginButtonClick);


        int isGoogleLogin = PlayerPrefs.GetInt("IsGoogleLogin",0);
        int isGuestLogin = PlayerPrefs.GetInt("IsGuestLogin", 0);     

        Debug.Log("Game Start: Attempting silent login.");

        if (isGoogleLogin == 0 && isGuestLogin == 0)
            loginPanel.SetActive(true);
#if UNITY_ANDROID
        else if(isGuestLogin == 1 && isGoogleLogin == 1)
            PlayGamesPlatform.Instance.Authenticate(ProcessAutoAuthentication);
        else if (isGoogleLogin == 1)
            PlayGamesPlatform.Instance.Authenticate(ProcessAutoAuthentication);
#endif
        else if (isGuestLogin == 1)
            toDownCheck();
    }
#if UNITY_ANDROID
    private void ProcessAutoAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            AndroidToast.ShowToast("구글 로그인 성공! 다운로드 할 파일이 있는지 체크합니다.");
            DisableLoginButtons();
            toDownCheck();
        }
        else
        {
            AndroidToast.ShowToast("구글 로그인 실패! 로그인 페이지로 이동합니다.");
            loginPanel.SetActive(true);
            EnableLoginButtons();
        }
    }
#endif
    public void OnGuestLoginButtonClick()
    {
        Debug.Log("Guest login selected.");
        TransDataClass.deviceIdToApply = SystemInfo.deviceUniqueIdentifier;
        PlayerPrefs.SetInt("IsGuestLogin", 1);
        toDownCheck();
    }

    public void OnGoogleLoginButtonClick()
    {
        Debug.Log("Google login button clicked.");

        DisableLoginButtons();
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(ProcessManualAuthentication);
#endif
    }
#if UNITY_ANDROID
    private void ProcessManualAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {

            AndroidToast.ShowToast("구글 로그인 성공! 다운로드 할 파일이 있는지 체크합니다.");
            TransDataClass.googleIdToApply = PlayGamesPlatform.Instance.GetUserId();
            TransDataClass.deviceIdToApply = SystemInfo.deviceUniqueIdentifier;

            PlayerPrefs.SetInt("IsGoogleLogin", 1);

            toDownCheck();
        }
        else
        {
            AndroidToast.ShowToast("구글 로그인 실패! 로그인 페이지로 이동합니다.");
            Debug.LogWarning($"Manual Google login failed or canceled: {status}");
            EnableLoginButtons();
        }
    }
    private IEnumerator WaitUntilAuthenticated()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (!PlayGamesPlatform.Instance.localUser.authenticated && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.Log("Silent login success after waiting");
            // 로딩 패널 false
            toDownCheck();
        }
        else
        {
            Debug.Log("Silent login failed after timeout");
            EnableLoginButtons();
            // 로딩 패널 false
            loginPanel.SetActive(true);
        }
    }

#endif
    private void DisableLoginButtons()
    {
        guestLoginButton.interactable = false;
        googleLoginButton.interactable = false;
    }

    private void EnableLoginButtons()
    {
        guestLoginButton.interactable = true;
        googleLoginButton.interactable = true;
    }

    private void toDownCheck()
    {
        loginPanel.SetActive(false);
        downCheck.SetActive(true);
        StartCoroutine(downManager.CheckUpdateFiles());
    }
}
