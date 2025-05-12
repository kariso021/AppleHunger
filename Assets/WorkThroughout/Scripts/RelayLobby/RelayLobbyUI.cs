using System;
using System.Collections;
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
    public Button GotoLobbyBtn;


    [SerializeField]
    private TMP_Text waitingText;

    [SerializeField]
    private GameObject selfCanvas;


    void Start()
    {
        // Main
        createRoomBtn.onClick.AddListener(OpenCreatePanel);
        joinRoomBtn.onClick.AddListener(OpenJoinPanel);

        waitingText.text = "매칭 대기 중...";




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

        RelayManager.Instance.OnClientJoined += HandleClientJoined;
    }

    private void HandleClientJoined(ulong clientId)
    {
        waitingText.text = "매칭이 성사되었습니다 !";
        // 0.5초 대기 후 캔버스 비활성화
        StartCoroutine(HideCanvasAfterDelay(0.5f));
    }

    private IEnumerator HideCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        selfCanvas.SetActive(false);
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
