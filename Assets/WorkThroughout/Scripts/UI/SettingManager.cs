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
#if UNITY_ANDROID && !UNITY_EDITOR
        loginButton.onClick.AddListener(() =>
        PlayGamesPlatform.Instance.Authenticate(ProcessManualAuthentication));
#endif
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
#if UNITY_ANDROID && !UNITY_EDITOR
    private void ProcessManualAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            // db 파일을 새로 받기 위한 삭제 과정
            string rawDbPath = Path.Combine(UnityEngine.Application.persistentDataPath, "game_data.db").Replace("\\", "/");
            if (File.Exists(rawDbPath))
            {
                File.Delete(rawDbPath);
                Debug.Log($"[Setting] 로컬 DB 파일 삭제 완료: {rawDbPath}");
            }

            // PlayerPrefs로 로그인 처리
            PlayerPrefs.SetInt("isGoogleLogin", 1);

            // 팝업 처리
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => SceneManager.LoadScene("Down"));
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "구글 계정 연결을 위해 게임을 재실행 합니다. \n" + "기존에 게스트로 플레이한 정보는 삭제됩니다.";



        }
        else
        {
            AndroidToast.ShowToast("구글 로그인 실패!");
        }
    }
#endif
}
