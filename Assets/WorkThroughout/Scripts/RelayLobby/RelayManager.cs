using System;
using Unity.Services.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    public event Action<string> OnJoinCodeCreated;   // 코드 생성 알림

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
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay 생성 실패: {e.Message}");
        }
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
}
