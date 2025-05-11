using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.Rendering;

public class PlayerRegister : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        StartCoroutine(RegisterPlayer());
    }

    private IEnumerator RegisterPlayer()
    {
        // 1) PlayerDataManager 준비 대기
        float timeout = 5f, timer = 0f;
        while ((PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsSpawned) && timer < timeout)
        {
            Debug.Log("⏳ PlayerDataManager 로딩 대기 중...");
            timer += Time.deltaTime;
            yield return null;
        }
        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsSpawned)
        {
            Debug.LogError("PlayerDataManager 등록 실패");
            yield break;
        }

        // 2) 로컬 세션 로드
        var session = SQLiteManager.Instance.LoadPlayerSession();

        Debug.Log("isconnected 여부와 isIngame여부");
        Debug.Log(session.isConnected);
        Debug.Log(session.isInGame);
        //여기서 false로 나옴
        bool isReconnect = SQLiteManager.Instance.playerSession.isConnected;
        int playerId;
        int rating;
        string iconKey;
        string nickName;

        if (isReconnect)
        {
            Debug.Log("재접속 모드 Register에서 실행중");
            // 재접속: DB에 남은 playerId만 사용
            playerId = session.playerId;
            PlayerDataManager.Instance.RequestReconnectServerRpc(playerId);

            yield return null;
        }
        else
        {
            // 신규 접속: SQLite에서 정보 읽어오거나 기본값 사용
            if (SQLiteManager.Instance?.player != null)
            {
                playerId = SQLiteManager.Instance.player.playerId;
                rating = SQLiteManager.Instance.player.rating;
                iconKey = SQLiteManager.Instance.player.profileIcon;
                nickName = SQLiteManager.Instance.player.playerName;
            }
            else
            {
                Debug.LogWarning("SQLiteManager null → 기본값으로 등록 진행");
                playerId = 1;
                rating = 1000;
                iconKey = "101";
                nickName = "Player";
            }

            // 3) 서버에 신규 프로필/번호/레이팅/닉네임 등록
            PlayerDataManager.Instance.RegisterPlayerIDServerRpc(playerId);
            PlayerDataManager.Instance.RegisterPlayerRatingServerRpc(rating);
            PlayerDataManager.Instance.RegisterPlayerNicknameServerRpc(nickName);
            PlayerDataManager.Instance.RegisterPlayerIconServerRpc(iconKey);

            // 4) 점수 초기화
            ScoreManager.Instance.RequestAddScoreServerRpc(playerId, 0, 0);

            Debug.Log($"✅ 신규 Player 등록 완료 - ID:{playerId}, Rating:{rating}, Icon:{iconKey}");
        }

        // 5) 재접속 또는 신규 모두에서 준비 완료 알림
        PlayerDataManager.Instance.NotifyPlayerReadyServerRpc(isReconnect);


        //IsConnected 
        var playersession = SQLiteManager.Instance.playerSession;
        playersession.isConnected = true;
        SQLiteManager.Instance.SavePlayerSession(playersession);
        var playersessiontemp= SQLiteManager.Instance.LoadPlayerSession();
        Debug.Log(playersessiontemp.isConnected);

    }
}