using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerDataManager : NetworkBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private Dictionary<ulong, int> clientIdToNumber = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> clientIdToRating = new Dictionary<ulong, int>();
    private Dictionary<ulong, string> clientIdToProfile = new Dictionary<ulong, string>();

    private HashSet<ulong> readyClientIds = new HashSet<ulong>();

    // Register완료형 변수
    public static event Action<ulong> OnPlayerFullyRegistered;


    public Dictionary<ulong, int> GetAllMappings()
    {
        return new Dictionary<ulong, int>(clientIdToNumber);
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        OnPlayerFullyRegistered += HandlePlayerRegistered;
    }

    private void OnDisable()
    {
        OnPlayerFullyRegistered -= HandlePlayerRegistered;
    }


    public override void OnNetworkSpawn()
    {

        Debug.Log("[PlayerDataManager] OnNetworkSpawn() 호출됨. 네트워크에 스폰 완료!");
    }

    public void RegisterPlayerNumber(ulong clientId, int number)
    {
        if (!IsServer) return;
        Debug.Log($"[ServerRpc] RegisterPlayerNumberServerRpc 호출됨! number = {number}");

        if (!clientIdToNumber.ContainsKey(clientId))
        {
            clientIdToNumber[clientId] = number;
            Debug.Log($"[Server] Registered ClientID {clientId} -> Number {number}");
        }
    }

    public int GetNumberFromClientID(ulong clientId)
    {
        return clientIdToNumber.TryGetValue(clientId, out var number) ? number : -1;
    }

    public void Unregister(ulong clientId)
    {
        clientIdToNumber.Remove(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerNumberServerRpc(int number, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId; //클라이언트에서 조작을 하지 않기 위해서 여기서 송신자를 가리는거임(서버에서 실행)
        RegisterPlayerNumber(clientId, number);
    }

    public void RegisterPlayerRating(ulong clientId, int rating)
    {
        if (!IsServer) return;

        clientIdToRating[clientId] = rating;
        Debug.Log($"[Server] Registered ClientID {clientId} -> Rating {rating}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerRatingServerRpc(int rating, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        RegisterPlayerRating(clientId, rating);
    }

    public int GetRatingFromClientID(ulong clientId)
    {
        return clientIdToRating.TryGetValue(clientId, out var rating) ? rating : -1;
    }



    public string GetIconNumberFromClientID(ulong clientId)
    {
        return clientIdToProfile.TryGetValue(clientId, out var IconNumber) ? IconNumber : "0";
    }






    //--------------------------------------------------------------------------------UI 관리하기 위한 아이디 식별자public
    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerProfileServerRpc(string profileIcon, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log("프로필 등록부분 서버부분에서 작동");
        RegisterPlayerProfile(clientId, profileIcon);
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            PlayerUI.Instance.SetOpponentProfileImage(profileIcon);
        }

    }

    public void RegisterPlayerProfile(ulong clientId, string profileIcon)
    {
        if (!IsServer) return;

        clientIdToProfile[clientId] = profileIcon;
        Debug.Log($"[Server] Registered ClientID {clientId} -> Profile {profileIcon}");
    }

   


    //--------------------------------------------------------------------------------- notifyplayer Ready 부분

    [ServerRpc(RequireOwnership = false)]
    public void NotifyPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        Debug.Log($"✅ 클라이언트 {senderClientId} 준비 완료, 이벤트 발행!");
        OnPlayerFullyRegistered?.Invoke(senderClientId);
    }

    [ClientRpc]
    public void SendOpponentProfileClientRpc(string profileIcon, ulong clientId, ClientRpcParams rpcParams = default)
    {

        Debug.Log("Client RPC 발생함");
        if (NetworkManager.Singleton.LocalClientId != clientId)
        {
            PlayerUI.Instance?.SetOpponentProfileImage(profileIcon);
        }
    }

    //여기서 보내는 id 
    private void HandlePlayerRegistered(ulong clientId)
    {
        Debug.Log($"🎯 서버 이벤트 발생 - 등록된 클라이언트: {clientId}");


        string icon = clientIdToProfile[clientId];
        Debug.Log($"{icon}");


        SendOpponentProfileClientRpc(icon, clientId);
    }






}




