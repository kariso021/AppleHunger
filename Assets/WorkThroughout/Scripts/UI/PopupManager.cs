using System;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("Popup Panels")]
    public GameObject creditPopup;
    public GameObject profilePopup;
    public GameObject rankProfilePopup;
    public GameEnding nicknamePopup;

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

        if (popup.tag == "Profile")
        {
            Debug.Log("쇼 팝업 클라이언트 아이디");
            pendingOnComplete = () => OnPlayerDetailsLoaded();
            StartCoroutine(ClientNetworkManager.Instance.GetPlayerDetalis(SQLiteManager.Instance.player.playerId));
        }

    }
    public void ShowPopup(GameObject popup, int playerId)
    {
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
            pendingOnComplete = () => OnPlayerDetailsLoaded();
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
                activePopup.GetComponent<ProfilePopup>().SetProfile(
                SQLiteManager.Instance.playerDetails.playerName,
                SQLiteManager.Instance.playerDetails.totalGames,
                SQLiteManager.Instance.playerDetails.wins,
                SQLiteManager.Instance.playerDetails.losses,
                SQLiteManager.Instance.playerDetails.winRate,
                SQLiteManager.Instance.playerDetails.rating,
                SQLiteManager.Instance.playerDetails.unlockIcons,
                SQLiteManager.Instance.playerDetails.unlockBoards
                );


                if (SQLiteManager.Instance.player.playerId == SQLiteManager.Instance.playerDetails.playerId)
                {
                    FindAnyObjectByType<MatchRecordsManager>().CreateMatchRecords();
                    if(GameObject.Find("RankingPanel") == true) // 일단 임시 방편...
                        AddressableManager.Instance.LoadRankProfilePopupIconFromGroup();
                    else
                        AddressableManager.Instance.LoadProfilePopupIconFromGroup();
                }
                else 
                    AddressableManager.Instance.LoadRankProfilePopupIconFromGroup();
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

    // 🔹 클라이언트에서 데이터를 받은 후 실행
    public void OnDataReceived()
    {
        pendingOnComplete?.Invoke(); // ✅ 저장된 콜백 실행
        pendingOnComplete = null;  // ✅ 콜백 초기화
    }
}
