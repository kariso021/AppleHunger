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
    [SerializeField] private InputWindow inputWindow;    // ← 인스펙터에 연결
    [SerializeField] private Button editNameButton; // 버튼 컴포넌트

    private string playerName = "Apple Player";

    private void Awake()
    {
        Instance = this;

        // 버튼 할당
        editNameButton.onClick.AddListener(() =>
        {
            inputWindow.Show(
                title: "Player Name",
                defaultText: playerName,
                validChars: "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ .,-",
                charLimit: 20,
                onCancel: () => { /* 취소 시 아무 것도 안 함 */ },
                onSubmit: newName =>
                {
                    playerName = newName;
                    playerNameText.text = playerName;
                    OnNameChanged?.Invoke(this, EventArgs.Empty);
                }
            );
        });

        // 초기 텍스트
        playerNameText.text = playerName;
    }

    private void Start()
    {
        OnNameChanged += EditPlayerName_OnNameChanged;
    }

    private void EditPlayerName_OnNameChanged(object sender, EventArgs e)
    {
        // LobbyManage 쪽에 플레이어 이름 업데이트 요청
        LobbyManage.Instance.UpdatePlayerName(playerName);
    }

    public string GetPlayerName() => playerName;
}
