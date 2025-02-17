using UnityEngine;
using UnityEngine.UI;
public class PanelController : MonoBehaviour
{
    public Button homeButton;
    public Button rankingButton;
    public Button settingsButton;
    public Button profileButton;
    private void Start()
    {
        // Button ¹ÙÀÎµù
        homeButton.onClick.AddListener(() => 
        NavManager.Instance.ShowPanel(NavManager.Instance.homePanel));
        rankingButton.onClick.AddListener(() => 
        NavManager.Instance.ShowPanel(NavManager.Instance.rankingPanel));
        settingsButton.onClick.AddListener(() => 
        NavManager.Instance.ShowPanel(NavManager.Instance.settingsPanel));
        profileButton.onClick.AddListener(() =>
        NavManager.Instance.ShowPanel(NavManager.Instance.profilePanel));
    }
}