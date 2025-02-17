using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score : " + score;
    }

    public void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true); // ���� ���� ȭ�� Ȱ��ȭ
    }
}