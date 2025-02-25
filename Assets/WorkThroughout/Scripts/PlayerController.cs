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

    private float updateInterval = 0.016f; // 20 FPS
    private float timeSinceLastUpdate = 0f;

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

        flashCanvasGroup = flashImage.GetComponent<CanvasGroup>();
        if (flashCanvasGroup == null)
        {
            flashCanvasGroup = flashImage.gameObject.AddComponent<CanvasGroup>();
        }

        // 3️⃣ 초기화
        flashCanvasGroup.alpha = 0f;
        flashCanvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        scoreManager = GetComponent<ScoreManager>(); // ScoreManager 연결

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

    public override void OnStartClient()
    {
        base.OnStartClient();

        // 모든 클라이언트에서 드래그 박스 렌더러 끄기
        if (localDragBoxRenderer != null)
        {
            localDragBoxRenderer.enabled = false;
            localDragBox.transform.localScale = Vector3.zero;  // 크기 초기화
        }

        // 자신의 클라이언트만 드래그 박스 렌더링 활성화
        if (IsOwner)
        {
            localDragBoxRenderer.enabled = true;
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

        //FPS 프레임조절
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate < updateInterval) return; // FPS 제한
        timeSinceLastUpdate = 0f;


        if (!isDragging)
        {
            float dragThreshold = 0.1f;
            if (Vector2.Distance(dragStartPos, mainCamera.ScreenToWorldPoint(finger.screenPosition)) > dragThreshold)
            {
                isDragging = true;
                localDragBoxRenderer.enabled = true;
                selectedApples.Clear();
                currentSum = 0;

                //  서버에 드래그 시작 요청 (네트워크 드래그 박스)
                networkDragBoxManager.SendDragStartServerRpc(dragStartPos, base.Owner);
            }
        }

        if (isDragging)
        {
            dragEndPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
            UpdateLocalDragBox();
            DetectAppleUnderCursor();

            // 서버에 드래그 업데이트 요청
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
            StartCoroutine(TriggerFlashEffect());
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
                apple.GetComponent<SpriteRenderer>().color = originalColors[apple]; // 원래 색상 복구
            }
        }
        selectedApples.Clear();
        currentSum = 0; // 합계 초기화
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


    //TrigerFlashImage

    private IEnumerator TriggerFlashEffect()
    {
        isDragRestricted = true; // 드래그 제한

        float flashDuration = 0.5f; // 총 지속 시간
        float halfDuration = flashDuration / 2f; // 절반 동안 밝아지고 절반 동안 어두워짐
        float elapsedTime = 0f;

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

}
