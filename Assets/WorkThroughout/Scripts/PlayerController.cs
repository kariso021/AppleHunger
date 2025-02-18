using FishNet.Managing;
using FishNet.Object;
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
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private int currentSum = 0;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;
    private bool isDragRestricted = false;
    private bool isCooldownActive = false;

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
            Debug.LogError("🚨 DragBox가 씬에 존재하지 않습니다!");
        }

        if (flashImage != null)
        {
            flashCanvasGroup = flashImage.GetComponent<CanvasGroup>() ?? flashImage.gameObject.AddComponent<CanvasGroup>();
            flashCanvasGroup.alpha = 0f;
            flashCanvasGroup.blocksRaycasts = false;
        }
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

        if (currentSum == 10)
        {
            RequestAppleRemovalServerRpc(selectedApples.ToArray(), currentSum);
        }
        else
        {
            // ✅ sum != 10이면 원래 색상으로 복구
            ResetAppleColors();
        }

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



    [ServerRpc]
    private void RequestAppleRemovalServerRpc(GameObject[] apples, int sum)
    {
        if (sum == 10)
        {
            Debug.Log($"🍏 Server: Removing {apples.Length} apples.");

            foreach (GameObject apple in apples)
            {
                if (apple != null && apple.TryGetComponent(out NetworkObject netObj))
                {
                    InstanceFinder.ServerManager.Despawn(apple); // ✅ FishNet 공식 방식 적용
                    Destroy(apple);
                }
            }
        }
    }


  

}
