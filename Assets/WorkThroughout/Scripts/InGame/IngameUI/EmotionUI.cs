using UnityEngine;
using UnityEngine.UI;

public class EmotionUI : MonoBehaviour
{
    public GameObject emotionPanel;
    public EmotionUIHandler emotionUIHandler;

    [Header("Button Of Emtion")]
    public Button tauntButton;
    public Button laughButton;
    public Button clapButton;


    private bool isPanelOpen = false;

    private void Start()
    {

        emotionPanel.SetActive(false);
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
}