using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NavManager : MonoBehaviour
{
    public static NavManager Instance;
    public static string currentScene;
    public static string previousScene;

    [Header("NavBar Effect")]
    public List<Image> navBarButtonImageList;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
        //navBarButtonList = new List<Button>();
    }

    public void NavigateTo(string panelName)
    {
        if (PopupManager.Instance.activePopup != null)
            PopupManager.Instance.ClosePopup();

        PanelManager.Instance.ShowPanel(panelName);
    }

    public void ChangeNavBarActiveEffect(string activeButtonName)
    {

        foreach(var image in navBarButtonImageList)
        {
            if (image.gameObject.activeSelf)
            {
                image.gameObject.SetActive(false);
                break;
            }
        }

        switch (activeButtonName)
        {
            case "Home":
                navBarButtonImageList[0].gameObject.SetActive(true);
                break;
            case "Ranking":
                navBarButtonImageList[1].gameObject.SetActive(true);
                break;
            case "Collection":
                navBarButtonImageList[2].gameObject.SetActive(true);
                break;
        }
    }
}
