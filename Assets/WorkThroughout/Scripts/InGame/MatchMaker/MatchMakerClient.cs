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
        //데이터베이스에서 가져온 정보로 가공된 뭔가...

        if(!AuthenticationService.Instance.IsSignedIn)//로그인 안되어있으면 로그인 시키기(테스트용으로 익명로그인 사용중임. 나중에 바꿔야함.)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        else {
            Debug.Log($"Already Signed In as {PlayerID()}");
            }

        

        //여기에 클라시작
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
    private string GetCloneNumberSuffix()//테스팅용으로  Parrrel Sink 사용 가능
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
                    //이부분이 레이팅이 되어야 하고 그에 맞는 매칭을 잡아줘야함
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
        Debug.Log($"[클라이언트] Ticket Assigned : {assignment.Ip} : {assignment.Port}");

        // 바로 메시지 표시
        matchResultText.text = "매칭이 잡혔습니다!";

        // 2초 기다렸다가 나머지 처리
        StartCoroutine(DelayedConnectToServer(assignment));
    }


    private IEnumerator DelayedConnectToServer(MultiplayAssignment assignment)
    {
        yield return new WaitForSeconds(2f);

        // 2초 후에 대기 캔버스 끄기
        if (waitingCanvas != null)
            waitingCanvas.SetActive(false);

        // 서버 접속 시도
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
