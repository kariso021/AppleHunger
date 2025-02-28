using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("Popup Panels")]
    public GameObject creditPopup;
    public GameObject profilePopup;

    private GameObject activePopup = null; // ���� Ȱ��ȭ�� �˾� ����

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
        // ���� ���� Ȱ��ȭ �� �˾��� �ִٸ�?
        if (activePopup != null)
        {
            // �˾��� ���� �ݾƾ� ��
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
