using JetBrains.Annotations;
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
    public Button collectionButton;

    [Header("Close Buttons")]
    public Button creditCloseButton;
    [Header("Popup Buttons")]
    public Button nameChangeButton;

    // Multiplay Mode ��ư
    [Header("MuliplyMode button")]
    public GameObject multiplaymodePanel;
    public Button RelaymultiplayModeButton;
    public Button MatchmakerplayModeButton;
    public Button GameModePanelCloseButton;

    private ClientNetworkManager clientNetworkManager;

    public Managers testManager;

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


        multiPlayButton.onClick.AddListener(() => LoadGameModePopup());
        RelaymultiplayModeButton.onClick.AddListener(() => LoadRelayModeGameScene());
        MatchmakerplayModeButton.onClick.AddListener(() => LoadMatchmakerGameScene());
        GameModePanelCloseButton.onClick.AddListener(() =>
        {
            multiplaymodePanel.SetActive(false);
        });






        singlePlayButton.onClick.AddListener(() => LoadSingleGameScene());

        // Ranking Panel Buttons
        myRankProfileButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));


        // �Ŀ� ������ �˾��� �� �ؽ�Ʈ���� �������� ������ �ްų� Ȥ��,
        // �̹� �������� �� ��ũ���� �ι��� ������ ���� ��ư�� �����ϴ� ��������
        // �����ؾ� �ϹǷ�, Ŭ���̾�Ʈ���� �˾Ƽ� �� ������ �ؽ�Ʈ���� ������� 
        // ���� ������ �ؾ��ҵ�?



     // Collection Panel Buttons
        //customIconButton.onClick.AddListener(() =>
        //NavManager.Instance.NavigateTo("Collection/Icon"));
        //customBoadrButton.onClick.AddListener(() =>
        //NavManager.Instance.NavigateTo("Collection/Board"));

        customIconConfrimButton.onClick.AddListener(() => {
            // Nav
            NavManager.Instance.NavigateTo("Collection");
            // Popup
            PopupManager.Instance.ShowLoading("����");
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

        //customBoardConfrimButton.onClick.AddListener(() =>
        //{
        //    // Nav
        //    NavManager.Instance.NavigateTo("Collection");
        //    // Popup
        //    PopupManager.Instance.ShowPopup(PopupManager.Instance.loadingPopup);
        //    //Invoke(nameof(PopupManager.Instance.HideLoading), 1.0f);
        //    // Local Data Change
        //    SQLiteManager.Instance.player.boardImage =
        //    FindAnyObjectByType<ItemManager>().currentItemBoard.
        //    GetComponent<ItemData>().itemUniqueId.ToString();

        //    // Server Data Change
        //    StartCoroutine(clientNetworkManager.UpdatePlayerData());
        //});

        // NavBar Buttons
        // Button ���ε�
        // Navbar Buttons (���� PanelManager�� ���� �г� ����)
        homeButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Home"));
        rankingButton.onClick.AddListener(() => 
        NavManager.Instance.NavigateTo("Ranking"));
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
    }

    private void LoadSingleGameScene()
    {
        SceneManager.LoadScene("TestInGame");
    }

    private void LoadGameModePopup()
    {
        multiplaymodePanel.SetActive(true);
    }

    private void LoadRelayModeGameScene()
    {
        SceneManager.LoadScene("RelayIngame");
    }

    private void LoadMatchmakerGameScene()
    {
        SceneManager.LoadScene("InGame");
    }

}
