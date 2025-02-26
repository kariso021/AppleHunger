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
    public Button closeButton1;
    public Button closeButton2;

    [Header("Debug Buttons")]
    public Button playerAdd;
    public Button playerGet;
    public Button playerPut;
    public Button playerStatsGet;
    public Button playerItemsGet;
    public Button playerItemsUnlock;
    public Button matchAdd;
    public Button loginGet;
    public Button loginPut;
    public Button rankingGet;
    public Button rankingGetFromId;

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
        //profileButton.onClick.AddListener(() =>
        //PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));
        // single,multi �÷��� ���� �Լ� ���ε�

        // Ranking Panel Buttons
        rankProfileButton.onClick.AddListener(() =>
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
        customBoardConfrimButton.onClick.AddListener(() =>
        NavManager.Instance.NavigateTo("Collection"));

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
        closeButton1.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());
        closeButton2.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());



        // Debug Buttons
        // �׽�Ʈ ���
        playerAdd.onClick.AddListener(() =>
        clientNetworkManager.AddPlayer(Random.Range(12345, 99999).ToString()));

        playerGet.onClick.AddListener(() =>
        clientNetworkManager.GetPlayerData(ClientDataManager.Instance.playerData.playerId));

        playerPut.onClick.AddListener(() =>
        clientNetworkManager.UpdatePlayerData());

        playerStatsGet.onClick.AddListener(() =>
        clientNetworkManager.GetPlayerStats(ClientDataManager.Instance.playerData.playerId));

        playerItemsGet.onClick.AddListener(() =>
        clientNetworkManager.GetPlayerItems(ClientDataManager.Instance.playerData.playerId));

        playerItemsUnlock.onClick.AddListener(() =>
        clientNetworkManager.UnlockPlayerItems(ClientDataManager.Instance.playerData.playerId,102));

        matchAdd.onClick.AddListener(() =>
        clientNetworkManager.AddMatchRecords(ClientDataManager.Instance.playerData.playerId,
        ClientDataManager.Instance.playerData.playerId + 1));

        loginGet.onClick.AddListener(() =>
        clientNetworkManager.GetLogin(ClientDataManager.Instance.playerData.playerId));

        loginPut.onClick.AddListener(() =>
        clientNetworkManager.UpdateLogin(ClientDataManager.Instance.playerData.playerId));

        rankingGet.onClick.AddListener(() =>
        clientNetworkManager.GetRankingList(ClientDataManager.Instance.playerData.playerId));

        rankingGetFromId.onClick.AddListener(() =>
        clientNetworkManager.GetRanking(ClientDataManager.Instance.playerData.playerId));
    }

    // Update is called once per frame
    void Update()
    {
            
    }


}
