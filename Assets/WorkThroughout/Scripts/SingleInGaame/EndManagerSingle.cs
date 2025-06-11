using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class EndManagerSingle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endPanel;    // ������ �� ��� �ǳ�
    [SerializeField] private TextMeshProUGUI scoreText;         // ���� ���ھ ������ Text
    [SerializeField] private Button lobbyButton;     // �κ�� ���ư��� ��ư

    [Header("Submit DB References")]
    [SerializeField] private Managers manager;
    private void Awake()
    {
        GameTimerSingle.OnGameEnded += HandleGameEnded;
    }

    private void Start()
    {
        endPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        GameTimerSingle.OnGameEnded -= HandleGameEnded;
    }

    private void HandleGameEnded()
    {
        var player = SQLiteManager.Instance.player;
        int finalScore = ScoreManagerSingle.Instance.GetScore();
        int gold = UnityEngine.Random.Range(50, 141);
        int totalGold = player.currency + gold;

        StartCoroutine(manager.UpdateCurrencyAndRating(player.playerId, gold, 0));

        scoreText.text = $"Score: {finalScore} \n" + $"Gold: {player.currency} �� {totalGold}  (+{gold})";
        endPanel.SetActive(true);

        SQLiteManager.Instance.SavePlayerCurrency(totalGold);

        lobbyButton.onClick.RemoveAllListeners();
        lobbyButton.onClick.AddListener(GoToLobby);
    }

    private void GoToLobby()
    {
        SceneManager.LoadScene("TestLobby");
    }
}
