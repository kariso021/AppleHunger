using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{

    public static LobbyCreateUI Instance { get; private set; }

    [Header("Buttons & Text")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button lobbyNameButton;
    [SerializeField] private Button publicPrivateButton;
    [SerializeField] private Button maxPlayersButton;
    [SerializeField] private Button gameModeButton;

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI publicPrivateText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;
    [SerializeField] private TextMeshProUGUI gameModeText;

    [Header("Input Dialog")]
    [SerializeField] private InputWindow inputWindow;  // ← 인스펙터에 연결

    private string lobbyName;
    private bool isPrivate;
    private int maxPlayers;
    private LobbyManage.GameMode gameMode;

    private void Awake()
    {
        Instance = this;

        createButton.onClick.AddListener(() => {
            LobbyManage.Instance.CreateLobby(
                lobbyName,
                maxPlayers,
                isPrivate,
                gameMode
            );
            Hide();
        });

        // ? Lobby Name 입력
        lobbyNameButton.onClick.AddListener(() => {
            inputWindow.Show(
                title: "Lobby Name",
                defaultText: lobbyName,
                validChars: "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,-",
                charLimit: 20,
                onCancel: () => { /* 취소 시 그대로 */ },
                onSubmit: newName => {
                    lobbyName = newName;
                    UpdateText();
                }
            );
        });

        // ? Public / Private 토글
        publicPrivateButton.onClick.AddListener(() => {
            isPrivate = !isPrivate;
            UpdateText();
        });

        // ? Max Players 입력
        maxPlayersButton.onClick.AddListener(() => {
            inputWindow.Show(
                title: "Max Players",
                defaultText: maxPlayers.ToString(),
                validChars: "0123456789",
                charLimit: 2,
                onCancel: () => { },
                onSubmit: text => {
                    if (int.TryParse(text, out int n))
                    {
                        maxPlayers = n;
                        UpdateText();
                    }
                }
            );
        });

        // ? GameMode 순환
        gameModeButton.onClick.AddListener(() => {
            gameMode = (gameMode == LobbyManage.GameMode.CaptureTheFlag)
                ? LobbyManage.GameMode.Conquest
                : LobbyManage.GameMode.CaptureTheFlag;
            UpdateText();
        });

        Hide();
    }

    private void UpdateText()
    {
        lobbyNameText.text = lobbyName;
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        maxPlayersText.text = maxPlayers.ToString();
        gameModeText.text = gameMode.ToString();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        lobbyName = "MyLobby";
        isPrivate = false;
        maxPlayers = 4;
        gameMode = LobbyManage.GameMode.CaptureTheFlag;
        UpdateText();
    }
}
