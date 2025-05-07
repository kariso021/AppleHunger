#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using System.Collections;
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
        networkCheck());
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
#if UNITY_ANDROID
    private void networkCheck()
    {
        if(PlayerPrefs.GetInt("IsGoogleLogin") == 1)
        {
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "이미 구글 로그인이 되어있는 계정입니다.";
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.GetComponentInChildren<TMP_Text>().text = "확인";

            return;
        }

        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "인터넷 연결이 되어있지 않습니다. \n" + "인터넷 연결 후 로그인을 시도해주세요.";
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.GetComponentInChildren<TMP_Text>().text = "확인";
            Debug.Log("[Network] Network is not available");

            return;
        }
        PlayGamesPlatform.Instance.Authenticate(ProcessManualAuthentication);
    }

    private IEnumerator loginSuccessProcess()
    {
        string userGoogleId = AESUtil.Encrypt(PlayGamesPlatform.Instance.GetUserId());
        string userDeviceId = AESUtil.Encrypt(SystemInfo.deviceUniqueIdentifier);
        if (userGoogleId != null)
        {
            bool isSuccess = false;
            PlayerPrefs.SetString("GoogleId", userGoogleId);

            // PlayerPrefs로 로그인 처리
            PlayerPrefs.SetInt("IsGoogleLogin", 1);

            if (SQLiteManager.Instance.player.googleId == userGoogleId) yield break; // 게스트로 들어왔지만 이미 db에는 구글 로그인 이력이 남아있어서 정보가 남아있는 경우

            // players 테이블에 googleId로 검색을 했을 떄, 만약 null 값이 반환된다면 그냥 바로 updateGoogleId 로 현재 게스트 계정의 players 테이블에 구글 아이디 추가
            // null이 아니라면 아래 과정을 진행...
            // success 가 true면 이미 계정이 존재, false면 첫 연동
            yield return ClientNetworkManager.Instance.GetPlayerData(
                "googleId",
                userGoogleId,
                false, 
                success => { isSuccess = success; }
                );

            if (isSuccess) // 이미 구글 계정이 있는 유저일 때
            {

                yield return ClientNetworkManager.Instance.AddAuthMapping(userDeviceId, userGoogleId);

                yield return ClientNetworkManager.Instance.DeletePlayer(SQLiteManager.Instance.player.playerId);



                // db 파일을 새로 받기 위한 삭제 과정
                string rawDbPath = Path.Combine(UnityEngine.Application.persistentDataPath, "game_data.db").Replace("\\", "/");
                if (File.Exists(rawDbPath))
                {
                    File.Delete(rawDbPath); // 어차피 동기 작업이라 알아서 이게 끝나기 전까진 다음 내용 실행 안함
                    Debug.Log($"[Setting] Complete Delete Local DB : {rawDbPath}");
                }



                // 팝업 처리
                PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => AndroidInputManager.Instance.RestartApp());
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "구글 계정 연결을 위해 게임을 재실행 합니다. \n" + "기존에 게스트로 플레이한 정보는 삭제됩니다.";

            }
            else // 처음으로 구글 계정을 연동한 유저일 때
            {
                yield return ClientNetworkManager.Instance.UpdatePlayerGoogleId(SQLiteManager.Instance.player.playerId, userGoogleId);

                PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => AndroidInputManager.Instance.RestartApp());
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "구글 계정 연결을 위해 게임을 재실행 합니다.";
            }
        }
    }
    private void ProcessManualAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            StartCoroutine(loginSuccessProcess());

        }
        else
        {
            AndroidToast.ShowToast("구글 로그인 실패!");
        }
    }
#endif
}