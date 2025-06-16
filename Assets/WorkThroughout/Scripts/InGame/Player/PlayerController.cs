using Unity.Netcode;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using System;
using Unity.Burst.Intrinsics;

public class PlayerController : NetworkBehaviour
{
    private Camera mainCamera;
    private List<GameObject> selectedApples = new List<GameObject>();
    private ScoreManager scoreManager;
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private int currentSum = 0;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;
    private bool isDragRestricted = false; //초기값 true 로 잡고
    private bool isCooldownActive = false;

    [SerializeField] ClientComboUI clientComboUI; // 콤보 UI를 위한 참조


    //콤보 불러오려고 잠깐 쓰는것
    public static Vector3 LastDragBoxWorldPos { get; private set; }


    //제한시간 동안 타이머 슬라이더 적용
    public Image restrictTimerSlider;





    public static event System.Action<ulong> OnPlayerInitialized;

    private float updateInterval = 0.016f; // 20 FPS
    private float timeSinceLastUpdate = 0f;

    public static event Action<int, int, ulong> OnAppleCollected; // (사과 개수, 점수 값, 클라이언트 ID)

    [Header("Local Drag Box")]
    public GameObject localDragBox;
    private SpriteRenderer localDragBoxRenderer;


    [Header("Network Drag Box")]
    private NetworkDragBoxManager networkDragBoxManager;

    public Image flashImage;
    private CanvasGroup flashCanvasGroup;

    private int myPlayerId;



    private void Awake()
    {
        EnhancedTouchSupport.Enable();


        flashCanvasGroup = flashImage.GetComponent<CanvasGroup>();
        if (flashCanvasGroup == null)
        {
            flashCanvasGroup = flashImage.gameObject.AddComponent<CanvasGroup>();
        }
        flashCanvasGroup.alpha = 0f;
        flashCanvasGroup.blocksRaycasts = false;
        restrictTimerSlider.gameObject.SetActive(false);
    }


    private void Start()
    {
        mainCamera = Camera.main;
     

        if (localDragBox != null)
        {
            localDragBoxRenderer = localDragBox.GetComponent<SpriteRenderer>();
            localDragBoxRenderer.enabled = false;
        }
        else
        {
            Debug.LogError("🚨 Local DragBox가 씬에 존재하지 않습니다!");
        }





        //  자기 자신의 NetworkDragBoxManager 참조
        networkDragBoxManager = GetComponentInChildren<NetworkDragBoxManager>();
    }


    private void OnEnable()
    {
        TouchSimulation.Enable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += OnFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += OnFingerMove;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= OnFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= OnFingerMove;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= OnFingerUp;
    }





    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        // 등록 로직은 PlayerRegister가 자체적으로 처리
        OnPlayerInitialized?.Invoke(OwnerClientId);
        myPlayerId = SQLiteManager.Instance.player.playerId;
        if (localDragBox != null)
        {
            localDragBoxRenderer = localDragBox.GetComponent<SpriteRenderer>();
            localDragBoxRenderer.enabled = true;
        }

        // 준비시간때 조작 불가능하게
        // 처음엔 드래그 제한
        RestrictDrag();

        // GameTimer와 연동
        if (GameTimer.Instance != null)
            GameTimer.OnTimerUpdated += CheckControlTime;
    }

    private void OnFingerDown(Finger finger)
    {
        if (!IsOwner || isDragRestricted || isCooldownActive) return;
        dragStartPos = Camera.main.ScreenToWorldPoint(finger.screenPosition);
        isDragging = false;
    }

    private void OnFingerMove(Finger finger)
    {
        if (!IsOwner || isDragRestricted || isCooldownActive) return;

        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate < updateInterval) return;
        timeSinceLastUpdate = 0f;

        if (!isDragging)
        {
            float dragThreshold = 0.1f;
            if (Vector2.Distance(dragStartPos, Camera.main.ScreenToWorldPoint(finger.screenPosition)) > dragThreshold)
            {
                isDragging = true;
                localDragBoxRenderer.enabled = true;
                selectedApples.Clear();
                currentSum = 0;

                networkDragBoxManager.SendDragStartServerRpc(dragStartPos, OwnerClientId);
            }
        }

        if (isDragging)
        {
            dragEndPos = Camera.main.ScreenToWorldPoint(finger.screenPosition);
            UpdateLocalDragBox();
            DetectAppleUnderCursor();

            networkDragBoxManager.SendDragUpdateServerRpc(dragStartPos,dragEndPos,OwnerClientId);
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (!isDragging) return;

        //apple 2개 이상선택되어야 
        int selectedCount = selectedApples.Count;
        if (currentSum == 10)
        {
            LastDragBoxWorldPos = localDragBox.transform.position;
            List<ulong> appleIds = new List<ulong>();
            foreach (GameObject apple in selectedApples)
            {
                if (apple.TryGetComponent(out NetworkObject netObj))
                {
                    appleIds.Add(netObj.NetworkObjectId);
                }
            }
            RequestAppleRemovalServerRpc(appleIds.ToArray(), currentSum, myPlayerId);

            // 사과 제거 및 사운드 처리
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayVFX(1);
   



        ResetAppleColors();
        }
        else
        {
            ResetAppleColors();
            if(selectedCount> 1)
                StartCoroutine(TriggerFlashEffect());
        }

        localDragBoxRenderer.enabled = false;
        isDragging = false;


        networkDragBoxManager.SendDragEndServerRpc(OwnerClientId);
    }

    private void UpdateLocalDragBox()
    {
        Vector2 center = (dragStartPos + dragEndPos) / 2;
        Vector2 size = new Vector2(Mathf.Abs(dragEndPos.x - dragStartPos.x), Mathf.Abs(dragEndPos.y - dragStartPos.y));

        localDragBox.transform.position = center;
        localDragBox.transform.localScale = new Vector3(size.x, size.y, 1);
    }

    private void DetectAppleUnderCursor()
    {
        Bounds dragBounds = new Bounds((dragStartPos + dragEndPos) / 2,
                                       new Vector3(Mathf.Abs(dragEndPos.x - dragStartPos.x), Mathf.Abs(dragEndPos.y - dragStartPos.y), 1));

        List<GameObject> applesToDeselect = new List<GameObject>();

        foreach (GameObject go in selectedApples)
        {
            if (go == null)
                continue;

            var apple = go.GetComponent<Apple>();
            // 컴포넌트가 없거나, bounds와 겹치지 않으면 해제 대상으로
            if (apple == null || !apple.OverlapsBox(dragBounds))
                applesToDeselect.Add(go);
        }

        foreach (GameObject apple in applesToDeselect)
        {
            if (apple != null && originalColors.ContainsKey(apple))
            {
                //apple.GetComponent<SpriteRenderer>().color = originalColors[apple]; 구버전

                //신버전
                apple.GetComponent<Apple>()?.OnDeselect();
                selectedApples.Remove(apple);
                currentSum -= apple.GetComponent<Apple>().Value;
            }
        }

        foreach (var go in GameObject.FindGameObjectsWithTag("Apple"))
        {
            if (go == null || selectedApples.Contains(go))
                continue;

            var apple = go.GetComponent<Apple>();
            if (apple == null)
                continue;

            if (apple.OverlapsBox(dragBounds))
            {
                var rend = go.GetComponent<SpriteRenderer>();
                // 최초 색 저장
                if (!originalColors.ContainsKey(go))
                    originalColors[go] = rend.color;

                // 선택 콜백
                apple.OnSelect();
                selectedApples.Add(go);
                currentSum += apple.Value;
            }
        }
    }

    private void ResetAppleColors()
    {
        foreach (GameObject apple in selectedApples)
        {
            if (apple != null && originalColors.ContainsKey(apple))
            {
                //apple.GetComponent<SpriteRenderer>().color = originalColors[apple]; 구버전
                apple.GetComponent<Apple>()?.OnDeselect(); // 신버전
            }
        }
        selectedApples.Clear();
        currentSum = 0;
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestAppleRemovalServerRpc(ulong[] appleIds, int sum, int playerId, ServerRpcParams rpcParams = default)
    {
        if (sum != 10) return;

        Debug.Log($"Server: Removing {appleIds.Length} apples.");

        int appleCount = appleIds.Length;
        int appleScoreValue = 0;

        // AppleScoreValue 계산
        foreach (ulong appleId in appleIds)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(appleId, out NetworkObject appleObj) &&
                appleObj.TryGetComponent(out Apple appleComponent))
            {
                appleScoreValue = appleComponent.ScoreValue;
                AppleManager.Instance?.DespawnApple(appleComponent);
            }
        }

        // playerID 기준으로 점수 처리
        ulong callerClientId = rpcParams.Receive.SenderClientId;
        ScoreManager.Instance.AddScore(playerId, appleCount, appleScoreValue,callerClientId, false);
        Debug.Log($"Server: {playerId}의 점수 업데이트");
    }

    //TrigerFlashImage

    private IEnumerator TriggerFlashEffect()
    {
        isDragRestricted = true; // 드래그 제한

        float flashDuration = 0.5f; // 총 지속 시간
        float halfDuration = flashDuration / 2f; // 절반 동안 밝아지고 절반 동안 어두워짐
        float elapsedTime = 0f;

        StartCoroutine(nameof(RestrictTimerActive));

        if (flashCanvasGroup != null)
        {
            // 밝아지는 구간
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 0.5f, elapsedTime / halfDuration); // 부드럽게 증가
                flashCanvasGroup.alpha = alpha;
                yield return null;
            }

            elapsedTime = 0f;

            // 어두워지는 구간
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0.5f, 0f, elapsedTime / halfDuration); // 부드럽게 감소
                flashCanvasGroup.alpha = alpha;
                yield return null;
            }

            flashCanvasGroup.alpha = 0f; // 최종적으로 완전히 투명
        }

        yield return new WaitForSeconds(1.0f); // 1초간 드래그 제한 유지
        isDragRestricted = false; // 드래그 제한 해제
    }


    //---------------------------------------------------------------------------------------조작제한


    public void RestrictDragForWhile(int seconds)
    {
        StartCoroutine(RestrictDragOnlyCoroutine(seconds));
    }

    private IEnumerator RestrictDragOnlyCoroutine(int seconds)
    {
        isDragRestricted = true;
        yield return new WaitForSeconds(seconds); // 2초 동안 조작 제한
        isDragRestricted = false;
    }

    public void RestrictDrag()
    {
        if (!IsOwner) return;
        isDragRestricted = true;
    }

    public void UnrestrictDrag()
    {
        if (!IsOwner) return;
        isDragRestricted = false;
    }

    //--------------------------------------------------------------------------------------------

    public void ShowLocalCombo(int comboCount)
    {
        clientComboUI.ShowCombo(comboCount, LastDragBoxWorldPos);
    }

    //-------------------------------------------------------------------------------------------- RestrictTimerSlider

    private IEnumerator RestrictTimerActive()
    {
        float elasped = 0f;
        float duration = 2f;

        restrictTimerSlider.gameObject.SetActive(true);

        restrictTimerSlider.fillAmount = 1f;

        while (elasped < duration)
        {
            elasped += Time.deltaTime;
            float amount = Mathf.Lerp(1f, 0f, elasped / duration);
            restrictTimerSlider.fillAmount = amount;
            yield return null;
        }

        restrictTimerSlider.fillAmount = 0f;

        restrictTimerSlider.gameObject.SetActive(false);
    }

    //-------------------------------------------------------------------------------------------- 컨트롤러 조작가능여부

    private void CheckControlTime(float newTime)
    {
        // 준비 시간 경과 후 한번만 해제
        if (GameTimer.Instance != null && GameTimer.Instance.CanControl)
        {
            UnrestrictDrag(); 
            GameTimer.OnTimerUpdated -= CheckControlTime; // 다시 호출되지 않게 해제
        }
    }

}
