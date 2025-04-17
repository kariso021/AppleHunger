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
using System.Collections;
using UnityEngine.Networking;


#if UNITY_EDITOR
using ParrelSync;
#endif


public class MatchMakerClient : MonoBehaviour
{


    private string _ticketId;
    [SerializeField] private GameObject waitingCanvas;
    [SerializeField] private TMPro.TextMeshProUGUI matchResultText;

    [Serializable]
    private class UnityTokenResponse
    {
        public string idToken;
        public string sessionToken;
    }





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

        //--------------------------------------------------------------CustomId �α���


        if (!AuthenticationService.Instance.IsSignedIn)
        {
            string customId = SQLiteManager.Instance.player.playerId.ToString();
            await ClientNetworkManager.Instance.SignInWithCustomId(customId);
        }
        else
        {
            Debug.Log($"Already Signed In as {PlayerID()}");
        }




        //TODO : ������ �Ǵ� �ؾ��Ұ�

        // ������ �Ǵ�
        //������ playerid�� ��û�� ������ ���� ���������� �ƴ���
        //ingame == true && ���� ���� ���¶��
        //�������� �Ǵ��ϰ�
        // ������ �Ǵ��� �ϱ� ���ؼ� �ʿ��� ���� -> isReconnectable -> �Ǵ��� Isingame �̶� Isconnected �� �Ǵ� isingame == true && isconnected == false
        //NetworkManager.Singleton.GetComponent<Unitytransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        //������ �ٷ� �ϰ� ClientID Start Client�� �ϸ� �ȴ�.




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
        Debug.Log($"[Ŭ���̾�Ʈ] Ticket Assigned : {assignment.Ip} : {assignment.Port}");

        // �ٷ� �޽��� ǥ��
        matchResultText.text = "��Ī�� �������ϴ�!";

        // 2�� ��ٷȴٰ� ������ ó��
        StartCoroutine(DelayedConnectToServer(assignment));
    }





    private IEnumerator DelayedConnectToServer(MultiplayAssignment assignment)
    {
        yield return new WaitForSeconds(2f);

        // 2�� �Ŀ� ��� ĵ���� ����
        if (waitingCanvas != null)
            waitingCanvas.SetActive(false);

        // ���� ���� �õ�
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

    //----------------------------------------------------------------����Ƽ�� ó������


    private async void OnApplicationQuit()
    {
        await CancelTicketIfExists();
    }

    private async void OnDestroy()
    {
        await CancelTicketIfExists();
    }

    private async Task CancelTicketIfExists()
    {
        if (!string.IsNullOrEmpty(_ticketId))
        {
            try
            {
                await MatchmakerService.Instance.DeleteTicketAsync(_ticketId);
                Debug.Log($"[MatchMakerClient] Ticket {_ticketId} canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MatchMakerClient] Failed to cancel ticket: {ex.Message}");
            }
        }
    }
}
