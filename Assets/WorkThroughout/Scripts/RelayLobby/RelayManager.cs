using System;
using Unity.Services.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEditor.PackageManager;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    public event Action<string> OnJoinCodeCreated;   // 코드 생성 알림

    // 클라이언트 입장 성공 시
    public event Action OnJoinSucceeded;

    //상대 입장 신호
    public event Action<ulong> OnClientJoined;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log($"Signed In: {AuthenticationService.Instance.PlayerId}");
    }

    // 호스트용
    public async void CreateRelay()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(2);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // UI에 코드 전달
            OnJoinCodeCreated?.Invoke(joinCode);

            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            utp.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();

            //들어오는 클라이언트 감지
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;


        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay 생성 실패: {e.Message}");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // 호스트 자신(로컬) 연결 콜백은 무시
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        Debug.Log($"[RelayManager] Remote client joined: {clientId}");
        OnClientJoined?.Invoke(clientId);
        // 더 이상 추가 접속이 필요 없으면 콜백 해제
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    // 클라이언트용
    public async void JoinRelay(string joinCode)
    {
        try
        {
            var alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log($"Joined relay: {alloc.AllocationId}");

            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            utp.SetClientRelayData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData,
                alloc.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay 참가 실패: {e.Message}");
        }
    }


    private void OnClientConnectedLocal(ulong clientId)
    {
        // 내 클라이언트가 성공적으로 연결됐을 때만 처리
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[RelayManager] Client successfully connected to host");
            OnJoinSucceeded?.Invoke();
            // 다시는 호출되지 않도록 해제
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedLocal;
        }
    }
}
