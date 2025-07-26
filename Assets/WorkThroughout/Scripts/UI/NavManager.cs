using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NavManager : MonoBehaviour
{
    public static NavManager Instance;
    public static string currentScene;
    public static string previousScene;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
    }

    public void NavigateTo(string panelName)
    {
        if (PopupManager.Instance.activePopup != null)
            PopupManager.Instance.ClosePopup();

        PanelManager.Instance.ShowPanel(panelName);
    }
}
