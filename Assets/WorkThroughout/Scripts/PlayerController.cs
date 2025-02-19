using FishNet.Managing;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using FishNet;

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
    private bool isDragRestricted = false;
    private bool isCooldownActive = false;

    [Header("Local Drag Box")]
    public GameObject localDragBox; // 로컬 전용 드래그 박스
    private SpriteRenderer localDragBoxRenderer;

    [Header("Network Drag Box Manager")]
    private NetworkDragBoxManager networkDragBoxManager;

    public Image flashImage;
    private CanvasGroup flashCanvasGroup;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        scoreManager = GetComponent<ScoreManager>(); // ✅ ScoreManager 연결

        if (localDragBox != null)
        {
            localDragBoxRenderer = localDragBox.GetComponent<SpriteRenderer>();
            localDragBoxRenderer.enabled = false;
        }
        else
        {
            Debug.LogError("🚨 Local DragBox가 씬에 존재하지 않습니다!");
        }

        if (flashImage != null)
        {
            flashCanvasGroup = flashImage.GetComponent<CanvasGroup>() ?? flashImage.gameObject.AddComponent<CanvasGroup>();
            flashCanvasGroup.alpha = 0f;
            flashCanvasGroup.blocksRaycasts = false;
        }

        // ✅ 자기 자신의 NetworkDragBoxManager 참조
        networkDragBoxManager = GetComponentInChildren<NetworkDragBoxManager>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }
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

    #region Touch Events

    private void OnFingerDown(Finger finger)
    {
        if (!IsOwner || isDragRestricted || isCooldownActive) return;

        dragStartPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
        isDragging = false;
    }

    private void OnFingerMove(Finger finger)
    {
        if (!IsOwner || isDragRestricted || isCooldownActive) return;

        if (!isDragging)
        {
            float dragThreshold = 0.1f;
            if (Vector2.Distance(dragStartPos, mainCamera.ScreenToWorldPoint(finger.screenPosition)) > dragThreshold)
            {
                isDragging = true;
                localDragBoxRenderer.enabled = true;
                selectedApples.Clear();
                currentSum = 0;

                // ✅ 서버에 드래그 시작 요청 (네트워크 드래그 박스)
                networkDragBoxManager.SendDragStartServerRpc(dragStartPos, base.Owner);
            }
        }

        if (isDragging)
        {
            dragEndPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
            UpdateLocalDragBox();
            DetectAppleUnderCursor();

            // ✅ 서버에 드래그 업데이트 요청
            networkDragBoxManager.SendDragUpdateServerRpc(dragStartPos, dragEndPos, base.Owner);
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (!isDragging) return;

        if (currentSum == 10)
        {
            RequestAppleRemovalServerRpc(selectedApples.ToArray(), currentSum);
        }
        else
        {
            // ✅ sum != 10이면 원래 색상으로 복구
            ResetAppleColors();
        }

        localDragBoxRenderer.enabled = false;
        isDragging = false;

        // ✅ 서버에 드래그 종료 요청
        networkDragBoxManager.SendDragEndServerRpc(base.Owner);
    }

    #endregion

    #region Drag Box Methods

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

        foreach (GameObject apple in selectedApples)
        {
            if (apple == null) continue;

            Vector2 appleCenter = apple.transform.position;

            if (!dragBounds.Contains(appleCenter))
            {
                applesToDeselect.Add(apple);
            }
        }

        foreach (GameObject apple in applesToDeselect)
        {
            if (apple != null && originalColors.ContainsKey(apple))
            {
                apple.GetComponent<SpriteRenderer>().color = originalColors[apple];
                selectedApples.Remove(apple);
                currentSum -= apple.GetComponent<Apple>().Value;
            }
        }

        foreach (GameObject apple in GameObject.FindGameObjectsWithTag("Apple"))
        {
            if (apple == null) continue;

            Vector2 appleCenter = apple.transform.position;

            if (dragBounds.Contains(appleCenter))
            {
                Apple appleComponent = apple.GetComponent<Apple>();
                if (!selectedApples.Contains(apple) && appleComponent != null)
                {
                    SpriteRenderer appleRenderer = apple.GetComponent<SpriteRenderer>();

                    if (!originalColors.ContainsKey(apple))
                    {
                        originalColors[apple] = appleRenderer.color;
                    }

                    selectedApples.Add(apple);
                    currentSum += appleComponent.Value;
                    appleRenderer.color = Color.yellow;
                }
            }
        }
    }

    private void ResetAppleColors()
    {
        foreach (GameObject apple in selectedApples)
        {
            if (apple != null && originalColors.ContainsKey(apple))
            {
                apple.GetComponent<SpriteRenderer>().color = originalColors[apple]; // ✅ 원래 색상 복구
            }
        }
        selectedApples.Clear();
        currentSum = 0; // ✅ 합계 초기화
    }

    #endregion

    #region Apple Management

    [ServerRpc(RequireOwnership = false)]
    private void RequestAppleRemovalServerRpc(GameObject[] apples, int sum, NetworkConnection sender = null)
    {
        if (sum == 10 && sender != null)
        {
            Debug.Log($"Server: Removing {apples.Length} apples.");

            int appleCount = apples.Length;
            int AppleScoreValue = 0;

            foreach (GameObject apple in apples)
            {
                if (apple != null && apple.TryGetComponent(out Apple appleComponent) &&
                    apple.TryGetComponent(out NetworkObject netObj))
                {
                    AppleScoreValue = appleComponent.ScoreValue;
                    InstanceFinder.ServerManager.Despawn(apple);
                    Destroy(apple);
                }
            }

            // 개별 플레이어의 ScoreManager 찾기
            ScoreManager scoreManager = sender.FirstObject.GetComponent<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScoreServerRpc(appleCount, AppleScoreValue, sender); // 점수 공식 적용
            }
            else
            {
                Debug.LogError($"🚨 ScoreManager를 찾을 수 없습니다! ClientId: {sender.ClientId}");
            }
        }
    }

    #endregion
}
