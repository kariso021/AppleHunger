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
        // NetworkManager 에 붙어 있는 UTP 가져오기
        utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (utp == null)
            Debug.LogError("UnityTransport 컴포넌트를 NetworkManager에 붙여주세요!");

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
        //  Relay 모드로 전환
        SetTransportMode(UnityTransport.ProtocolType.RelayUnityTransport);

        gameModePanel.SetActive(false);
        matchmakingPanel.SetActive(true);

        // 이제 StartClient / StartHost 등 네트워크 시작
        NetworkManager.Singleton.StartClient();
    }

    private void EnterFriendlyMode()
    {
        // 기본(UDP) 모드로 전환
        SetTransportMode(UnityTransport.ProtocolType.UnityTransport);

        gameModePanel.SetActive(false);
        friendlyPanel.SetActive(true);

        NetworkManager.Singleton.StartHost();
    }

    private void ReturnToLobby()
    {
        // 씬 전환 전 네트워크 셧다운
        if (NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("Lobby");
    }

    /// <summary>
    /// 인스펙터 enum 으로 선택된 Protocol 타입을 런타임에 적용합니다.
    /// </summary>
    private void SetTransportMode(UnityTransport.ProtocolType mode)
    {
        if (utp == null) return;
           // 인스펙터에 노출된 enum 필드
        utp.Initialize();                 // 드라이버 재생성 필요하다면 호출
        Debug.Log($"Transport mode set to {mode}");
    }
}
