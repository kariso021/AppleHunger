using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionEntryHandler : MonoBehaviour
{
    [SerializeField] private string inGameSceneName = "IngameS";
    [SerializeField] private Managers managers;

    private void Start()
    {
        StartCoroutine(CheckAndEnterSession());
    }

    private IEnumerator CheckAndEnterSession()
    {
        // 1) 로컬 세션 정보 로드
        var session = SQLiteManager.Instance.LoadPlayerSession();
        if (session == null)
        {
            Debug.LogWarning("[SessionEntry] PlayerSession 없음 → 로비 유지");
            yield break;
        }

        // 2) 서버에 isInGame 여부 요청 (코루틴)
        bool isInGame = false;
        yield return StartCoroutine(
            managers.GetIsInGameCoroutine(
                session.playerId,
                result => isInGame = result
            )
        );

        // 3) 결과에 따라 씬 전환
        if (isInGame)
        {
            Debug.Log("[SessionEntry] 이미 In-Game 상태 → InGame 씬 로드");
            SceneManager.LoadScene(inGameSceneName);
        }
        else
        {
            Debug.Log("[SessionEntry] In-Game 상태 아님 → 로비 유지");
        }
    }
}