using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerControllerSingle : MonoBehaviour
{
    private Camera mainCamera;
    private List<GameObject> selectedApples = new List<GameObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private int currentSum = 0;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;
    private bool isDragRestricted = false;

    [Header("Drag Box")]
    public GameObject localDragBox;
    private SpriteRenderer localDragBoxRenderer;

    [Header("Flash Effect")]
    public Image flashImage;
    private CanvasGroup flashCanvasGroup;

    private float updateInterval = 0.016f; // 20 FPS
    private float timeSinceLastUpdate = 0f;

    private ScoreManagerSingle scoreManager;



    private void Awake()
    {
        EnhancedTouchSupport.Enable();

        // 플래시 이펙트 설정
        flashCanvasGroup = flashImage.GetComponent<CanvasGroup>();
        if (flashCanvasGroup == null)
            flashCanvasGroup = flashImage.gameObject.AddComponent<CanvasGroup>();
        flashCanvasGroup.alpha = 0f;
        flashCanvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        scoreManager = ScoreManagerSingle.Instance;

        if (localDragBox != null)
        {
            localDragBoxRenderer = localDragBox.GetComponent<SpriteRenderer>();
            localDragBoxRenderer.enabled = false;
        }
        else
        {
            Debug.LogError("Local DragBox가 할당되지 않았습니다!");
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
        if (isDragRestricted) return;
        dragStartPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
        isDragging = false;
    }

    private void OnFingerMove(Finger finger)
    {
        if (isDragRestricted) return;

        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate < updateInterval) return;
        timeSinceLastUpdate = 0f;

        Vector2 worldPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);

        if (!isDragging)
        {
            if (Vector2.Distance(dragStartPos, worldPos) > 0.1f)
            {
                isDragging = true;
                localDragBoxRenderer.enabled = true;
                selectedApples.Clear();
                currentSum = 0;
            }
        }

        if (isDragging)
        {
            dragEndPos = worldPos;
            UpdateLocalDragBox();
            DetectAppleUnderCursor();
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (!isDragging) return;

        if (currentSum == 10)
        {
            // 올바르게 10점 모은 경우: 사과 제거 & 점수 처리
            int appleCount = selectedApples.Count;
            int appleScore = 0;

            foreach (var appleObj in selectedApples)
            {
                if (appleObj.TryGetComponent(out AppleSingle apple))
                {
                    appleScore = apple.ScoreValue;
                    AppleManagerSingle.Instance.RemoveApple(apple);
                }
            }

            // ScoreManager에 수집 정보 전달
            // (싱글 모드용 AddScore(int count, int score) 메서드 필요)
            scoreManager.AddScore(appleCount, appleScore);
        }
        else
        {
            // 실패 시 플래시 이펙트
            StartCoroutine(TriggerFlashEffect());
        }

        ResetAppleColors();
        localDragBoxRenderer.enabled = false;
        isDragging = false;
    }

    private void UpdateLocalDragBox()
    {
        Vector2 center = (dragStartPos + dragEndPos) / 2f;
        Vector2 size = new Vector2(
            Mathf.Abs(dragEndPos.x - dragStartPos.x),
            Mathf.Abs(dragEndPos.y - dragStartPos.y)
        );

        localDragBox.transform.position = center;
        localDragBox.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    private void DetectAppleUnderCursor()
    {
        var bounds = new Bounds(
            (dragStartPos + dragEndPos) / 2f,
            new Vector3(
                Mathf.Abs(dragEndPos.x - dragStartPos.x),
                Mathf.Abs(dragEndPos.y - dragStartPos.y),
                1f
            )
        );

        // 드래그 영역 밖으로 나간 사과 해제
        for (int i = selectedApples.Count - 1; i >= 0; i--)
        {
            var obj = selectedApples[i];
            if (obj == null || !bounds.Intersects(obj.GetComponent<AppleSingle>().AppleBounds))
            {
                var renderer = obj.GetComponent<SpriteRenderer>();
                renderer.color = originalColors[obj];
                currentSum -= obj.GetComponent<AppleSingle>().Value;
                originalColors.Remove(obj);
                selectedApples.RemoveAt(i);
            }
        }

        // 영역 내 새로 진입한 사과 선택
        foreach (var obj in GameObject.FindGameObjectsWithTag("Apple"))
        {
            if (bounds.Intersects(obj.GetComponent<AppleSingle>().AppleBounds) && !selectedApples.Contains(obj))
            {
                var apple = obj.GetComponent<AppleSingle>();
                if (apple == null) continue;

                var renderer = obj.GetComponent<SpriteRenderer>();
                originalColors[obj] = renderer.color;

                selectedApples.Add(obj);
                currentSum += apple.Value;
                renderer.color = Color.yellow;
            }
        }
    }

    private void ResetAppleColors()
    {
        foreach (var obj in selectedApples)
        {
            if (obj != null && originalColors.TryGetValue(obj, out var col))
                obj.GetComponent<SpriteRenderer>().color = col;
        }
        selectedApples.Clear();
        originalColors.Clear();
        currentSum = 0;
    }

    private IEnumerator TriggerFlashEffect()
    {
        isDragRestricted = true;

        // 플래시 이미지 활성화
        flashImage.gameObject.SetActive(true);

        float half = 0.25f;
        float t = 0f;

        // 밝아졌다 어두워지기
        while (t < half)
        {
            t += Time.deltaTime;
            flashCanvasGroup.alpha = Mathf.Lerp(0f, 0.5f, t / half);
            yield return null;
        }
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            flashCanvasGroup.alpha = Mathf.Lerp(0.5f, 0f, t / half);
            yield return null;
        }
        flashCanvasGroup.alpha = 0f;

        // 잠시 대기
        yield return new WaitForSeconds(1f);

        // 플래시 이미지 비활성화
        flashImage.gameObject.SetActive(false);

        isDragRestricted = false;
    }
}