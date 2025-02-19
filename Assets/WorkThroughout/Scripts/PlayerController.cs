using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : NetworkBehaviour
{
    private Camera mainCamera;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;

    [Header("Local Drag Box")]
    public GameObject localDragBox;  // 로컬 전용 드래그 박스
    private SpriteRenderer localDragBoxRenderer;

    [Header("Network Drag Box Manager")]
    private NetworkDragBoxManager networkDragBoxManager;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        localDragBoxRenderer = localDragBox.GetComponent<SpriteRenderer>();
        localDragBoxRenderer.enabled = false;

        // ✅ 자기 자신이 가진 NetworkDragBoxManager 참조
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

    private void OnFingerDown(Finger finger)
    {
        if (!IsOwner) return;
        StartDrag(finger);
    }

    private void OnFingerMove(Finger finger)
    {
        if (!IsOwner || !isDragging) return;
        UpdateDrag(finger);
    }

    private void OnFingerUp(Finger finger)
    {
        if (!isDragging) return;
        EndDrag();
    }

    private void StartDrag(Finger finger)
    {
        dragStartPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
        isDragging = true;
        localDragBoxRenderer.enabled = true;

        // ✅ 자기 자신의 NetworkDragBoxManager를 통해 서버에 드래그 시작 요청
        networkDragBoxManager.SendDragStartServerRpc(dragStartPos, base.Owner);
    }

    private void UpdateDrag(Finger finger)
    {
        dragEndPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
        UpdateLocalDragBox();

        // ✅ 자기 자신의 NetworkDragBoxManager를 통해 서버에 드래그 업데이트 요청
        networkDragBoxManager.SendDragUpdateServerRpc(dragStartPos, dragEndPos, base.Owner);
    }

    private void EndDrag()
    {
        localDragBoxRenderer.enabled = false;
        isDragging = false;

        // ✅ 자기 자신의 NetworkDragBoxManager를 통해 서버에 드래그 종료 요청
        networkDragBoxManager.SendDragEndServerRpc(base.Owner);
    }

    private void UpdateLocalDragBox()
    {
        Vector2 center = (dragStartPos + dragEndPos) / 2;
        Vector2 size = new Vector2(Mathf.Abs(dragEndPos.x - dragStartPos.x), Mathf.Abs(dragEndPos.y - dragStartPos.y));

        localDragBox.transform.position = center;
        localDragBox.transform.localScale = new Vector3(size.x, size.y, 1);
    }
}
