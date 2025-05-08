using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;               // UnityTransport

public class GameModeUIManager : MonoBehaviour
{
    public GameObject Nobj;

    [Header("Panels")]
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private GameObject friendlyPanel;

    [Header("Mode Buttons")]
    [SerializeField] private Button btnMatchmaking;
    [SerializeField] private Button btnFriendly;

    [Header("Cancle Button")]
    [SerializeField] private Button btnCancelandGotoLobby;

    private void Awake()
    {
        // 모드 진입
        btnMatchmaking.onClick.AddListener(EnterMatchmakingMode);
        btnFriendly.onClick.AddListener(EnterFriendlyMode);
        btnCancelandGotoLobby.onClick.AddListener(ReturnToLobby);
        ShowOnlyGameMode();
    }

    private void ShowOnlyGameMode()
    {
        gameModePanel.SetActive(true);
        matchmakingPanel.SetActive(false);
        friendlyPanel.SetActive(false);
    }

    private void EnterMatchmakingMode()
    {
        //transport 의 종류 분명하게 바꿔야함 networkmanager 에서


        gameModePanel.SetActive(false);
        matchmakingPanel.SetActive(true);
        // MatchmakerManager.Instance.StartMatchmaking(); // 필요시 호출
    }

    private void EnterFriendlyMode()
    {
        //transport 의 종류 분명하게 바꿔야함 networkmanager 에서


        gameModePanel.SetActive(false);
        friendlyPanel.SetActive(true);
        // RelayLobbyUI 에서 Create/Join 패널 토글 처리
        
    }

    private void ReturnToLobby()
    {
        //NetworkManager 삭제해야겠지.
        Destroy(Nobj);
        SceneManager.LoadScene("Lobby");
    }
}