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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Home Panel Buttons Binding
        profileButton.onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(PopupManager.Instance.profilePopup));

        // ����-Ŭ�� �׽�Ʈ , 1�� �ӽ� ���̵�
        profileButton.onClick.AddListener(() =>
        FindAnyObjectByType<ClientDatabaseManager>().GetPlayerData(1));
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
        // �׽�Ʈ ���
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
    }

    // Update is called once per frame
    void Update()
    {
            
    }


}
