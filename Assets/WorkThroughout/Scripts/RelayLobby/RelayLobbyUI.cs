using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelayLobbyUI : MonoBehaviour
{
    [Header("Main Buttons")]
    public Button createRoomBtn;
    public Button joinRoomBtn;

    [Header("Create Room Panel")]
    public GameObject createRoomPanel;
    public TMP_Text codeText;
    public Button copyCodeBtn;
    public Button closeCreatePanelBtn;

    [Header("Join Room Panel")]
    public GameObject joinRoomPanel;
    public TMP_InputField joinInputField;
    public Button confirmJoinBtn;
    public Button closeJoinPanelBtn;

    void Start()
    {
        // Main
        createRoomBtn.onClick.AddListener(OpenCreatePanel);
        joinRoomBtn.onClick.AddListener(OpenJoinPanel);

        // Relay 이벤트 바인딩
        RelayManager.Instance.OnJoinCodeCreated += code => {
            codeText.text = code;
            copyCodeBtn.interactable = true;
        };

        // Create Panel
        copyCodeBtn.onClick.AddListener(() => {
            GUIUtility.systemCopyBuffer = codeText.text;
        });
        closeCreatePanelBtn.onClick.AddListener(() => createRoomPanel.SetActive(false));

        // Join Panel
        confirmJoinBtn.onClick.AddListener(() => {
            string code = joinInputField.text.Trim();
            if (!string.IsNullOrEmpty(code))
                RelayManager.Instance.JoinRelay(code);
        });
        closeJoinPanelBtn.onClick.AddListener(() => joinRoomPanel.SetActive(false));

        // 처음엔 둘 다 숨김
        createRoomPanel.SetActive(false);
        joinRoomPanel.SetActive(false);
    }

    void OpenCreatePanel()
    {
        createRoomPanel.SetActive(true);
        joinRoomPanel.SetActive(false);
        copyCodeBtn.interactable = false;
        codeText.text = "생성 중...";
        RelayManager.Instance.CreateRelay();
    }

    void OpenJoinPanel()
    {
        joinRoomPanel.SetActive(true);
        createRoomPanel.SetActive(false);
        joinInputField.text = "";
    }
}
