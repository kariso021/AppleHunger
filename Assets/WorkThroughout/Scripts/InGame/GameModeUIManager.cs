using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private GameObject friendlyPanel;

    [Header("Mode Buttons")]
    [SerializeField] private Button btnMatchmaking;
    [SerializeField] private Button btnFriendly;

    [Header("Cancel Button")]
    [SerializeField] private Button btnCancelandGotoLobby;

    private UnityTransport utp;

    private void Awake()
    {
        // NetworkManager �� �پ� �ִ� UTP ��������
        utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (utp == null)
            Debug.LogError("UnityTransport ������Ʈ�� NetworkManager�� �ٿ��ּ���!");

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
        //  Relay ���� ��ȯ
        SetTransportMode(UnityTransport.ProtocolType.RelayUnityTransport);

        gameModePanel.SetActive(false);
        matchmakingPanel.SetActive(true);

        // ���� StartClient / StartHost �� ��Ʈ��ũ ����
        NetworkManager.Singleton.StartClient();
    }

    private void EnterFriendlyMode()
    {
        // �⺻(UDP) ���� ��ȯ
        SetTransportMode(UnityTransport.ProtocolType.UnityTransport);

        gameModePanel.SetActive(false);
        friendlyPanel.SetActive(true);

        NetworkManager.Singleton.StartHost();
    }

    private void ReturnToLobby()
    {
        // �� ��ȯ �� ��Ʈ��ũ �˴ٿ�
        if (NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("Lobby");
    }

    /// <summary>
    /// �ν����� enum ���� ���õ� Protocol Ÿ���� ��Ÿ�ӿ� �����մϴ�.
    /// </summary>
    private void SetTransportMode(UnityTransport.ProtocolType mode)
    {
        if (utp == null) return;
           // �ν����Ϳ� ����� enum �ʵ�
        utp.Initialize();                 // ����̹� ����� �ʿ��ϴٸ� ȣ��
        Debug.Log($"Transport mode set to {mode}");
    }
}
