using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmotionUI : MonoBehaviour
{
    public static EmotionUI Instance { get; private set; }
    public GameObject emotionPanel;
    public EmotionUIHandler emotionUIHandler;



    [Header("Button Of Emtion")]
    public Button AngryButton;
    public Button LaughButton;
    public Button SadButton;


    [Header("Button Of Emotions Image")]// 버튼 아이콘 설정]
    [SerializeField] Image AngryButtonImage;
    [SerializeField] Image LaughButtonImage;
    [SerializeField] Image SadButtonImage;

    [Header("Emotion Play Panel")]
    public Button ExpendButton; // 패널 열기 버튼

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
        AngryButton.onClick.AddListener(() => OnEmotionClicked(EmotionType.Angry));
        LaughButton.onClick.AddListener(() => OnEmotionClicked(EmotionType.Laugh));
        SadButton.onClick.AddListener(() => OnEmotionClicked(EmotionType.Sad));


        ExpendButton.onClick.AddListener(ToggleEmotionPanel); // 패널 열기 버튼 이벤트 등록

        ApplyButtonIcons(); // 버튼 아이콘 적용
    }

    public void ToggleEmotionPanel()
    {
        isPanelOpen = !isPanelOpen;
        emotionPanel.SetActive(isPanelOpen);
    }

    private void OnEmotionClicked(EmotionType emotion)
    {
        emotionUIHandler.PlayEmotion(emotion);
        ClosePanel();
    }

    public void ClosePanel()
    {
        isPanelOpen = false;
        emotionPanel.SetActive(false);
    }

    public void ShowOpponentEmotion(string emotionCode)
    {
        AddressableManager.Instance.LoadImageFromGroup(emotionCode, opponent_ShowEmotionImage);
        opponent_ShowEmotionPanel.SetActive(true);
        StartCoroutine(HideOpponentAfterSeconds(1f));
    }

    private IEnumerator HideOpponentAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        opponent_ShowEmotionPanel.SetActive(false);
    }


    //Adressable로 이미지 적용시키는거
    private void ApplyButtonIcons()
    {
        string angryCode = GetStringFromEmotion(EmotionType.Angry);
        AddressableManager.Instance
            .LoadImageFromGroup(angryCode, AngryButtonImage.GetComponent<Image>());

        string laughCode = GetStringFromEmotion(EmotionType.Laugh);
        AddressableManager.Instance
            .LoadImageFromGroup(laughCode, LaughButtonImage.GetComponent<Image>());

 
        string sadCode = GetStringFromEmotion(EmotionType.Sad);
        AddressableManager.Instance
            .LoadImageFromGroup(sadCode, SadButtonImage.GetComponent<Image>());

        Debug.Log($"[Emotion] EM TEST _____ Character {SQLiteManager.Instance.player.profileIcon} , A = {angryCode} , L = {laughCode} , S = {sadCode}");
    }

    private string GetStringFromEmotion(EmotionType emotion) 
    {
            // 1) 프로필 아이콘 3번째 자리 추출
            string icon = SQLiteManager.Instance.player.profileIcon;
            string n = (icon != null && icon.Length >= 3)
                ? icon[2].ToString()
                : "0";

            int emotionNum;
            switch (emotion)
            {
                case EmotionType.Angry: emotionNum = 1; break;
                case EmotionType.Laugh: emotionNum = 2; break;
                case EmotionType.Sad: emotionNum = 3; break;
                default: emotionNum = 0; break;
            }

            // 최종조합
            return $"3{n}{emotionNum}";
    }

}