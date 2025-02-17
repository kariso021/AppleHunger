using UnityEngine;

public class NavManager : MonoBehaviour
{
    public static NavManager Instance;

    [Header("Main Panels")]
    public GameObject homePanel;
    public GameObject rankingPanel;
    public GameObject settingsPanel;
    public GameObject profilePanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPanel(GameObject panel)
    {
        homePanel.SetActive(false);
        rankingPanel.SetActive(false);
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);

        panel.SetActive(true);
    }
}
