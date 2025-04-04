using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [Header("Main Panels")]
    public GameObject homePanel;
    public GameObject rankingPanel;
    public GameObject collectionPanel;

    [Header("Collecion Panels")]
    public GameObject chooseIconPanel;
    public GameObject chooseBoardPanel;
    public GameObject customPanel;
    private Dictionary<string, GameObject> panels;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // �г��� Dictionary�� ���
        panels = new Dictionary<string, GameObject>
        {
            { "Home", homePanel },
            { "Ranking", rankingPanel },
            { "Collection", collectionPanel },
            { "Collection/Icon", chooseIconPanel },
            { "Collection/Board",chooseBoardPanel },
            { "Collection/Custom",customPanel}
        };
    }

    public void ShowPanel(string panelName)
    {
        // ��� �г� ��Ȱ��ȭ
        foreach (var panel in panels.Values)
        {
            panel.SetActive(false);
        }

        // Ư�� �г� Ȱ��ȭ
        if (panels.ContainsKey(panelName))
        {
            if (panelName == "Collection/Icon")
            {
                panels["Collection"].SetActive(true);
                FindAnyObjectByType<ItemManager>().CreateItemList("icon");
            }
            else if (panelName == "Collection/Board")
            {
                panels["Collection"].SetActive(true);
                FindAnyObjectByType<ItemManager>().CreateItemList("board");
            }
            else if(panelName == "Collection")
            {
                panels["Collection/Custom"].SetActive(true);
            }
            else if (panelName == "Ranking")
                FindAnyObjectByType<RankingRecordsManager>().CreateRankRecords();
            panels[panelName].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Panel '{panelName}'�� ã�� �� �����ϴ�!");
        }
    }
}
