using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionEntryHandler : MonoBehaviour
{
    [SerializeField] private string inGameSceneName = "InGame";
    [SerializeField] private Managers managers;

    private void Start()
    {
        StartCoroutine(CheckAndEnterSession());
    }

    private IEnumerator CheckAndEnterSession()
    {
        // 1) 로컬 세션 로드
        var session = SQLiteManager.Instance.LoadPlayerSession();

        //// 1-1) 세션이 없으면 새로 생성
        if (session == null)
        {
            Debug.Log("[SessionEntry] 로컬 세션 없음 → 새 세션 생성 및 서버 등록");
            session = new PlayerSessionData
            {
                playerId = SQLiteManager.Instance.player.playerId,
                isInGame = false,
                isConnected = false
            };
            SQLiteManager.Instance.SavePlayerSession(session);

            yield break;
        }

        // 2) 서버에서 isInGame 조회
        bool isInGame = false;
        yield return StartCoroutine(
            managers.GetIsInGameCoroutine(
                session.playerId,
                result => isInGame = result
            )
        );

        // 3) 로컬에 연결 상태 반영
        session.isInGame = isInGame;
        if(isInGame == true)
        {
            session.isConnected = true;
            SQLiteManager.Instance.SavePlayerSession(session);
        }
        else
        {
            session.isConnected = false;
            SQLiteManager.Instance.SavePlayerSession(session);
        }
        SQLiteManager.Instance.SavePlayerSession(session);
        Debug.Log($"[SessionEntry] isIngame: {session.isInGame}");

        // 4) InGame 씬 전환 또는 로비 유지
        if (isInGame)
        {
            Debug.Log("[SessionEntry] In-Game 상태 → 씬 전환");
            session.isConnected = true;
            SQLiteManager.Instance.SavePlayerSession(session);
            SceneManager.LoadScene(inGameSceneName);
        }
        else
        {
            session.isConnected = false;
            SQLiteManager.Instance.SavePlayerSession(session);
            Debug.Log("[SessionEntry] 로비 유지");
        }
    }
}