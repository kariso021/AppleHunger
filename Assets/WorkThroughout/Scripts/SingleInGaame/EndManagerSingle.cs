using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem.EnhancedTouch;

public class EndManagerSingle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endPanel;    // ������ �� ��� �ǳ�
    [SerializeField] private TextMeshProUGUI scoreText;         // ���� ���ھ ������ Text
    [SerializeField] private TMP_Text currentGoldText;         // ���� ���� ��差 ������ text
    [SerializeField] private TMP_Text deltaGoldText;           // ���ھ� ��ȭ��
    [SerializeField] private TMP_Text currentRatingText;              // ������ ��ȭ ǥ��(�̱ۿ��� ��ȭx, �׳� ux)
    [SerializeField] protected TMP_Text deltaRatingText;              // ��ȭ��(������ 0)
    [SerializeField] private Button lobbyButton;     // �κ�� ���ư��� ��ư
    [SerializeField] private GameObject myProfilePanel;

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

        if (EnhancedTouchSupport.enabled) // ���� ������ �ƿ� ���� �����ǿ� �������� ���ϵ��� ��ġ�� ���°�
        {
            Debug.Log("��ġ���ƹ����ݾ�");
            PlayerControllerSingle.Instance.RestrictTouchWhenGameEnded();

        }

        StartCoroutine(manager.UpdateCurrencyAndRating(player.playerId, gold, 0));

        scoreText.text = $"Score: {finalScore}";
        currentGoldText.text = player.currency.ToString();
        deltaGoldText.text = $"+{gold.ToString()}";
        currentRatingText.text = player.rating.ToString();
        deltaRatingText.text = "+0";
        endPanel.SetActive(true);

        myProfilePanel.SetActive(false);

        SQLiteManager.Instance.SavePlayerCurrency(totalGold);

        lobbyButton.onClick.RemoveAllListeners();
        lobbyButton.onClick.AddListener(GoToLobby);
    }

    private void GoToLobby()
    {
        SceneManager.LoadScene("TestLobby");
    }
}
