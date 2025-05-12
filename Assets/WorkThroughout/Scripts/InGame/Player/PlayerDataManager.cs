using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 서버-클라이언트를 가로질러 플레이어 상태(번호, 점수, 프로필 등)를 관리합니다.
/// 하나의 Dictionary를 playerId 기준으로 사용하며, ClientId 값만 업데이트합니다.
/// </summary>
public class PlayerDataManager : NetworkBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    
    [SerializeField] private float panelDuration = 3f; // 패널 표시 시간

    [Serializable]
    private class PlayerState
    {
        public ulong ClientId;   // 현재 연결된 Netcode clientId (0: 오프라인)
        public int Rating;       // 플레이어 점수/랭킹
        public string IconKey;   // 프로필 아이콘 식별자
        public string Nickname;  // 플레이어 닉네임
    }

    [Tooltip("API 호출용 Managers 할당")]
    [SerializeField]
    private Managers managers;

    // playerId → PlayerState
    private readonly Dictionary<int, PlayerState> _statesByPlayer = new Dictionary<int, PlayerState>();
    // 준비 완료된 playerId 집합
    private readonly HashSet<int> _readyPlayers = new HashSet<int>();

    //임시버전임
    private readonly HashSet<ulong> _readyClients = new HashSet<ulong>();

    private const int ExpectedPlayerCount = 2;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        base.OnNetworkDespawn();
    }

    #region Public API

    /// <summary>
    /// clientId를 사용해 PlayerId와 상태를 조회
    /// </summary>
    public bool TryGetStateByClient(ulong clientId, out int playerId, out int rating, out string iconKey, out string nickname)
    {
        foreach (var kv in _statesByPlayer)
        {
            if (kv.Value.ClientId == clientId)
            {
                playerId = kv.Key;
                rating = kv.Value.Rating;
                iconKey = kv.Value.IconKey;
                nickname = kv.Value.Nickname;
                return true;
            }
        }
        playerId = -1;
        rating = 0;
        iconKey = string.Empty;
        nickname = string.Empty;
        return false;
    }

    /// <summary>
    /// 모든 playerId → clientId 맵 반환
    /// </summary>
    public Dictionary<int, ulong> GetAllClientMappings()
    {
        var map = new Dictionary<int, ulong>();
        foreach (var kv in _statesByPlayer)
            map[kv.Key] = kv.Value.ClientId;
        return map;
    }

    #endregion

    #region Server RPCs - Registration

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerIDServerRpc(int playerId, ServerRpcParams rpc = default)
    {
        var cid = rpc.Receive.SenderClientId;
        if (!_statesByPlayer.TryGetValue(playerId, out var state))
        {
            state = new PlayerState();
            _statesByPlayer[playerId] = state;
        }
        state.ClientId = cid;
        // API: inGame = true
        Debug.Log($"[PDM] Player {playerId} connected (client {cid})");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerRatingServerRpc(int rating, ServerRpcParams rpc = default)
    {
        if (TryGetStateByClient(rpc.Receive.SenderClientId, out var pid, out _, out _, out _))
        {
            _statesByPlayer[pid].Rating = rating;
            Debug.Log($"[PDM] Player {pid} rating updated to {rating}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerIconServerRpc(string iconKey, ServerRpcParams rpc = default)
    {
        if (TryGetStateByClient(rpc.Receive.SenderClientId, out var pid, out _, out _, out _))
        {
            _statesByPlayer[pid].IconKey = iconKey;
            Debug.Log($"[PDM] Player {pid} icon updated: {iconKey}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerNicknameServerRpc(string nickname, ServerRpcParams rpc = default)
    {
        if (TryGetStateByClient(rpc.Receive.SenderClientId, out var pid, out _, out _, out _))
        {
            _statesByPlayer[pid].Nickname = nickname;
            Debug.Log($"[PDM] Player {pid} nickname updated: {nickname}");
        }
    }

    #endregion

    #region Ready & Sync

    [ServerRpc(RequireOwnership = false)]
    public void NotifyPlayerReadyServerRpc(bool isReconnect, ServerRpcParams rpc = default)
    {
        var cid = rpc.Receive.SenderClientId;

        // clientId 기준으로 준비 등록
        if (!isReconnect)
        {
            if (_readyClients.Add(cid))
                Debug.Log($"[PDM] ✅ client {cid} is ready (total: {_readyClients.Count})");

            if (_readyClients.Count == ExpectedPlayerCount)
            {
                //준비된 기준 이때부터 타이머 적용시켜야함. 그리고 패널을 띄워줘야함.
                Debug.Log("[PDM] All clients ready, syncing...");
                SyncAllClients();

                // 1) 상태 동기화
                SyncAllClients();

                //// 2) 매칭 패널 띄우기
                //ShowMatchPanelClientRpc(panelDuration);

                //// 3) 타이머는 여기서 한 번만 호출
                //GameTimer.Instance.StartTimerWithDelayServerRpc(panelDuration);

                _readyClients.Clear();
            }
        }
    }




    private void SyncAllClients()
    {
        foreach (var kv in _statesByPlayer)
        {
            var pid = kv.Key;
            var st = kv.Value;

            // rpcParams 없이 호출하면 모든 클라이언트에게 전송됩니다.
            UpdatePlayerStateClientRpc(
                pid,
                st.ClientId,
                st.Rating,
                st.IconKey,
                st.Nickname
            );
        }
    }

    [ClientRpc]
    private void ShowMatchPanelClientRpc(float panelDuration)
    {
        PlayerUI.Instance.OnMatchFoundShowPanel(panelDuration);
    }

    [ClientRpc]
    private void UpdatePlayerStateClientRpc(
        int playerId,
        ulong clientId,
        int rating,
        string iconKey,
        string nickname,
        ClientRpcParams rpcParams = default)
    {
        bool isMe = NetworkManager.Singleton.LocalClientId == clientId;
        if (isMe)
        {
            Debug.Log("내 UI 업데이트");
            PlayerUI.Instance.SetMyRating(rating);
            PlayerUI.Instance.SetMyProfileImage(iconKey);
            PlayerUI.Instance.SetMyNickname(nickname);
        }
        else
        {
            Debug.Log("상대방 UI 업데이트");
            PlayerUI.Instance.SetOpponentRating(rating);
            PlayerUI.Instance.SetOpponentIconImage(iconKey);
            PlayerUI.Instance.SetOpponentNickName(nickname);
        }
    }

    #endregion

    #region Reconnect & Disconnect

    //나중에 UI 띄워주는 용도로 만들자.
    private void HandleClientDisconnect(ulong clientId)
    {
        //foreach (var kv in _statesByPlayer)
        //{
        //    if (kv.Value.ClientId == clientId)
        //    {
        //        kv.Value.ClientId = 0;
        //        break;
        //    }
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestReconnectServerRpc(int playerId, ServerRpcParams rpc = default)
    {
        var newCid = rpc.Receive.SenderClientId;
        if (_statesByPlayer.TryGetValue(playerId, out var state))
        {
            state.ClientId = newCid;
            Debug.Log($"[PDM] Player {playerId} reconnected (client {newCid})");
        }

        //여기에 점수 동기화 먼저
        ScoreManager.Instance.SyncAllScoresToClient(newCid);

        SyncAllClients();

    }


    #endregion

    #region Session API



    public IEnumerator UpdateAllSessionsFalse()
    {
        foreach (var pid in _statesByPlayer.Keys)
        {
            Debug.Log($"[PDM] Update session for player {pid} to false");
            yield return StartCoroutine(managers.UpdatePlayerSessionCoroutine(pid, false));
        }
    }

    #endregion

    #region Emotion RPCs

    [ServerRpc(RequireOwnership = false)]
    public void SendEmotionServerRpc(EmtionType emotion, ServerRpcParams rpc = default)
    {
        var sender = rpc.Receive.SenderClientId;
        SendEmotionClientRpc(emotion, sender);
    }

    [ClientRpc]
    private void SendEmotionClientRpc(EmtionType emotion, ulong senderClientId, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;
        EmotionUI.Instance.ShowOpponentEmotion(emotion);
    }

    #endregion

    public int GetPlayerRating(int playerId)
    {
        if (_statesByPlayer.TryGetValue(playerId, out var state))
            return state.Rating;
        return 0;
    }
}