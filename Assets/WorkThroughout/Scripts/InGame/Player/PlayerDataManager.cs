using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerDataManager : NetworkBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private Dictionary<ulong, int> clientIdToPlayerId = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> clientIdToRating = new Dictionary<ulong, int>();
    private Dictionary<ulong, string> clientIdToIcon = new Dictionary<ulong, string>();
    private Dictionary<ulong, string> clientIdToNickname = new Dictionary<ulong, string>();

    private HashSet<ulong> readyClientIds = new HashSet<ulong>();

    public Managers managers;





    public Dictionary<ulong, int> GetAllMappings()
    {
        return new Dictionary<ulong, int>(clientIdToPlayerId);
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

        if (!clientIdToPlayerId.ContainsKey(clientId))
        {
            clientIdToPlayerId[clientId] = number;
            Debug.Log($"[Server] Registered ClientID {clientId} -> Number {number}");
        }
    }

    public int GetNumberFromClientID(ulong clientId)
    {
        return clientIdToPlayerId.TryGetValue(clientId, out var number) ? number : -1;
    }

    public void Unregister(ulong clientId)
    {
        clientIdToPlayerId.Remove(clientId);
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
        return clientIdToIcon.TryGetValue(clientId, out var IconNumber) ? IconNumber : "0";
    }






    //--------------------------------------------------------------------------------UI 관리하기 위한 아이디 식별자public
    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerProfileServerRpc(string profileIcon, ServerRpcParams rpcParams = default)
    {    
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log("프로필 등록부분 서버부분에서 작동");
        RegisterPlayerIcon(clientId, profileIcon);
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            PlayerUI.Instance.SetOpponentIconImage(profileIcon);
        }

    }

    public void RegisterPlayerIcon(ulong clientId, string profileIcon)
    {
        if (!IsServer) return;

        clientIdToIcon[clientId] = profileIcon;
        Debug.Log($"[Server] Registered ClientID {clientId} -> Profile {profileIcon}");
    }


    //---------------------------------------------------------------------------------------------

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerNickNameServerRpc(string nickName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        RegisterPlayerNickName(clientId, nickName);
    }



    public void RegisterPlayerNickName(ulong clientId, string nickName)
    {
        clientIdToNickname[clientId] = nickName;
    }




   


    //--------------------------------------------------------------------------------- notifyplayer Ready 부분

    [ServerRpc(RequireOwnership = false)]
    public void NotifyPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        if(!readyClientIds.Contains(senderClientId))
        {
            readyClientIds.Add(senderClientId);
        }


        if(readyClientIds.Count == 2)
        {
            Debug.Log("모든 플레이어가 준비 완료됨. 상대방 데이터 동기화 시작");
            SyncAllOpponentDataToEachClient();
        }
    }


    private void SyncAllOpponentDataToEachClient()
    {
        foreach(var clientid in readyClientIds)
        {
            string icon = clientIdToIcon[clientid];
            string nickname = clientIdToNickname[clientid];
            int rating = clientIdToRating[clientid];

            SendOpponentProfileClientRpc(icon, nickname, rating, clientid);
        }
    }



    [ClientRpc]
    public void SendOpponentProfileClientRpc(string profileIcon,string nickName, int rating, ulong clientId, ClientRpcParams rpcParams = default)
    {

        Debug.Log("Client RPC 발생함");
        if (NetworkManager.Singleton.LocalClientId != clientId)
        {
            PlayerUI.Instance?.SetOpponentIconImage(profileIcon);
            PlayerUI.Instance?.SetOpponentNickName(nickName);
            PlayerUI.Instance?.SetOpponentRating(rating);
        }
    }

    //--------------------------------------------------------------------------------- Emotion 관련

    [ServerRpc(RequireOwnership = false)]
    public void SendEmotionServerRpc(EmtionType emotion, ulong senderId)
    {
        SendEmotionClientRpc(emotion, senderId);
    }

    [ClientRpc]
    private void SendEmotionClientRpc(EmtionType emotion, ulong senderId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderId) return;

        // 이 시점에서 상대 감정 표시
        EmotionUI.Instance.ShowOpponentEmotion(emotion);
    }

    //clientid to playerid && playerid to Client ID

    public int GetPlayerIdFromClientId(ulong clientId)
    {
        return clientIdToPlayerId.TryGetValue(clientId, out var pid) ? pid : -1;
    }

    /// <summary>
    /// playerId → clientId 매핑 조회
    /// </summary>
    public ulong GetClientIdFromPlayerId(int playerId)
    {
        // 가장 먼저 매칭되는 clientId를 반환
        foreach (var kv in clientIdToPlayerId)
        {
            if (kv.Value == playerId)
                return kv.Key;
        }
        return 0UL;
    }


    //-------------------------------------------------------------------------------- 새로운 동기화 부분 관련

    [ServerRpc(RequireOwnership = false)]
    public void RequestReconnectServerRpc(int playerId, ServerRpcParams rpc = default)
    {
        ulong newCid = rpc.Receive.SenderClientId;
        ulong oldCid = GetClientIdFromPlayerId(playerId);
        if (oldCid == 0) return;

        // 1) copy & remove oldCid data
        var oldRating = clientIdToRating[oldCid];
        var oldIcon = clientIdToIcon[oldCid];
        var oldName = clientIdToNickname[oldCid];
        UnbindByPlayerId(playerId);

        // 2) bind newCid → playerId + restore data
        clientIdToPlayerId[newCid] = playerId;
        clientIdToRating[newCid] = oldRating;
        clientIdToIcon[newCid] = oldIcon;
        clientIdToNickname[newCid] = oldName;

        // 3) move score
        ScoreManager.Instance.HandleReconnect(oldCid, newCid);

        // 4) prepare RPC to only the reconnecting client
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { newCid }
            }
        };

        // 5) for every live client (you + opponent), call the same RPC
        foreach (var kv in clientIdToPlayerId)
        {
            ulong cid = kv.Key;
            int pid = kv.Value;
            int rating = clientIdToRating[cid];
            string icon = clientIdToIcon[cid];
            string nick = clientIdToNickname[cid];

            SyncPlayerStateClientRpc(pid, rating, icon, nick, cid, rpcParams);
        }
    }

    [ClientRpc]
    private void SyncPlayerStateClientRpc(
      int number,
      int rating,
      string icon,
      string nickname,
      ulong clientId,
      ClientRpcParams rpcParams = default)
    {
        bool amI = NetworkManager.Singleton.LocalClientId == clientId;
        if (amI)
        {
            // My own UI
            PlayerUI.Instance.SetMyNumber(number);
            PlayerUI.Instance.SetMyRating(rating);
            PlayerUI.Instance.SetMyProfileImage(icon);
            PlayerUI.Instance.SetMyNickname(nickname);
        }
        else
        {
            // Opponent's UI
            PlayerUI.Instance.SetOpponentNumber(number);
            PlayerUI.Instance.SetOpponentRating(rating);
            PlayerUI.Instance.SetOpponentIconImage(icon);
            PlayerUI.Instance.SetOpponentNickName(nickname);
        }
    }



    //unbind

    /// <summary>
    /// playerId → clientId 역추적 후 해당 매핑을 제거합니다.
    /// </summary>
    public void UnbindByPlayerId(int playerId)
    {
        // 1) playerId → clientId 역추적
        var entry = clientIdToPlayerId.FirstOrDefault(kv => kv.Value == playerId);
        if (!entry.Equals(default(KeyValuePair<ulong, int>)))
        {
            ulong oldCid = entry.Key;

            // 2) 모든 딕셔너리에서 oldCid 키 제거
            clientIdToPlayerId.Remove(oldCid);
            clientIdToRating.Remove(oldCid);
            clientIdToIcon.Remove(oldCid);
            clientIdToNickname.Remove(oldCid);

            Debug.Log($"[PlayerDataManager] Unbound all data for oldCid={oldCid}, playerId={playerId}");
        }
    }


    //세선 호출

    [ServerRpc(RequireOwnership = false)]
    public void SetClientInGameServerRpc(bool inGame, ServerRpcParams rpcParams = default)
    {
        ulong cid = rpcParams.Receive.SenderClientId;
        Debug.Log($"[PlayerDataManager] CID={cid} → inGame={inGame}");

        // playerId로 변환
        if (!clientIdToPlayerId.TryGetValue(cid, out var playerId))
            return;

        // 외부 API에도 즉시 업스트림
        if (managers != null)
        {
            StartCoroutine(
                managers.UpdatePlayerSessionCoroutine(
                    playerId,
                    inGame,
                    success => Debug.Log($"[API] playerSession({playerId}) inGame={inGame} → {(success ? "OK" : "FAIL")}")
                )
            );
        }
        else
        {
            Debug.LogWarning("[PlayerDataManager] Managers가 할당되지 않았습니다!");
        }
    }



}




