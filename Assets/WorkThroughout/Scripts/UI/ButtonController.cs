using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [Header("Home Panel Buttons")]
    public Button profileButton;
    public Button singlePlayButton;
    public Button multiPlayButton;
    [Header("Ranking Panel Buttons")]
    public Button rankProfileButton;

    [Header("Setting Panel Buttons")]
    public Button bgmOnOffButton; // 배경음
    public Button vfxOnOffButton; // 효과음
    public Button loginButton; // 일단 구글 로그인? or guest?
    public Button creditButton;

    [Header("Collection Panel Buttons")]
    public Button customIconButton;
    public Button customBoadrButton;
    public Button customIconConfrimButton;
    public Button customBoardConfrimButton;
    [Header("Navbar Buttons")]
    public Button homeButton;
    public Button rankingButton;
    public Button settingsButton;
    public Button collectionButton;

    [Header("Close Buttons")]
    public Button closeButton1;
    public Button closeButton2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Home Panel Buttons Binding
        profileButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));

        // 서버-클라 테스트 , 1은 임시 아이디
        profileButton.onClick.AddListener(() =>
        FindAnyObjectByType<ClientDatabaseManager>().GetPlayerData(1));
        // single,multi 플레이 관련 함수 바인딩

        // Ranking Panel Buttons
        rankProfileButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));
        // 후에 프로필 팝업의 각 텍스트마다 서버에서 정보를 받거나 혹은,
        // 이미 서버에서 각 랭크별로 인물의 정보를 담은 버튼을 생성하는 개념으로
        // 접근해야 하므로, 클라이언트에서 알아서 그 정보의 텍스트들을 기반으로 
        // 여는 식으로 해야할듯?

        // Setting Panel Buttons
        // bgm,vfx 는 후에 SoundManager에서 함수를 가져와 바인딩
        // login 도 구글 로그인 기능을 가져와 바인딩
        // 테스트 기능
        loginButton.onClick.AddListener(() =>
        FindAnyObjectByType<ClientDatabaseManager>().ChangePlayerDataTest(100));
        //
        creditButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.creditPopup));

        // Collection Panel Buttons
        customIconButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection/Icon"));
        customBoadrButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection/Board"));
        customIconConfrimButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection"));
        customBoardConfrimButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection"));

        // NavBar Buttons
        // Button 바인딩
        // Navbar Buttons (이제 PanelManager를 통해 패널 변경)
        homeButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Home"));
        rankingButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Ranking"));
        settingsButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Settings"));
        collectionButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Collection"));

        // Close Buttons
        closeButton1.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());
        closeButton2.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());
    }

    // Update is called once per frame
    void Update()
    {
            
    }


}
