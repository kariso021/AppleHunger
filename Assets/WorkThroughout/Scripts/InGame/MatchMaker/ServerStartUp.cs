using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEditor.Analytics;
using UnityEngine;
#if UNITY_SERVER
using Unity.Services.Multiplay;
#endif
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.UI;
using Unity.Services.Matchmaker.Models;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Unity.Services.Matchmaker;

public class ServerStartUp : MonoBehaviour
{
    public static event System.Action ClientInstance;

    private string _internalServerIP = "0.0.0.0";
    private string _externalServerIP = "0.0.0.0";
    private ushort _serverPort = 7777;

    private string _externalConnectionString => $"{_externalServerIP}:{_serverPort}";

#if UNITY_SERVER
    private IMultiplayService _multiplayService;
    private MultiplayEventCallbacks _serverCallbacks;
    private IServerEvents _serverEvents;
#endif

    private const int _multiplayServiceTimeout = 20000;

    private string _allocationId;
    private BackfillTicket _localBackfillTicket;
    private CreateBackfillTicketOptions _createBackfillTicketOptions;
    private const int _ticketCheckMs = 1000;
    private MatchmakingResults _matchmakingPayload;

    private bool _backfilling = false;

    async void Start()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-dedicatedServer") server = true;
            if (args[i] == "-port" && (i + 1 < args.Length)) _serverPort = (ushort)int.Parse(args[i + 1]);
            if (args[i] == "-ip" && (i + 1 < args.Length)) _externalServerIP = args[i + 1];
        }

        if (server)
        {
            StartServer();
            await StartServerServices();
        }
        else
        {
            ClientInstance?.Invoke();
        }
    }

    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(_internalServerIP, _serverPort);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
    }

    private void ClientDisconnected(ulong clientId)
    {
        if (!_backfilling && NetworkManager.Singleton.ConnectedClients.Count > 0 && NeedsPlayers())
        {
            BeginBackfilling(_matchmakingPayload);
        }
    }

    async Task StartServerServices()
    {
        await UnityServices.InitializeAsync();
#if UNITY_SERVER
        try
        {
            _multiplayService = MultiplayService.Instance;
            await _multiplayService.StartServerQueryHandlerAsync(10, "n/a", "n/a", "0", "n/a");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Something went wrong trying to set up the SQP Services");
        }

        try
        {
            _matchmakingPayload = await GetMatchMakerPayload(_multiplayServiceTimeout);
            if (_matchmakingPayload != null)
            {
                Debug.Log($"Got payload: {_matchmakingPayload}");
                await StartBackfill(_matchmakingPayload);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the Allocation & Backfill Services: {ex}");
        }
#endif
    }

    private async Task StartBackfill(MatchmakingResults payload)
    {
        var backfillProperties = new BackfillTicketProperties(payload.MatchProperties);
        new BackfillTicket { Id = payload.MatchProperties.BackfillTicketId, Properties = backfillProperties };
        await BeginBackfilling(payload);
    }

    private async Task BeginBackfilling(MatchmakingResults payload)
    {
        if (string.IsNullOrEmpty(_localBackfillTicket.Id))
        {
            var matchProperties = payload.MatchProperties;

            _createBackfillTicketOptions = new CreateBackfillTicketOptions
            {
                Connection = _externalConnectionString,
                QueueName = payload.QueueName,
                Properties = new BackfillTicketProperties(matchProperties)
            };
            _localBackfillTicket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(_createBackfillTicketOptions);
        }
        _backfilling = true;
#pragma warning disable 4014
        Backfillloop();
#pragma warning restore 4014
    }

    private async void Backfillloop()
    {
        while (NeedsPlayers())
        {
            _localBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(_localBackfillTicket.Id);
            if (_backfilling && !NeedsPlayers())
            {
                await MatchmakerService.Instance.DeleteBackfillTicketAsync(_localBackfillTicket.Id);
                _backfilling = false;
                return;
            }
            await Task.Delay(_ticketCheckMs);
        }
        _backfilling = false;
    }

    private bool NeedsPlayers()
    {
        return NetworkManager.Singleton.ConnectedClients.Count < 2;
    }

    private async Task<MatchmakingResults> GetMatchMakerPayload(int timeout)
    {
        var matchmakerPayloadTask = SubscribeAndAwiatMatchmakerAllocation();
        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }
        return null;
    }

    private async Task<MatchmakingResults> SubscribeAndAwiatMatchmakerAllocation()
    {
#if UNITY_SERVER
        if (_multiplayService == null) return null;
        _allocationId = null;
        _serverCallbacks = new MultiplayEventCallbacks();
        _serverCallbacks.Allocate += OnMultiplayAllocation;
        _serverEvents = await _multiplayService.SubscribeToServerEventsAsync(_serverCallbacks);

        _allocationId = await AwaitAllocationID();
        return await GetMatchMakerAllocationPayloadAsync();
#else
        return null;
#endif
    }

    private async Task<MatchmakingResults> GetMatchMakerAllocationPayloadAsync()
    {
#if UNITY_SERVER
        try
        {
            var payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log($"nameof(GetMatchMakerAllocationPayloadAsync) \n{modelAsJson}");
            return payloadAllocation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error in GetMatchMakerAllocationPayloadAsync: {ex}");
        }
#endif
        return null;
    }

    private async Task<string> AwaitAllocationID()
    {
#if UNITY_SERVER
        var config = _multiplayService.ServerConfig;
        while (string.IsNullOrEmpty(_allocationId))
        {
            var configId = config.AllocationId;
            if (!string.IsNullOrEmpty(configId))
            {
                _allocationId = configId;
                break;
            }
            await Task.Delay(100);
        }
        return _allocationId;
#else
        return null;
#endif
    }

#if UNITY_SERVER
    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {

        Debug.Log($"On Allocation: {allocation.AllocationId}");
        if (!string.IsNullOrEmpty(allocation.AllocationId))
        {
            _allocationId = allocation.AllocationId;
        }
}
#endif
private void Dispose()
    {
#if UNITY_SERVER
        if (_serverCallbacks != null)
            _serverCallbacks.Allocate -= OnMultiplayAllocation;
        _serverEvents?.UnsubscribeAsync();
#endif
    }
}
