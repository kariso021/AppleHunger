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
        // 1) ���� ���� ���� �ε�
        var session = SQLiteManager.Instance.LoadPlayerSession();
        if (session == null)
        {
            Debug.LogWarning("[SessionEntry] PlayerSession ���� �� �κ� ����");
            yield break;
        }

        // 2) ������ isInGame ���� ��û (�ڷ�ƾ)
        bool isInGame = false;
        yield return StartCoroutine(
            managers.GetIsInGameCoroutine(
                session.playerId,
                result => isInGame = result
            )
        );

        // 3) ����� ���� �� ��ȯ
        if (isInGame)
        {
            Debug.Log("[SessionEntry] �̹� In-Game ���� �� InGame �� �ε�");
            SceneManager.LoadScene(inGameSceneName);
        }
        else
        {
            Debug.Log("[SessionEntry] In-Game ���� �ƴ� �� �κ� ����");
        }
    }
}