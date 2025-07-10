using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("Popup Panels")]
    public GameObject creditPopup;
    public GameObject profilePopup;
    public GameObject rankProfilePopup;
    public GameObject nicknamePopup;
    public GameObject loadingPopup;
    public GameObject settingPopup;
    public GameObject warningPopup;
    public GameObject topPopup;

    public GameObject activePopup = null; // 현재 활성화된 팝업 저장
    private Action pendingOnComplete; // 콜백 저장
    private Action nickNameChangeEvent;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowPopup(GameObject popup)
    {
        // 만약 현재 활성화 된 팝업이 있다면?
        if (activePopup != null)
        {
            // 팝업을 먼저 닫아야 함
            ClosePopup();
        }

        activePopup = popup;
        activePopup.SetActive(true);
        //RectTransform rect = activePopup.GetComponent<RectTransform>();

        //rect.sizeDelta = new Vector2(rect.sizeDelta.x, Screen.height);

        if (popup.tag == "Profile")
        {
            Debug.Log("쇼 팝업 클라이언트 아이디");
            pendingOnComplete = () => OnPlayerDetailsLoaded();
            StartCoroutine(ClientNetworkManager.Instance.GetPlayerDetalis(SQLiteManager.Instance.player.playerId));
        }
        else if(popup.tag == "Setting")
        {

        }

    }
    public void ShowPopup(GameObject popup, int playerId)
    {
        Debug.Log("팝업실행");
        // 만약 현재 활성화 된 팝업이 있다면?
        if (activePopup != null)
        {
            // 팝업을 먼저 닫아야 함
            ClosePopup();
        }

        activePopup = popup;
        activePopup.SetActive(true);

        if (popup.tag == "Profile")
        {
            pendingOnComplete = () =>
            {
                OnPlayerDetailsLoaded();
                HideLoading();
            };
            StartCoroutine(ClientNetworkManager.Instance.GetPlayerDetalis(playerId));
        }

    }
    // 🔹 데이터가 다 로드된 후 실행될 메서드
    private void OnPlayerDetailsLoaded()
    {
        Debug.Log($"✅ PROFILE LOADED: {SQLiteManager.Instance.playerDetails.playerName} , {SQLiteManager.Instance.playerDetails.playerId}");

        // 자기 자신의 프로필을 열람할 때만 매치 기록을 불러오기
        if (activePopup != null)
        {
            if (activePopup.tag == "Profile") // 현재 활성화 된 팝업이 프로필 관련일때만
            {
                activePopup.GetComponentInChildren<ProfilePopup>().SetProfile(
                SQLiteManager.Instance.playerDetails.playerName,
                SQLiteManager.Instance.playerDetails.totalGames,
                SQLiteManager.Instance.playerDetails.wins,
                SQLiteManager.Instance.playerDetails.losses,
                SQLiteManager.Instance.playerDetails.winRate,
                SQLiteManager.Instance.playerDetails.rating,
                SQLiteManager.Instance.playerDetails.unlockIcons,
                SQLiteManager.Instance.playerDetails.unlockBoards
                );


                if (SQLiteManager.Instance.player.playerId == SQLiteManager.Instance.playerDetails.playerId) // 본인 프로필일 경우
                {
                    FindAnyObjectByType<MatchRecordsManager>().CreateMatchRecords();
                    if (GameObject.Find("UI_Canvas_Popup_RankProfile") == true) // 일단 임시 방편...
                    {
                        AddressableManager.Instance.LoadRankProfilePopupIconFromGroup();
                        Debug.Log("[PM] My Profile and Rank");
                    }
                    else
                    {
                        AddressableManager.Instance.LoadProfilePopupIconFromGroup();
                        Debug.Log("[PM] My Profile only");
                    }
                }
                else
                { // 본인 프로필이 아닐 경우
                    AddressableManager.Instance.LoadRankProfilePopupIconFromGroup();
                    Debug.Log("[PM] Other Profile and Rank");
                }
            }
        }
        else
        {
            Debug.Log("프로필 팝업이 없어요");
        }
    }
    public void ClosePopup()
    {
        if (activePopup == null) return;

        Debug.Log($"active POP close {activePopup.name}");

        activePopup.SetActive(false);
        activePopup = null;
    }

    public void ShowLoading(string text)
    {
        string output = text + "중입니다";

        if (loadingPopup != null && !loadingPopup.activeSelf)
        {
            loadingPopup.SetActive(true);
            TMP_Text loadingText = loadingPopup.GetComponentInChildren<TMP_Text>();
            TextAnimation anim = loadingText.GetComponent<TextAnimation>();
            RectTransform rect = loadingPopup.GetComponent<RectTransform>();

            loadingText.text = text;
            anim.baseText = output;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, Screen.height);


        }
    }
    public void ChangeLoadingText(string text)
    {
        string output = text + "실패!" +
            "";
        if (loadingPopup != null && loadingPopup.activeSelf)
        {
            loadingPopup.GetComponentInChildren<TMP_Text>().text = output;
        }
    }
    public void HideLoading()
    {
        Debug.Log("작동하는거냐 하이드씨");
        if (loadingPopup != null && loadingPopup.activeSelf)
        {
            loadingPopup.SetActive(false);
        }
    }

    public void HideLoading(float time)
    {
        Invoke(nameof(HideLoading), time);
    }
    // 🔹 클라이언트에서 데이터를 받은 후 실행
    public void OnDataReceived()
    {
        pendingOnComplete?.Invoke(); // ✅ 저장된 콜백 실행
        pendingOnComplete = null;  // ✅ 콜백 초기화
    }

    public void DisconnectedNetworkShow()
    {
        ShowPopup(warningPopup);
        warningPopup.GetComponent<ModalPopup>().config.text = "인터넷 연결이 되어있지 않습니다. \n" + "인터넷 연결 후 게임을 재실행해주세요.";
        warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
        warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
        warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => Application.Quit());
    }
    public void NonWifiNetworkShow(Action action)
    {
        PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
        PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "데이터로 연결되어 있습니다. 정말 다운받으시겠습니까?";
        PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
        PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => { action?.Invoke();});
    }
}
