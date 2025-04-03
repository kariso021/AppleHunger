using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDataManager : NetworkBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private Dictionary<ulong, int> clientIdToNumber = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> clientIdToRating = new Dictionary<ulong, int>();
    private Dictionary<ulong, string> clientIdToProfile = new Dictionary<ulong, string>();

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






    //--------------------------------------------------------------------------------UI 관리하기 위한 아이디 식별자
    


}


