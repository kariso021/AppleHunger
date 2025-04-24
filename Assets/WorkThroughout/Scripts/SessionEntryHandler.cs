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
        // 1) ���� ���� �ε�
        var session = SQLiteManager.Instance.LoadPlayerSession();
        if (session == null)
        {
            Debug.Log("[SessionEntry] ���ÿ� ���� ���� �� �� ���� ���� �� ���� ���");

            // 1-1) �� ���� ��ü ���� (isInGame �⺻ false)
            session = new PlayerSessionData
            {
                playerId = SQLiteManager.Instance.player.playerId,
                isInGame = false
            };
            SQLiteManager.Instance.SavePlayerSession(session);

            // 1-2) ������ Upsert ��û
            yield return StartCoroutine(
                managers.UpdatePlayerSessionCoroutine(
                    session.playerId,
                    session.isInGame
                )
            );

            // ù ����̴� �κ� ����
            yield break;
        }

        // 2) �������� isInGame ��ȸ
        bool isInGame = false;

   
        yield return StartCoroutine(
            managers.GetIsInGameCoroutine(
                session.playerId,
                result => isInGame = result
            )
        );

        // 3) ���°� �������� ���á����� ��� ����
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

        // 4) In-Game ���¸� �� ��ȯ, �ƴϸ� �κ� ���� �׸��� isConnected �κ� false �� �����
        if (isInGame)
        {
            Debug.Log("[SessionEntry] In-Game ���� �� InGame �� �ε�");
            SceneManager.LoadScene(inGameSceneName);
        }
        else
        {
            Debug.Log("[SessionEntry] In-Game �ƴ� �� �κ� ����");
            //���õ������� inconnceted �κ� false�� �ٲ������
            SQLiteManager.Instance.playerSession.isConnected = false;
            SQLiteManager.Instance.SavePlayerSession(
                SQLiteManager.Instance.playerSession);
        }
    }
}
