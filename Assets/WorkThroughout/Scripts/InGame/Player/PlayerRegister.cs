using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerRegister : NetworkBehaviour
{
    [Header("API 매니저 (Inspector에서 할당)")]
    public Managers managers;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        StartCoroutine(RegisterPlayer());
    }

    private IEnumerator RegisterPlayer()
    {
        // 1) PlayerDataManager 스폰 대기
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

        // 2) 로컬 세션 로드 & 재접속 모드 판단
        var session = SQLiteManager.Instance.LoadPlayerSession();
        if (session != null && session.isConnected)
        {
            Debug.Log("[PlayerRegister] 재접속 모드 → 초기 등록 스킵");
            yield break;
        }

        // 3) 신규 매칭 모드: DB 에서 플레이어 정보 가져오기
        int playerId = 1;
        int rating = 1000;
        string iconKey = "101";
        string nickName = "Player";

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
        }

        try
        {
            // 4) 서버에 프로필/번호/레이팅/닉네임 등록
            PlayerDataManager.Instance.RegisterPlayerNumberServerRpc(playerId);
            PlayerDataManager.Instance.RegisterPlayerRatingServerRpc(rating);
            PlayerDataManager.Instance.RegisterPlayerNickNameServerRpc(nickName);
            PlayerDataManager.Instance.RegisterPlayerProfileServerRpc(iconKey);

            // 5) 점수 초기화 (clientId 기준)
            ScoreManager.Instance.RequestAddScoreServerRpc(
                NetworkManager.Singleton.LocalClientId, 0, 0);

            Debug.Log($"✅ Player 등록 완료 - ID:{playerId}, Rating:{rating}, Icon:{iconKey}");

            // 6) 서버에 준비 완료 알림
            PlayerDataManager.Instance.NotifyPlayerReadyServerRpc();


            // 게임에 처음 등장하는것을 알리는 rpc 호출
            PlayerDataManager.Instance.SetClientInGameServerRpc(true);

            // ============================================
            // 7) 로컬 세션에 isConnected = true 저장
            session = session ?? new PlayerSessionData { playerId = playerId };
            session.isConnected = true;
            SQLiteManager.Instance.SavePlayerSession(session);

            // 8) API 서버에 isInGame = true 로 업데이트 (코루틴)
            if (managers != null)
            {
                StartCoroutine(
                    managers.UpdatePlayerSessionCoroutine(
                        playerId,
                        true,
                        success => Debug.Log("[PlayerRegister] playerSession 업데이트 " +
                                            (success ? "성공" : "실패"))
                    )
                );
            }
            else
            {
                Debug.LogWarning("Managers 참조가 할당되지 않았습니다!");
            }
            // ============================================
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 등록 중 예외 발생: {ex}");
        }
    }
}
