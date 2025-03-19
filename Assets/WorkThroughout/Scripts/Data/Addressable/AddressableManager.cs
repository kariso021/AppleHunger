using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
/*
자 내가 지금 여기서 해야할 일
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
    public Image profileIcon; // Home의 프로필
    public Image profilePopupIcon; // Home의 프로필 클릭시 나오는 팝업창 프로필
    public Image rankProfilePopupIcon; // 랭킹 탭에서 프로필 클릭시 나오는 팝업창
    public Image myRankProfileIcon; // 랭킹 탭에서 내 랭크 프로필 이미지
    public List<GameObject> rankingIconObj; // 랭킹 탭에서 랭커들의 아이콘 이미지
    public List<GameObject> itemIconObj; // collection 에서의 아이콘 이미지
    public List<GameObject> itemBoardObj; // collection 에서의 보드 이미지
    public List<GameObject> matchIconObj; // MatchHistory 에서의 아이콘 이미지

    // Release
    // 기존 spritesList 대신 Dictionary 사용 (key -> sprite 저장)
    private Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임이 진행하는 동안엔 삭제가 일어나면 안되므로
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // 20250318 수정
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
    /// itemUniqueId로 itemData에서 참조하여 사용. 매개변수는 string 형태이므로 .ToString() 메서드를 이용해 변환 필요
    /// 매개변수값은 함수 내부에서 "icon_ + 매개변수" 값의 형태로 변형되어 동작
    /// </summary>
    /// <param name="itemUniqueId"></param>
    public void LoadProfileIconFromGroup()
    {
        Debug.Log("프로필 아이콘 업데이트");
        string itemUniqueId = SQLiteManager.Instance.player.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                profileIcon.sprite = handle.Result;
                // 로드된 Sprite를 Dictionary에 저장하여 나중에 해제 가능하도록 관리
                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }
            }
        };
    }
    public void LoadProfilePopupIconFromGroup()
    {
        Debug.Log("프로필 아이콘 업데이트");

        string itemUniqueId = SQLiteManager.Instance.player.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                profilePopupIcon.sprite = handle.Result;
                // 로드된 Sprite를 Dictionary에 저장하여 나중에 해제 가능하도록 관리
                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }
            }
        };
    }
    public void LoadRankProfilePopupIconFromGroup()
    {
        Debug.Log("프로필 랭킹 아이콘 업데이트");

        string itemUniqueId = SQLiteManager.Instance.playerDetails.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                rankProfilePopupIcon.sprite = handle.Result;
                // 로드된 Sprite를 Dictionary에 저장하여 나중에 해제 가능하도록 관리
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
                    // 로드된 Sprite를 Dictionary에 저장하여 나중에 해제 가능하도록 관리
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
                // 로드된 Sprite를 Dictionary에 저장하여 나중에 해제 가능하도록 관리
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
                    // 로드된 Sprite를 Dictionary에 저장하여 나중에 해제 가능하도록 관리
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
                    // 로드된 Sprite를 Dictionary에 저장하여 나중에 해제 가능하도록 관리
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
                    // 로드된 Sprite를 Dictionary에 저장하여 나중에 해제 가능하도록 관리
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
            Addressables.Release(loadedSprites[key]); // 로드된 Sprite 해제
            loadedSprites.Remove(key); // Dictionary에서 삭제
        }
    }
}
