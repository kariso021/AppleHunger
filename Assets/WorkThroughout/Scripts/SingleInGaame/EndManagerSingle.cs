using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem.EnhancedTouch;

public class EndManagerSingle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endPanel;    // 끝났을 때 띄울 판넬
    [SerializeField] private TextMeshProUGUI scoreText;         // 최종 스코어를 보여줄 Text
    [SerializeField] private TMP_Text currentGoldText;         // 현재 가진 골드량 보여줄 text
    [SerializeField] private TMP_Text deltaGoldText;           // 스코어 변화량
    [SerializeField] private TMP_Text currentRatingText;              // 레이팅 변화 표시(싱글에선 변화x, 그냥 ux)
    [SerializeField] protected TMP_Text deltaRatingText;              // 변화량(실제론 0)
    [SerializeField] private Button lobbyButton;     // 로비로 돌아가기 버튼
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

        if (EnhancedTouchSupport.enabled) // 게임 끝나면 아예 게임 보드판에 관여하지 못하도록 터치를 막는거
        {
            Debug.Log("터치막아버리잖아");
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
