using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EmotionUI : MonoBehaviour
{
    public static EmotionUI Instance { get; private set; }
    public GameObject emotionPanel;
    public EmotionUIHandler emotionUIHandler;

    [Header("Button Of Emtion")]
    public Button tauntButton;
    public Button laughButton;
    public Button clapButton;

    [Header("Emotion Play Panel")]
    public GameObject player_ShowEmotionPanel;
    public Image player_ShowEmotionImage;
    public GameObject opponent_ShowEmotionPanel;
    public Image opponent_ShowEmotionImage;



    private bool isPanelOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        emotionPanel.SetActive(false);
        player_ShowEmotionPanel.SetActive(false);
        opponent_ShowEmotionPanel.SetActive(false);
        tauntButton.onClick.AddListener(() => OnEmotionClicked(EmtionType.Taunt));
        laughButton.onClick.AddListener(() => OnEmotionClicked(EmtionType.Laugh));
        clapButton.onClick.AddListener(() => OnEmotionClicked(EmtionType.Clap));
    }

    public void ToggleEmotionPanel()
    {
        isPanelOpen = !isPanelOpen;
        emotionPanel.SetActive(isPanelOpen);
    }

    private void OnEmotionClicked(EmtionType emotion)
    {
        emotionUIHandler.PlayEmotion(emotion);
        ClosePanel();
    }

    public void ClosePanel()
    {
        isPanelOpen = false;
        emotionPanel.SetActive(false);
    }

    public void ShowOpponentEmotion(EmtionType emotion)
    {
        string EmotionCode = GetStringFromEmotion(emotion);

        AddressableManager.Instance.LoadImageFromGroup(EmotionCode, opponent_ShowEmotionImage);
        opponent_ShowEmotionPanel.SetActive(true);
        StartCoroutine(HideOpponentAfterSeconds(1f));
    }

    private IEnumerator HideOpponentAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        opponent_ShowEmotionPanel.SetActive(false);
    }

    private string GetStringFromEmotion(EmtionType emotion) 
    {
        switch (emotion)
        {
            case EmtionType.Taunt: return "306";
            case EmtionType.Laugh: return "305";
            case EmtionType.Clap: return "303";
            default: return "301";
        }
    }
}