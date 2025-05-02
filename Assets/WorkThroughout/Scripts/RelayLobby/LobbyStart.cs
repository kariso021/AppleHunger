using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyStart : MonoBehaviour
{
    private Lobby hostLobby;
    //�ڷ�ƾ ������ ������ ���� �ȵ� �׳� Ÿ�̸� �����
    private float heartbeatTimer;


    private async void Start()
    {
         await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {

        HandleLobbyHeartbeat();
    }

    private async void HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
               heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 2;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                //QucikJoin ����� ����ϰ� �ʹٸ� IsPrivate�� false�� ����
                IsPrivate = true,
            };
      
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers,createLobbyOptions);

            hostLobby = lobby;
            Debug.Log("Lobby created: " + lobby.Name+" "+ lobby.MaxPlayers+ " " + lobby.Id );
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error creating lobby: " + e.Message);
        }
    }

    private async void ListLobbies()
    {
        try 
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>()
                { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)}
                ,
                Order = new List<QueryOrder>()
                { new QueryOrder(false,QueryOrder.FieldOptions.Created)}
            };
           
        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

        Debug.Log("Lobbies found: " + queryResponse.Results.Count);
        foreach (var result in queryResponse.Results) {
            Debug.Log("Lobby name: " + result.Name);
            }
        }
        catch(LobbyServiceException e)
            {
            Debug.Log(e);
            }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("Lobby joined: " + lobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            await Lobbies.Instance.QuickJoinLobbyAsync();
        }catch (LobbyServiceException e) {
            Debug.Log(e);
        }   
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Lobby Name: " + lobby.Name);
        foreach(var player in lobby.Players)
        {
            Debug.Log(player.Id);
        }
    }
}
