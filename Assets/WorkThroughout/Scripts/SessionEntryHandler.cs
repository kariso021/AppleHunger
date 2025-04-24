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
        if (session == null)
        {
            Debug.Log("[SessionEntry] 로컬에 세션 없음 → 새 세션 생성 및 서버 등록");

            // 1-1) 새 세션 객체 생성 (isInGame 기본 false)
            session = new PlayerSessionData
            {
                playerId = SQLiteManager.Instance.player.playerId,
                isInGame = false
            };
            SQLiteManager.Instance.SavePlayerSession(session);

            // 1-2) 서버에 Upsert 요청
            yield return StartCoroutine(
                managers.UpdatePlayerSessionCoroutine(
                    session.playerId,
                    session.isInGame
                )
            );

            // 첫 등록이니 로비 유지
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

        // 3) 상태가 변했으면 로컬·서버 모두 갱신
        if (session.isInGame != isInGame)
        {
            session.isInGame = isInGame;
            SQLiteManager.Instance.SavePlayerSession(session);

            yield return StartCoroutine(
                managers.UpdatePlayerSessionCoroutine(
                    session.playerId,
                    session.isInGame
                )
            );
        }

        // 4) In-Game 상태면 씬 전환, 아니면 로비 유지 그리고 isConnected 부분 false 로 만들기
        if (isInGame)
        {
            Debug.Log("[SessionEntry] In-Game 상태 → InGame 씬 로드");
            SceneManager.LoadScene(inGameSceneName);
        }
        else
        {
            Debug.Log("[SessionEntry] In-Game 아님 → 로비 유지");
            //로컬데이터의 inconnceted 부분 false로 바꿔줘야함
            SQLiteManager.Instance.playerSession.isConnected = false;
            SQLiteManager.Instance.SavePlayerSession(
                SQLiteManager.Instance.playerSession);
        }
    }
}
