using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("Popup Panels")]
    public GameObject creditPopup;
    public GameObject profilePopup;

    private GameObject activePopup = null; // 현재 활성화된 팝업 저장

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

        FindAnyObjectByType<MatchRecordsManager>().CreateMatchRecords();

        if (profilePopup != null)
        {
            profilePopup.GetComponent<ProfilePopup>().SetProfile(
                SQLiteManager.Instance.player.playerName,
                SQLiteManager.Instance.stats.totalGames,
                SQLiteManager.Instance.stats.wins,
                SQLiteManager.Instance.stats.losses,
                SQLiteManager.Instance.stats.winRate,
                SQLiteManager.Instance.player.rating,
                SQLiteManager.Instance.items.Count,
                SQLiteManager.Instance.items.Count
            );
        }

        Debug.Log($"active POP show {activePopup.name}");
    }

    public void ClosePopup()
    {
        if (activePopup == null) return;

        Debug.Log($"active POP close {activePopup.name}");

        activePopup.SetActive(false);
        activePopup = null;
    }
}
