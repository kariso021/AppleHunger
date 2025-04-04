using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using StatusOptions = Unity.Services.Matchmaker.Models.MultiplayAssignment.StatusOptions;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#if UNITY_EDITOR
using ParrelSync;
#endif


public class MatchMakerClient : MonoBehaviour
{


    private string _ticketId;
    [SerializeField] private GameObject waitingCanvas;
    [SerializeField] private TMPro.TextMeshProUGUI matchResultText;


    private void OnEnable()
    {
        ServerStartUp.ClientInstance += SignIn;
    }

    private void OnDisable()
    {
        ServerStartUp.ClientInstance -= SignIn;
    }

    private async void SignIn()
    {
        await ClientSignIn(serviceProfileName:"AppleHungerPlayer");
        //�����ͺ��̽����� ������ ������ ������ ����...

        if(!AuthenticationService.Instance.IsSignedIn)//�α��� �ȵǾ������� �α��� ��Ű��(�׽�Ʈ������ �͸�α��� �������. ���߿� �ٲ����.)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        else {
            Debug.Log($"Already Signed In as {PlayerID()}");
            }

        

        //���⿡ Ŭ�����
        StartClient();


    }

    private async Task ClientSignIn(string serviceProfileName = null)
    {
        if (serviceProfileName != null)
        {
#if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{GetCloneNumberSuffix()}";
#endif
            var initOptions = new InitializationOptions();
            initOptions.SetProfile(serviceProfileName);
            await UnityServices.InitializeAsync(initOptions);
        }
        else
        {
            await UnityServices.InitializeAsync();
        }
        Debug.Log($"Singed In Anoymously as {serviceProfileName}({PlayerID()})");
    }

    private string PlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

#if UNITY_EDITOR
    private string GetCloneNumberSuffix()//�׽��ÿ�����  Parrrel Sink ��� ����
    {
        string projectPath = ClonesManager.GetCurrentProjectPath();
        int lastUnderScore = projectPath.LastIndexOf('_');
        string projectCloneSuffix = projectPath.Substring(lastUnderScore + 1);
        if (projectCloneSuffix.Length != 1)
        {
            projectCloneSuffix = "";
        }
        return projectCloneSuffix;
    }
#endif


    public void StartClient() 
    {
        //��Ī ĵ���� Ȱ��ȭ
        if (waitingCanvas != null)
            waitingCanvas.SetActive(true);


        CreateATicket();
    }

    private async void CreateATicket()
    {
        var options = new CreateTicketOptions(queueName: "AppleMode");

        var players = new List<Player>
            {
            new Player(
                PlayerID(),
                new MatchmakingPlayerData
                {
                    //�̺κ��� �������� �Ǿ�� �ϰ� �׿� �´� ��Ī�� ��������
                    //Rating=SQLiteManager.Instance.player.rating,
                    Rating = 100,
    
                }
            )
            };

        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players,options);
        _ticketId = ticketResponse.Id;
        Debug.Log(message: $"ticket Id : {_ticketId}");
        PollTicketStatus();

    }

    private async void PollTicketStatus()
    {
        MultiplayAssignment multiplayAssignment = null;
        bool gotAssingment = false;
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(_ticketId);
            if (ticketStatus == null) continue;
            if(ticketStatus.Type == typeof(MultiplayAssignment))
            {
                multiplayAssignment = ticketStatus.Value as MultiplayAssignment;
            }
            switch (multiplayAssignment.Status)
            {
                case StatusOptions.Found:
                    gotAssingment = true;
                    TicketAssigned(multiplayAssignment);
                    Debug.Log("Tiket Assinged");
                    break;
                case StatusOptions.InProgress:
                    break;
                case StatusOptions.Failed:
                    gotAssingment = true;
                    Debug.LogError(message : $"Failed to get Ticket Status. Error : {multiplayAssignment.Message}");
                    break;
                case StatusOptions.Timeout:
                    gotAssingment = true;
                    Debug.LogError(message: $"Failed to get ticket because of Timeout");
                    break;
                default:
                    throw new InvalidOperationException();
            }


        }while (!gotAssingment);
    }

    private void TicketAssigned(MultiplayAssignment assignment)
    {
        Debug.Log(message: $"[Ŭ���̾�Ʈ] Ticket Assigned : {assignment.Ip} : {assignment.Port}");
        Debug.Log(message: $"Ticket Assigned : {assignment.Ip} : {assignment.Port}");

        //��Ī������ ǥ���ϴ� �κ�
        matchResultText.text = "��Ī�� �������ϴ�!";

        // ��Ī �Ϸ�Ǹ� ��� ĵ���� �����
        if (waitingCanvas != null)
            waitingCanvas.SetActive(false);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        NetworkManager.Singleton.StartClient();
    }

    [Serializable]
    public class MatchmakingPlayerData
    {
        public int Rating;
    }


    private bool IsDedicatedServerMode()
    {
        var args = Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            if (arg == "-dedicatedServer")
                return true;
        }
        return false;
    }

}
