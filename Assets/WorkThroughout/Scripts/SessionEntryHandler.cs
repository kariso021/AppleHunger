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

        //// 1-1) ������ ������ ���� ����
        if (session == null)
        {
            Debug.Log("[SessionEntry] ���� ���� ���� �� �� ���� ���� �� ���� ���");
            session = new PlayerSessionData
            {
                playerId = SQLiteManager.Instance.player.playerId,
                isInGame = false,
                isConnected = false
            };
            SQLiteManager.Instance.SavePlayerSession(session);

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

        // 3) ���ÿ� ���� ���� �ݿ�
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

        // 4) InGame �� ��ȯ �Ǵ� �κ� ����
        if (isInGame)
        {
            Debug.Log("[SessionEntry] In-Game ���� �� �� ��ȯ");
            session.isConnected = true;
            SQLiteManager.Instance.SavePlayerSession(session);
            SceneManager.LoadScene(inGameSceneName);
        }
        else
        {
            session.isConnected = false;
            SQLiteManager.Instance.SavePlayerSession(session);
            Debug.Log("[SessionEntry] �κ� ����");
        }
    }
}