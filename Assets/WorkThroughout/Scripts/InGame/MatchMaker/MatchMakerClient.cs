﻿using NUnit.Framework;
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
using UnityEngine.UI;



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

    public Managers managers;


    private void OnEnable()
    {
        ServerStartUp.ClientInstance += SignIn;
    }

    private void OnDisable()
    {
        ServerStartUp.ClientInstance -= SignIn;
    }


    private void Awake()
    {
        waitingCanvas.SetActive(true);
    }





    private async void SignIn()
    {

        await ClientSignIn(serviceProfileName:"AppleHungerPlayer");
        //데이터베이스에서 가져온 정보로 가공된 뭔가...

        //--------------------------------------------------------------CustomId 로그인


        if (!AuthenticationService.Instance.IsSignedIn)
        {
            string customId = SQLiteManager.Instance.player.playerId.ToString();
            await ClientNetworkManager.Instance.SignInWithCustomId(customId);
        }
        else
        {
            Debug.Log($"Already Signed In as {PlayerID()}");
        }




        //TODO : 재접속 판단 해야할것

        // 재접속 판단
        //서버에 playerid로 요청을 보내서 현재 게임중인지 아닌지
        //ingame == true && 연결 끊긴 상태라면
        //재접속을 판단하고
        // 재접속 판단을 하기 위해서 필요한 변수 -> isReconnectable -> 판단은 Isingame 이랑 Isconnected 로 판단 isingame == true && isconnected == false
        //NetworkManager.Singleton.GetComponent<Unitytransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        //재접속 바로 하고 ClientID Start Client를 하면 된다.

        var session = SQLiteManager.Instance.LoadPlayerSession();
        bool isReconnect = session.isInGame;

        if (isReconnect)
        {
            // — 재접속 모드 —
            Debug.Log("[MatchMakerClient] 재접속 모드");

            // (A) 대기 UI
            waitingCanvas?.SetActive(true);

            Debug.Log($"session 제대로 작동되고 있나 보자{session.serverIp},{(ushort)session.serverPort}");

            // (B) 캐시된 IP/Port로 설정
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(session.serverIp, (ushort)session.serverPort);
            
            // (C) 클라이언트 시작
            NetworkManager.Singleton.StartClient();

            // (D) 연결되면 매핑 갱신
            NetworkManager.Singleton.OnClientConnectedCallback += OnReconnected;
        }

        else
        {
            // — 신규 매칭 모드 —
            Debug.Log("[MatchMakerClient] 신규 매칭 모드");
            StartClient();  // 내부에서 CreateTicket() 호출
        }
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
                    HandleTicketCancellation("매칭에 실패했습니다.");
                    Debug.LogError(message : $"Failed to get Ticket Status. Error : {multiplayAssignment.Message}");
                    break;
                case StatusOptions.Timeout:
                    gotAssingment = true;
                    HandleTicketCancellation("매칭 시간이 초과되었습니다.");
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

        SQLiteManager.Instance.playerSession.serverIp = assignment.Ip;
        SQLiteManager.Instance.playerSession.serverPort = (int)assignment.Port;
        SQLiteManager.Instance.playerSession.isInGame = true;

        SQLiteManager.Instance.SavePlayerSession(SQLiteManager.Instance.playerSession);


        StartCoroutine(managers.UpdatePlayerSessionCoroutine(
            SQLiteManager.Instance.player.playerId,
            true,
            success => Debug.Log("[MatchMakerClient] playerSession 업데이트 " +
                                  (success ? "성공" : "실패"))
        ));

       // 서버 잡혔다고 isingame 신호 주기
       


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

    //----------------------------------------------------------------유령티켓 처리문제


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

    //------------------------------------------------------------------Reconnect 처리문제

    private void OnReconnected(ulong newCid)
    {
        // 한 번만 수행
        NetworkManager.Singleton.OnClientConnectedCallback -= OnReconnected;
        waitingCanvas?.SetActive(false);

        // oldCid → newCid 매핑 갱신
        int playerId = SQLiteManager.Instance.player.playerId;

        // 이제 클라이언트는 바로 게임룸에 들어가게 됩니다.
    }


    //------------------------------------------------------------------재접속 매핑 갱신

    // --- Coroutine for double-check and reconnect ---
    private IEnumerator ValidateAndReconnect(PlayerSessionData session)
    {
        const float checkInterval = 2f;
        bool firstCheck = false, secondCheck = false;

        // 1차 확인
        yield return new WaitForSeconds(checkInterval);
        yield return CheckSessionInGame(session, result => firstCheck = result);

        // 2차 확인
        yield return new WaitForSeconds(checkInterval);
        yield return CheckSessionInGame(session, result => secondCheck = result);

        if (firstCheck && secondCheck)
        {
            // 방이 여전히 유효 → 실제 재접속
            waitingCanvas?.SetActive(true);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(session.serverIp, (ushort)session.serverPort);
            NetworkManager.Singleton.StartClient();
            NetworkManager.Singleton.OnClientConnectedCallback += OnReconnected;
        }
    }

    // 2) 서버에 isInGame 상태 물어보는 헬퍼
    private IEnumerator CheckSessionInGame(
     PlayerSessionData localSession,
     Action<bool> resultCallback)
    {
        // 서버에서 받아올 isInGame 상태
        bool serverInGame = false;

        // 1) 서버에 세션 업데이트 요청
        //    • localSession.isInGame 값을 그대로 넘기지만,
        //      managers.LatestSession 에 서버 응답이 채워집니다.
        yield return managers.UpdatePlayerSessionCoroutine(
            localSession.playerId,            // 플레이어 ID
            localSession.isInGame,            // 반드시 bool 인자 필요
            updateSuccess =>                  // 요청 성공 여부 콜백
            {
                if (updateSuccess)
                {
                    // 2) 서버 응답으로 갱신된 LatestSession에서 읽어오기
                    serverInGame = SQLiteManager.Instance.LoadPlayerSession().isInGame;
                }
                else
                {
                    Debug.LogWarning(
                        $"[CheckSession] playerId={localSession.playerId} 세션 조회 실패");
                }
            }
        );

        // 3) 최종 결과 전달
        resultCallback(serverInGame);
    }

    public void CancelMatch()
    {
        if (!string.IsNullOrEmpty(_ticketId))
        {
            // 비동기 취소 작업 시작
            CancelTicketIfExists();
            // _ticketId 초기화 (추후 재매칭 시 다시 사용 가능)
            _ticketId = null;
            Debug.Log("[MatchMakerClient] 매칭 취소 요청 완료");
        }
        else
        {
            Debug.Log("[MatchMakerClient] 취소할 매칭 티켓이 없습니다.");
        }
    }

    private void HandleTicketCancellation(string userMessage)
    {
        // 1) 티켓 서버 쪽에서도 삭제
        _ = CancelTicketIfExists();

        _ticketId = null;

        matchResultText.text = userMessage;

        Debug.Log($"[MatchMakerClient] {userMessage}");
    }


}
