using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.IO;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            deviceIdText.text = SystemInfo.deviceUniqueIdentifier;
        }

        // Button Bind
        copyButton.onClick.AddListener(() =>
        CopyTextToClipboard());
        creditButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.creditPopup));
        closeButton.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());
        loginButton.onClick.AddListener(() =>
        PlayGamesPlatform.Instance.Authenticate(ProcessManualAuthentication));
    }
    void CopyTextToClipboard()
    {
        GUIUtility.systemCopyBuffer = deviceIdText.text;
    }
    // Update is called once per frame
    void Update()
    {

    }

    // Google Login
#if UNITY_ANDROID
    private void ProcessManualAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => SceneManager.LoadScene("Down"));

            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "구글 계정 연결을 위해 게임을 재실행 합니다. \n" + "기존에 게스트로 플레이한 정보는 삭제됩니다.";

            string rawDbPath = Path.Combine(Application.persistentDataPath, "game_data.db").Replace("\\", "/");
            
        }
        else
        {
            //AndroidToast.ShowToast("구글 로그인 실패! 로그인 페이지로 이동합니다.");
            //loginPanel.SetActive(true);
            //EnableLoginButtons();
        }
    }
#endif
}
