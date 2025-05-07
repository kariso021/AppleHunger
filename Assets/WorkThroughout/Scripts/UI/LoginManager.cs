#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
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
#if UNITY_ANDROID && !UNITY_EDITOR
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        if (PlayerPrefs.GetInt("IsFirstLogin", 0) == 0)
        {
            Debug.Log("[Login] First launch detected. Resetting PlayerPrefs...");

            //  최초 실행임 → 초기화
            PlayerPrefs.SetInt("IsFirstLogin", 1);
            PlayerPrefs.SetInt("IsGoogleLogin", 0);
            PlayerPrefs.SetInt("IsGuestLogin", 0);
            PlayerPrefs.Save();

            string rawDbPath = Path.Combine(Application.persistentDataPath, "game_data.db").Replace("\\", "/");

            //  기존 DB 삭제도 여기서 같이 수행
            if (File.Exists(rawDbPath))
            {
                File.Delete(rawDbPath);
                Debug.Log($"[Login] Deleted old DB file at {rawDbPath}");
            }
        }
        // 로딩패널 하나 setactive true 로
#endif
        Debug.Log($"Goolge {PlayerPrefs.GetInt("IsGoogleLogin")}");
    }

    private void Start()
    {
        downManager = FindAnyObjectByType<DownManager>();

        guestLoginButton.onClick.AddListener(OnGuestLoginButtonClick);
        googleLoginButton.onClick.AddListener(OnGoogleLoginButtonClick);


        int isGoogleLogin = PlayerPrefs.GetInt("IsGoogleLogin",0);
        int isGuestLogin = PlayerPrefs.GetInt("IsGuestLogin", 0);

        Debug.Log("Game Start: Attempting silent login.");


        // 인터넷 연결 여부 확인
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            // 인터넷 연결이 안되어있다면?
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "인터넷 연결이 되어있지 않습니다. \n" + "인터넷 연결 후 로그인을 시도해주세요.";
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() =>Application.Quit());
            Debug.Log("[Network] Network is not available at login");
        }
        else
        {
            Debug.Log($"[Login] google? {isGoogleLogin} , guest? {isGuestLogin}");
            if (isGoogleLogin == 0 && isGuestLogin == 0)
                loginPanel.SetActive(true);
            else
                toDownCheck();
        }
    }
#if UNITY_ANDROID && !UNITY_EDITOR
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
        TransDataClass.deviceIdToApply = AESUtil.Encrypt(SystemInfo.deviceUniqueIdentifier);
        PlayerPrefs.SetInt("IsGuestLogin", 1);
        toDownCheck();
    }

    public void OnGoogleLoginButtonClick()
    {
        Debug.Log("Google login button clicked.");

        DisableLoginButtons();
#if UNITY_ANDROID && !UNITY_EDITOR
        PlayGamesPlatform.Instance.Authenticate(ProcessManualAuthentication);
#endif
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    private void ProcessManualAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {

            AndroidToast.ShowToast("구글 로그인 성공! 다운로드 할 파일이 있는지 체크합니다.");
            TransDataClass.googleIdToApply = AESUtil.Encrypt(PlayGamesPlatform.Instance.GetUserId());
            TransDataClass.deviceIdToApply = AESUtil.Encrypt(SystemInfo.deviceUniqueIdentifier);

            PlayerPrefs.SetInt("IsGoogleLogin", 1);
            PlayerPrefs.SetString("GoogleId",TransDataClass.googleIdToApply);
            toDownCheck();
        }
        else
        {
            AndroidToast.ShowToast("구글 로그인 실패! 다시 시도해 주세요.");
            Debug.LogWarning($"Manual Google login failed or canceled: {status}");
            EnableLoginButtons();
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
