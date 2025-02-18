using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [Header("Main Panels")]
    public GameObject homePanel;
    public GameObject rankingPanel;
    public GameObject settingsPanel;
    public GameObject collectionPanel;

    [Header("Collecion Panels")]
    public GameObject chooseIconPanel;
    public GameObject chooseBoardPanel;

    private Dictionary<string, GameObject> panels;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 패널을 Dictionary에 등록
        panels = new Dictionary<string, GameObject>
        {
            { "Home", homePanel },
            { "Ranking", rankingPanel },
            { "Settings", settingsPanel },
            { "Collection", collectionPanel },
            { "Collection/Icon", chooseIconPanel },
            { "Collection/Board",chooseBoardPanel }
        };
    }

    public void ShowPanel(string panelName)
    {
        // 모든 패널 비활성화
        foreach (var panel in panels.Values)
        {
            panel.SetActive(false);
        }

        // 특정 패널 활성화
        if (panels.ContainsKey(panelName))
        {
            if(panelName == "Collection/Icon" || panelName == "Collection/Board")
                panels["Collection"].SetActive(true);
            panels[panelName].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Panel '{panelName}'을 찾을 수 없습니다!");
        }
    }
}
