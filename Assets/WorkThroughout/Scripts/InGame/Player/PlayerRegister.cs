using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

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
            // 서버에 프로필 등록
            PlayerDataManager.Instance.RegisterPlayerNumberServerRpc(playerId);
            PlayerDataManager.Instance.RegisterPlayerRatingServerRpc(rating);
            PlayerDataManager.Instance.RegisterPlayerNickNameServerRpc(nickName);
            PlayerDataManager.Instance.RegisterPlayerProfileServerRpc(iconKey);

            // 점수 초기화: clientId 기준으로 호출
            ScoreManager.Instance.RequestAddScoreServerRpc(
                NetworkManager.Singleton.LocalClientId, 0, 0);

            Debug.Log($"✅ Player 등록 완료 - ID:{playerId}, Rating:{rating}, Icon:{iconKey}");
            PlayerDataManager.Instance.NotifyPlayerReadyServerRpc();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 등록 중 예외 발생: {ex}");
        }
    }
}
