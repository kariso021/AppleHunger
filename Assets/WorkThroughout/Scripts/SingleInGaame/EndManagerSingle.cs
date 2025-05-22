using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndManagerSingle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endPanel;    // 끝났을 때 띄울 판넬
    [SerializeField] private TextMeshProUGUI scoreText;         // 최종 스코어를 보여줄 Text
    [SerializeField] private Button lobbyButton;     // 로비로 돌아가기 버튼

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
        int finalScore = ScoreManagerSingle.Instance.GetScore();

      
        scoreText.text = $"Score: {finalScore}";

        endPanel.SetActive(true);


        lobbyButton.onClick.RemoveAllListeners();
        lobbyButton.onClick.AddListener(GoToLobby);
    }

    private void GoToLobby()
    {
        SceneManager.LoadScene("TestLobby");
    }
}
