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
        // ��� ����
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
        //transport �� ���� �и��ϰ� �ٲ���� networkmanager ����


        gameModePanel.SetActive(false);
        matchmakingPanel.SetActive(true);
        // MatchmakerManager.Instance.StartMatchmaking(); // �ʿ�� ȣ��
    }

    private void EnterFriendlyMode()
    {
        //transport �� ���� �и��ϰ� �ٲ���� networkmanager ����


        gameModePanel.SetActive(false);
        friendlyPanel.SetActive(true);
        // RelayLobbyUI ���� Create/Join �г� ��� ó��
        
    }

    private void ReturnToLobby()
    {
        //NetworkManager �����ؾ߰���.
        Destroy(Nobj);
        SceneManager.LoadScene("Lobby");
    }
}