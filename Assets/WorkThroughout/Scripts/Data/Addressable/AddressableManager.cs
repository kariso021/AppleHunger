using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
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
    [SerializeField]
    private AssetReferenceSprite[] emojiSprites;

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

    // UI 바인딩 관련
    private bool isUIReady = false;
    private List<Action> pendingProfileUpdateActions = new List<Action>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임이 진행하는 동안엔 삭제가 일어나면 안되므로

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnLoaded;
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
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isUIReady = false; // 다음 씬에서 새 UI를 바인딩하기 전까지는 준비 안 됨
        StartCoroutine(DelayedAssignUIReferences());
    }


    private IEnumerator DelayedAssignUIReferences()
    {
        yield return new WaitForSeconds(0.1f); // 1 프레임 대기

        // 비활성화 포함해서 Image 컴포넌트 전부 탐색
        var allImages = Resources.FindObjectsOfTypeAll<Image>();

        foreach (var img in allImages)
        {
            if (img.gameObject.name == "IMG_Profile_Lobby")
                profileIcon = img;
            else if (img.gameObject.name == "IMG_Profile_Popup_Add")
                profilePopupIcon = img;
            else if (img.gameObject.name == "IMG_Profile_Popup_Rank")
                rankProfilePopupIcon = img;
            else if (img.gameObject.name == "MyRankProfileIconGameObject")
                myRankProfileIcon = img;
        }

        isUIReady = true;

        // 🔥 대기 중이던 액션 실행
        foreach (var action in pendingProfileUpdateActions)
        {
            action?.Invoke();
        }
        pendingProfileUpdateActions.Clear();

        Debug.Log("[Addressable] Complete UI Binding include deactive objects!");
    }

    void OnSceneUnLoaded(Scene scene)
    {
        rankingIconObj?.Clear();
        itemIconObj?.Clear();
        itemBoardObj?.Clear();
        matchIconObj?.Clear();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnLoaded;
    }

    private IEnumerator initAddressable()
    {
        var init = Addressables.InitializeAsync();
        yield return init;
    }
    public void LoadImageFromGroup(string itemUniqueId,Image image)
    {
        Debug.Log("[Addressable] Request Image Update");

        string key = null;

        if (itemUniqueId[0] == '1')
            key = "icon_" + itemUniqueId;
        else if (itemUniqueId[0] == '2')
            key = "board_" + itemUniqueId;
        else if (itemUniqueId[0] == '3')
            key = "Emoji_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                image.sprite = handle.Result;

                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }
            }
        };
    }
    /// <summary>
    /// itemUniqueId로 itemData에서 참조하여 사용. 매개변수는 string 형태이므로 .ToString() 메서드를 이용해 변환 필요
    /// 매개변수값은 함수 내부에서 "icon_ + 매개변수" 값의 형태로 변형되어 동작
    /// </summary>
    /// <param name="itemUniqueId"></param>
    public void LoadProfileIconFromGroup()
    {
        if (!isUIReady)
        {
            pendingProfileUpdateActions.Add(() => LoadProfileIconFromGroup());
            return;
        }

        string itemUniqueId = SQLiteManager.Instance.player.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                profileIcon.sprite = handle.Result;

                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }

            }
        };

    }
    public void LoadProfilePopupIconFromGroup()
    {

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

        if (!isUIReady)
        {
            Debug.Log("[Addressable] UI isn't ready for LoadImage.");
            pendingProfileUpdateActions.Add(() => LoadMyRankingIconFromGroup());
            return;
        }

        string itemUniqueId = SQLiteManager.Instance.player.profileIcon;
        string key = "icon_" + itemUniqueId;

        Addressables.LoadAssetAsync<Sprite>(key).Completed += (handle) =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                if (myRankProfileIcon != null)
                {
                    myRankProfileIcon.sprite = handle.Result;
                    Debug.Log("MY RANK NOT NULL");
                }
                else
                {
                    Debug.Log("MY RANK NULL");
                }

                if (!loadedSprites.ContainsKey(itemUniqueId))
                {
                    loadedSprites.Add(itemUniqueId, handle.Result);
                }

                Debug.Log("[Addressable] Complete load my ranking icon");
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
