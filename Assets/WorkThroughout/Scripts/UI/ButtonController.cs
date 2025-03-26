using NUnit.Framework;
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
    public Button bgmOnOffButton; // �����
    public Button vfxOnOffButton; // ȿ����
    public Button loginButton; // �ϴ� ���� �α���? or guest?
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
    public Button profileCloseButton;
    public Button rankProfileCloseButton;
    public Button creditCloseButton;
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

    //�ӽ�
    private int id = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        clientNetworkManager = FindAnyObjectByType<ClientNetworkManager>();
    }
    void Start()
    {
        

        // Home Panel Buttons Binding , �ӽ÷� ����
        profileButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));
        // single,multi �÷��� ���� �Լ� ���ε�


        multiPlayButton.onClick.AddListener(() => LoadInGameScene());

        // Ranking Panel Buttons
        myRankProfileButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));


        // �Ŀ� ������ �˾��� �� �ؽ�Ʈ���� �������� ������ �ްų� Ȥ��,
        // �̹� �������� �� ��ũ���� �ι��� ������ ���� ��ư�� �����ϴ� ��������
        // �����ؾ� �ϹǷ�, Ŭ���̾�Ʈ���� �˾Ƽ� �� ������ �ؽ�Ʈ���� ������� 
        // ���� ������ �ؾ��ҵ�?

        // Setting Panel Buttons
        // bgm,vfx �� �Ŀ� SoundManager���� �Լ��� ������ ���ε�
        // login �� ���� �α��� ����� ������ ���ε�
        creditButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.creditPopup));

        // Collection Panel Buttons
        customIconButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection/Icon"));
        customBoadrButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection/Board"));

        customIconConfrimButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection"));

        customIconConfrimButton.onClick.AddListener(() =>
        SQLiteManager.Instance.player.profileIcon = 
        FindAnyObjectByType<ItemManager>().currentItemIcon.
        GetComponent<ItemData>().itemUniqueId.ToString());

        customIconConfrimButton.onClick.AddListener(() =>
        FindAnyObjectByType<RankingRecordsManager>().UpdateMyRankingRecords());

        customIconConfrimButton.onClick.AddListener(() =>
        clientNetworkManager.UpdatePlayerData());

        customBoardConfrimButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection"));

        customBoardConfrimButton.onClick.AddListener(() =>
        SQLiteManager.Instance.player.boardImage =
        FindAnyObjectByType<ItemManager>().currentItemBoard.
        GetComponent<ItemData>().itemUniqueId.ToString());

        customBoardConfrimButton.onClick.AddListener(() =>
        clientNetworkManager.UpdatePlayerData());

        // NavBar Buttons
        // Button ���ε�
        // Navbar Buttons (���� PanelManager�� ���� �г� ����)
        homeButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Home"));
        rankingButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Ranking"));
        settingsButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Settings"));
        collectionButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Collection"));

        // Close Buttons
        profileCloseButton.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());
        rankProfileCloseButton.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());
        creditCloseButton.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());



        // Debug Buttons
            // �׽�Ʈ ���
        playerAdd.onClick.AddListener(() =>
        clientNetworkManager.AddPlayer());

        playerGet.onClick.AddListener(() =>
        StartCoroutine(clientNetworkManager.GetPlayerData("deviceId",SQLiteManager.Instance.player.deviceId, false)));

        playerPut.onClick.AddListener(() =>
        StartCoroutine(clientNetworkManager.UpdatePlayerData()));

        playerStatsGet.onClick.AddListener(() =>
        StartCoroutine(clientNetworkManager.GetPlayerStats(SQLiteManager.Instance.player.playerId)));

        playerItemsGet.onClick.AddListener(() =>
        clientNetworkManager.GetPlayerItems(SQLiteManager.Instance.player.playerId));

        playerItemsPurchase.onClick.AddListener(() =>
        clientNetworkManager.PurchasePlayerItem(SQLiteManager.Instance.player.playerId,102));

        matchAdd.onClick.AddListener(() =>
        StartCoroutine(clientNetworkManager.AddMatchRecords(SQLiteManager.Instance.player.playerId,
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


    private void LoadInGameScene()
    {
        SceneManager.LoadScene("InGame");
    }


}
