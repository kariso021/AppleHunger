using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPlayerName : MonoBehaviour
{
    public static EditPlayerName Instance { get; private set; }
    public event EventHandler OnNameChanged;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private InputWindow inputWindow;    // �� �ν����Ϳ� ����
    [SerializeField] private Button editNameButton; // ��ư ������Ʈ

    private string playerName = "Apple Player";

    private void Awake()
    {
        Instance = this;

        // ��ư �Ҵ�
        editNameButton.onClick.AddListener(() =>
        {
            inputWindow.Show(
                title: "Player Name",
                defaultText: playerName,
                validChars: "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ .,-",
                charLimit: 20,
                onCancel: () => { /* ��� �� �ƹ� �͵� �� �� */ },
                onSubmit: newName =>
                {
                    playerName = newName;
                    playerNameText.text = playerName;
                    OnNameChanged?.Invoke(this, EventArgs.Empty);
                }
            );
        });

        // �ʱ� �ؽ�Ʈ
        playerNameText.text = playerName;
    }

    private void Start()
    {
        OnNameChanged += EditPlayerName_OnNameChanged;
    }

    private void EditPlayerName_OnNameChanged(object sender, EventArgs e)
    {
        // LobbyManage �ʿ� �÷��̾� �̸� ������Ʈ ��û
        LobbyManage.Instance.UpdatePlayerName(playerName);
    }

    public string GetPlayerName() => playerName;
}
