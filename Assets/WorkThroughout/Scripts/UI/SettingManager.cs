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
    public Button loginButton; // �ϴ� ���� �α���? or guest?
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
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "�̹� ���� �α����� �Ǿ��ִ� �����Դϴ�.";
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.GetComponentInChildren<TMP_Text>().text = "Ȯ��";

            return;
        }

        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "���ͳ� ������ �Ǿ����� �ʽ��ϴ�. \n" + "���ͳ� ���� �� �α����� �õ����ּ���.";
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.GetComponentInChildren<TMP_Text>().text = "Ȯ��";
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

            // PlayerPrefs�� �α��� ó��
            PlayerPrefs.SetInt("IsGoogleLogin", 1);

            if (SQLiteManager.Instance.player.googleId == userGoogleId) yield break; // �Խ�Ʈ�� �������� �̹� db���� ���� �α��� �̷��� �����־ ������ �����ִ� ���

            // players ���̺� googleId�� �˻��� ���� ��, ���� null ���� ��ȯ�ȴٸ� �׳� �ٷ� updateGoogleId �� ���� �Խ�Ʈ ������ players ���̺� ���� ���̵� �߰�
            // null�� �ƴ϶�� �Ʒ� ������ ����...
            // success �� true�� �̹� ������ ����, false�� ù ����
            yield return ClientNetworkManager.Instance.GetPlayerData(
                "googleId",
                userGoogleId,
                false, 
                success => { isSuccess = success; }
                );

            if (isSuccess) // �̹� ���� ������ �ִ� ������ ��
            {

                yield return ClientNetworkManager.Instance.AddAuthMapping(userDeviceId, userGoogleId);

                yield return ClientNetworkManager.Instance.DeletePlayer(SQLiteManager.Instance.player.playerId);



                // db ������ ���� �ޱ� ���� ���� ����
                string rawDbPath = Path.Combine(UnityEngine.Application.persistentDataPath, "game_data.db").Replace("\\", "/");
                if (File.Exists(rawDbPath))
                {
                    File.Delete(rawDbPath); // ������ ���� �۾��̶� �˾Ƽ� �̰� ������ ������ ���� ���� ���� ����
                    Debug.Log($"[Setting] Complete Delete Local DB : {rawDbPath}");
                }



                // �˾� ó��
                PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => AndroidInputManager.Instance.RestartApp());
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "���� ���� ������ ���� ������ ����� �մϴ�. \n" + "������ �Խ�Ʈ�� �÷����� ������ �����˴ϴ�.";

            }
            else // ó������ ���� ������ ������ ������ ��
            {
                yield return ClientNetworkManager.Instance.UpdatePlayerGoogleId(SQLiteManager.Instance.player.playerId, userGoogleId);

                PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => AndroidInputManager.Instance.RestartApp());
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "���� ���� ������ ���� ������ ����� �մϴ�.";
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
            AndroidToast.ShowToast("���� �α��� ����!");
        }
    }
#endif
}