using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [Header("Home Panel Buttons")]
    public Button profileButton;
    public Button singlePlayButton;
    public Button multiPlayButton;
    [Header("Ranking Panel Buttons")]

    public Button myRankProfileButton;
    [Header("Setting Panel Buttons")]
    public Button settingButton;
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
    public Button creditCloseButton;
    [Header("Popup Buttons")]
    public Button nameChangeButton;
    [Header("Debug Buttons")]
    public Button playerAdd;
    public Button playerGet;
    public Button playerPut;
    public Button playerStatsGet;
    public Button playerItemsGet;
    public Button playerItemsPurchase;
    public Button matchAdd;
    public Button matchGet;
    public Button loginGet;
    public Button loginPut;
    public Button rankingGet;
    public Button localGet;

    private ClientNetworkManager clientNetworkManager;

    public Managers testManager;

    //임시
    private int id = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        clientNetworkManager = FindAnyObjectByType<ClientNetworkManager>();
    }
    void Start()
    {
        

        // Home Panel Buttons Binding , 임시로 방지
        profileButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));
        // single,multi 플레이 관련 함수 바인딩


        multiPlayButton.onClick.AddListener(() => LoadInGameScene());

        singlePlayButton.onClick.AddListener(() => LoadSingleGameScene());

        // Ranking Panel Buttons
        myRankProfileButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));


        // 후에 프로필 팝업의 각 텍스트마다 서버에서 정보를 받거나 혹은,
        // 이미 서버에서 각 랭크별로 인물의 정보를 담은 버튼을 생성하는 개념으로
        // 접근해야 하므로, 클라이언트에서 알아서 그 정보의 텍스트들을 기반으로 
        // 여는 식으로 해야할듯?



     // Collection Panel Buttons
        customIconButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection/Icon"));
        customBoadrButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection/Board"));

        customIconConfrimButton.onClick.AddListener(() => {
            // Nav
            NavManager.Instance.NavigateTo("Collection");
            // Popup
            PopupManager.Instance.ShowPopup(PopupManager.Instance.loadingPopup);
            //PopupManager.Instance.Invoke(nameof(PopupManager.Instance.HideLoading), 1f);
            // Local Data Change
            SQLiteManager.Instance.player.profileIcon =
            FindAnyObjectByType<ItemManager>().currentItemIcon.
            GetComponent<ItemData>().itemUniqueId.ToString();
            // Update
            FindAnyObjectByType<RankingRecordsManager>().UpdateMyRankingRecords();
            // Server Data Change
            StartCoroutine(clientNetworkManager.UpdatePlayerData());

            });

        customBoardConfrimButton.onClick.AddListener(() =>
        {
            // Nav
            NavManager.Instance.NavigateTo("Collection");
            // Popup
            PopupManager.Instance.ShowPopup(PopupManager.Instance.loadingPopup);
            //Invoke(nameof(PopupManager.Instance.HideLoading), 1.0f);
            // Local Data Change
            SQLiteManager.Instance.player.boardImage =
            FindAnyObjectByType<ItemManager>().currentItemBoard.
            GetComponent<ItemData>().itemUniqueId.ToString();

            // Server Data Change
            StartCoroutine(clientNetworkManager.UpdatePlayerData());
        });

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

        creditCloseButton.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());


        // Popup Buttons
        nameChangeButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.nicknamePopup));
        settingButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.settingPopup));
        // Debug Buttons
        // 테스트 기능
        playerAdd.onClick.AddListener(() =>
        SceneManager.LoadScene("Down"));

        playerGet.onClick.AddListener(() =>
        StartCoroutine(clientNetworkManager.GetPlayerData("deviceId",SQLiteManager.Instance.player.deviceId, false)));

        playerPut.onClick.AddListener(() =>
        StartCoroutine(clientNetworkManager.UpdatePlayerData()));

        playerStatsGet.onClick.AddListener(() =>
        StartCoroutine(clientNetworkManager.GetPlayerStats(SQLiteManager.Instance.player.playerId)));

        playerItemsGet.onClick.AddListener(() =>
        clientNetworkManager.GetPlayerItems(SQLiteManager.Instance.player.playerId));

        //playerItemsPurchase.onClick.AddListener(() =>
        //clientNetworkManager.PurchasePlayerItem(SQLiteManager.Instance.player.playerId,102));

        matchAdd.onClick.AddListener(() =>
        StartCoroutine(testManager.AddMatchResult(SQLiteManager.Instance.player.playerId,
        4)));

        matchGet.onClick.AddListener(() =>
        clientNetworkManager.GetMatchRecords(SQLiteManager.Instance.player.playerId));

        loginGet.onClick.AddListener(() =>
        clientNetworkManager.GetLogin(SQLiteManager.Instance.player.playerId));

        loginPut.onClick.AddListener(() =>
        clientNetworkManager.UpdateLogin(SQLiteManager.Instance.player.playerId));

        rankingGet.onClick.AddListener(() =>
        clientNetworkManager.GetRankingList());

        localGet.onClick.AddListener(() =>
        SQLiteManager.Instance.LoadAllData());
    }

    private void LoadSingleGameScene()
    {
        SceneManager.LoadScene("TestInGame");
    }

    private void LoadInGameScene()
    {
        SceneManager.LoadScene("InGame");
    }


}
