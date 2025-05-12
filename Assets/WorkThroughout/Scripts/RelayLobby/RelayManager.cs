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
    public event Action<string> OnJoinCodeCreated;   // �ڵ� ���� �˸�

    // Ŭ���̾�Ʈ ���� ���� ��
    public event Action OnJoinSucceeded;

    //��� ���� ��ȣ
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

    // ȣ��Ʈ��
    public async void CreateRelay()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(2);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // UI�� �ڵ� ����
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

            //������ Ŭ���̾�Ʈ ����
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;


        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay ���� ����: {e.Message}");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // ȣ��Ʈ �ڽ�(����) ���� �ݹ��� ����
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        Debug.Log($"[RelayManager] Remote client joined: {clientId}");
        OnClientJoined?.Invoke(clientId);
        // �� �̻� �߰� ������ �ʿ� ������ �ݹ� ����
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    // Ŭ���̾�Ʈ��
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
            Debug.LogError($"Relay ���� ����: {e.Message}");
        }
    }


    private void OnClientConnectedLocal(ulong clientId)
    {
        // �� Ŭ���̾�Ʈ�� ���������� ������� ���� ó��
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[RelayManager] Client successfully connected to host");
            OnJoinSucceeded?.Invoke();
            // �ٽô� ȣ����� �ʵ��� ����
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedLocal;
        }
    }
}
