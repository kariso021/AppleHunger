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

        // �÷��� ����Ʈ ����
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
            Debug.LogError("Local DragBox�� �Ҵ���� �ʾҽ��ϴ�!");
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
            // �ùٸ��� 10�� ���� ���: ��� ���� & ���� ó��
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

            // ScoreManager�� ���� ���� ����
            // (�̱� ���� AddScore(int count, int score) �޼��� �ʿ�)
            scoreManager.AddScore(appleCount, appleScore);
        }
        else
        {
            // ���� �� �÷��� ����Ʈ
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

        // �巡�� ���� ������ ���� ��� ����
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

        // ���� �� ���� ������ ��� ����
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

        // �÷��� �̹��� Ȱ��ȭ
        flashImage.gameObject.SetActive(true);

        float half = 0.25f;
        float t = 0f;

        // ������� ��ο�����
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

        // ��� ���
        yield return new WaitForSeconds(1f);

        // �÷��� �̹��� ��Ȱ��ȭ
        flashImage.gameObject.SetActive(false);

        isDragRestricted = false;
    }
}