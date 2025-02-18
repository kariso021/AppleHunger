using UnityEngine;

public class NavManager : MonoBehaviour
{
    public static NavManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void NavigateTo(string panelName)
    {
        PanelManager.Instance.ShowPanel(panelName);
    }
}
