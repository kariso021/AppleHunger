using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : NetworkBehaviour // 🟢 네트워크 오브젝트로 변경
{
    private Camera mainCamera;
    private List<GameObject> selectedApples = new List<GameObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private int currentSum = 0;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;
    private bool isDragRestricted = false; // 🚫 드래그 차단 여부
    private bool isCooldownActive = false; // 🔥 1초 쿨타임 방지

    public GameObject dragBox;
    private SpriteRenderer dragBoxRenderer;
    public Image flashImage;
    private CanvasGroup flashCanvasGroup;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        mainCamera = Camera.main;

        if (dragBox == null)
        {
            dragBox = GameObject.Find("DragBox");
        }

        if (dragBox != null)
        {
            dragBoxRenderer = dragBox.GetComponent<SpriteRenderer>();
            dragBoxRenderer.enabled = false;
        }
        else
        {
            Debug.LogError("🚨 DragBox가 씬에 존재하지 않습니다! Hierarchy에서 확인하세요.");
        }

        if (flashImage != null)
        {
            flashCanvasGroup = flashImage.GetComponent<CanvasGroup>();

            if (flashCanvasGroup == null)
            {
                flashCanvasGroup = flashImage.gameObject.AddComponent<CanvasGroup>();
            }

            flashCanvasGroup.alpha = 0f; // 처음엔 투명
            flashCanvasGroup.blocksRaycasts = false; // 처음엔 터치 가능
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            enabled = false; // 🛑 다른 플레이어의 입력을 방지
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

    private void OnFingerDown(Finger finger)
    {
        if (!IsOwner || isDragRestricted || isCooldownActive) return; // 🛑 본인만 입력 가능

        dragStartPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
        isDragging = false;
    }

    private void OnFingerMove(Finger finger)
    {
        if (!IsOwner || isDragRestricted || isCooldownActive) return; // 🛑 본인만 입력 가능

        if (!isDragging)
        {
            float dragThreshold = 0.1f;
            if (Vector2.Distance(dragStartPos, mainCamera.ScreenToWorldPoint(finger.screenPosition)) > dragThreshold)
            {
                isDragging = true;
                dragBoxRenderer.enabled = true;
                selectedApples.Clear();
                currentSum = 0;
            }
        }

        if (isDragging)
        {
            dragEndPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
            UpdateDragBox();
            DetectAppleUnderCursor();
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (!isDragging) return;

        // 🟢 서버에게 사과 제거 요청을 보냄
        RequestAppleRemovalServerRpc(selectedApples.ToArray(), currentSum);

        dragBoxRenderer.enabled = false;
        isDragging = false;
    }

    private void UpdateDragBox()
    {
        Vector2 center = (dragStartPos + dragEndPos) / 2;
        Vector2 size = new Vector2(Mathf.Abs(dragEndPos.x - dragStartPos.x), Mathf.Abs(dragEndPos.y - dragStartPos.y));

        dragBox.transform.position = center;
        dragBox.transform.localScale = new Vector3(size.x, size.y, 1);
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
                currentSum -= apple.GetComponent<Apple>().value;
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
                    currentSum += appleComponent.value;
                    appleRenderer.color = Color.yellow;
                }
            }
        }
    }


    [ServerRpc] // 🟢 서버에서만 실행
    private void RequestAppleRemovalServerRpc(GameObject[] apples, int sum)
    {
        if (sum == 10)
        {
            GameServer server = FindObjectOfType<GameServer>();
            if (server != null)
            {
                server.GetAppleManager().RemoveApplesServerRpc(apples); // ✅ AppleManager의 RemoveApples 호출
                server.GetScoreManager().AddScore(100, apples.Length);
            }
        }
    }
}

