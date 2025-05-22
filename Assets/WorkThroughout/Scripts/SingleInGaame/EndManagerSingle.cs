using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndManagerSingle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endPanel;    // ������ �� ��� �ǳ�
    [SerializeField] private TextMeshProUGUI scoreText;         // ���� ���ھ ������ Text
    [SerializeField] private Button lobbyButton;     // �κ�� ���ư��� ��ư

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
