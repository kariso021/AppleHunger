using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
/*
�� ���� ���� ���⼭ �ؾ��� ��
1. 
 */
public class AddressableManager : MonoBehaviour
{
    private static AddressableManager instance;
    public static AddressableManager Instance => instance;
    // Addressable
    [SerializeField]
    private AssetReferenceSprite[] iconSprites;
    [SerializeField]
    private AssetReferenceSprite[] boardsSprites;

    // Binding
    [SerializeField]
    public Image profileIcon; // Home�� ������
    public Image profilePopupIcon; // Home�� ������ Ŭ���� ������ �˾�â ������
    public Image rankProfilePopupIcon; // ��ŷ �ǿ��� ������ Ŭ���� ������ �˾�â
    public Image myRankProfileIcon; // ��ŷ �ǿ��� �� ��ũ ������ �̹���
    public List<GameObject> rankingIconObj; // ��ŷ �ǿ��� ��Ŀ���� ������ �̹���
    public List<GameObject> itemIconObj; // collection ������ ������ �̹���
    public List<GameObject> itemBoardObj; // collection ������ ���� �̹���
    public List<GameObject> matchIconObj; // MatchHistory ������ ������ �̹���

    // Release
    // ���� spritesList ��� Dictionary ��� (key -> sprite ����)
    private Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ������ �����ϴ� ���ȿ� ������ �Ͼ�� �ȵǹǷ�
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // 20250318 ����
        //StartCoroutine(initAddressable());
        DataSyncManager.Instance.OnPlayerProfileChanged += () => LoadProfileIconFromGroup();
        DataSyncManager.Instance.OnPlayerProfileChanged += () => LoadMyRankingIconFromGroup();
        //DataSyncManager.Instance.OnMatchHistoryChanged += () =>
    }

    private IEnumerator initAddressable()
    {
        var init = Addressables.InitializeAsync();
        yield return init;
    }

    /// <summary>
    /// itemUniqueId�� itemData���� �����Ͽ� ���. �Ű������� string �����̹Ƿ� .ToString() �޼��带 �̿��� ��ȯ �ʿ�
    /// �Ű��������� �Լ� ���ο��� "icon_ + �Ű�����" ���� ���·� �����Ǿ� ����
    /// </summary>
    /// <param name="itemUniqueId"></param>
    public void LoadProfileIconFromGroup()
    {
        Debug.Log("������ ������ ������Ʈ");
        string itemUniqueId = SQLiteManager.Instance.player.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                profileIcon.sprite = handle.Result;
                // �ε�� Sprite�� Dictionary�� �����Ͽ� ���߿� ���� �����ϵ��� ����
                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }
            }
        };
    }
    public void LoadProfilePopupIconFromGroup()
    {
        Debug.Log("������ ������ ������Ʈ");

        string itemUniqueId = SQLiteManager.Instance.player.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                profilePopupIcon.sprite = handle.Result;
                // �ε�� Sprite�� Dictionary�� �����Ͽ� ���߿� ���� �����ϵ��� ����
                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }
            }
        };
    }
    public void LoadRankProfilePopupIconFromGroup()
    {
        Debug.Log("������ ��ŷ ������ ������Ʈ");

        string itemUniqueId = SQLiteManager.Instance.playerDetails.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                rankProfilePopupIcon.sprite = handle.Result;
                // �ε�� Sprite�� Dictionary�� �����Ͽ� ���߿� ���� �����ϵ��� ����
                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }
            }
        };
    }
    public void LoadRankingIconFromGroup()
    {
        foreach (var imageObj in rankingIconObj)
        {
            string itemUniqueId = imageObj.GetComponent<RankingData>().profileIcon;
            Image image = imageObj.GetComponent<RankingData>().iconImage;
            string fileName = "icon_" + itemUniqueId;

            Addressables.LoadAssetAsync<Sprite>(fileName).Completed += (handle) =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    image.sprite = handle.Result;
                    // �ε�� Sprite�� Dictionary�� �����Ͽ� ���߿� ���� �����ϵ��� ����
                    if (!loadedSprites.ContainsKey(itemUniqueId))
                    {
                        loadedSprites.Add(itemUniqueId, handle.Result);
                    }
                }
            };
        }
    }
    public void LoadMyRankingIconFromGroup()
    {
        string itemUniqueId = SQLiteManager.Instance.player.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                myRankProfileIcon.sprite = handle.Result;
                // �ε�� Sprite�� Dictionary�� �����Ͽ� ���߿� ���� �����ϵ��� ����
                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }
            }
        };

    }
    public void LoadItemIconFromGroup()
    {
        foreach (var imageObj in itemIconObj)
        {
            string itemUniqueId = imageObj.GetComponent<ItemData>().itemUniqueId.ToString();
            Image image = imageObj.GetComponent<ItemData>().itemIcon;
            string fileName = "icon_" + itemUniqueId;

            Addressables.LoadAssetAsync<Sprite>(fileName).Completed += (handle) =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    image.sprite = handle.Result;
                    // �ε�� Sprite�� Dictionary�� �����Ͽ� ���߿� ���� �����ϵ��� ����
                    if (!loadedSprites.ContainsKey(itemUniqueId))
                    {
                        loadedSprites.Add(itemUniqueId, handle.Result);
                    }
                }
            };
        }
    }
    public void LoadItemBoardFromGroup()
    {
        foreach (var imageObj in itemBoardObj)
        {
            string itemUniqueId = imageObj.GetComponent<ItemData>().itemUniqueId.ToString();
            Image image = imageObj.GetComponent<ItemData>().itemIcon;
            string fileName = "board_" + itemUniqueId;

            Addressables.LoadAssetAsync<Sprite>(fileName).Completed += (handle) =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    image.sprite = handle.Result;
                    // �ε�� Sprite�� Dictionary�� �����Ͽ� ���߿� ���� �����ϵ��� ����
                    if (!loadedSprites.ContainsKey(itemUniqueId))
                    {
                        loadedSprites.Add(itemUniqueId, handle.Result);
                    }
                }
            };
        }
    }
    public void LoadMatchIconFromGroup()
    {
        foreach (var imageObj in matchIconObj)
        {
            string itemUniqueId = imageObj.GetComponent<MatchData>().itemUnqiueId;
            Image image = imageObj.GetComponent<MatchData>().iconSprite;
            string fileName = "icon_" + itemUniqueId;

            Addressables.LoadAssetAsync<Sprite>(fileName).Completed += (handle) =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    image.sprite = handle.Result;
                    // �ε�� Sprite�� Dictionary�� �����Ͽ� ���߿� ���� �����ϵ��� ����
                    if (!loadedSprites.ContainsKey(itemUniqueId))
                    {
                        loadedSprites.Add(itemUniqueId, handle.Result);
                    }
                }
            };
        }
    }
    public void profileIconRelease(string key)
    {
        if (loadedSprites.Count == 0) return;

        if (loadedSprites.ContainsKey(key))
        {
            Addressables.Release(loadedSprites[key]); // �ε�� Sprite ����
            loadedSprites.Remove(key); // Dictionary���� ����
        }
    }
}
