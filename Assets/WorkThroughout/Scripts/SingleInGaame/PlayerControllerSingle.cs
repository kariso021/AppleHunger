using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerControllerSingle : MonoBehaviour
{
    public static PlayerControllerSingle Instance { get; private set; }

    private Camera mainCamera;
    private List<GameObject> selectedApples = new List<GameObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private int currentSum = 0;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging = false;
    private bool isDragRestricted = true;

    [Header("Drag Box")]
    public GameObject localDragBox;
    private SpriteRenderer localDragBoxRenderer;

    [Header("Flash Effect")]
    public Image flashImage;
    public Slider restrictTimerSlider;
    private CanvasGroup flashCanvasGroup;

    [Header("Combo UI")]
    [SerializeField] private ComboUI comboUIgameObj;

    private float updateInterval = 0.016f; // 20 FPS
    private float timeSinceLastUpdate = 0f;

    private ScoreManagerSingle scoreManager;



    private void Awake()
    {
        EnhancedTouchSupport.Enable();

        if (Instance == null) Instance = this;
        else Destroy(gameObject);

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

        int selectedCount = selectedApples.Count;

        if (currentSum == 10 && selectedCount >=2)
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

            // 20250507 ������ ���� �߰� Null ���� �߰���
            if(AudioManager.Instance != null)
                AudioManager.Instance.PlayVFX(1);
   

            // �巡�� �ڽ� ���ο� �޺� ī��Ʈ ����
            comboUIgameObj.gameObject.transform.position = localDragBox.transform.position;
            comboUIgameObj.ShowComboEffect();
        }
        else if(selectedCount >=2)
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
            var apple = obj?.GetComponent<AppleSingle>();
            if (obj == null || apple == null || !apple.OverlapsBox(bounds))
            {
                var renderer = obj.GetComponent<SpriteRenderer>();
                renderer.color = originalColors[obj];
                apple.OnDeselect();
                currentSum -= apple?.Value ?? 0;
                originalColors.Remove(obj);
                selectedApples.RemoveAt(i);
            }
        }

        // ���� �� ���� ������ ��� ����
        foreach (var obj in GameObject.FindGameObjectsWithTag("Apple"))
        {
            var apple = obj.GetComponent<AppleSingle>();
            if (apple == null || selectedApples.Contains(obj)) continue;

            if (apple.OverlapsBox(bounds)) // ���ý�(����)
            {
                var renderer = obj.GetComponent<SpriteRenderer>();
                originalColors[obj] = renderer.color;
                apple.OnSelect();
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
            {
                obj.GetComponent<SpriteRenderer>().color = col;
                obj.GetComponent<AppleSingle>().OnDeselect();
            }
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

        StartCoroutine(nameof(RestrictTimerActive));

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
        yield return new WaitForSeconds(1.9f);

        // �÷��� �̹��� ��Ȱ��ȭ
        flashImage.gameObject.SetActive(false);

        isDragRestricted = false;
    }

    private IEnumerator RestrictTimerActive()
    {

        float elasped = 0f;
        float duration = 2f;

        restrictTimerSlider.gameObject.SetActive(true);


        restrictTimerSlider.value = 1f;

        while(elasped < duration)
        {
            elasped += Time.deltaTime;
            float amount = Mathf.Lerp(1f,0f, elasped / duration);
            restrictTimerSlider.value = amount;
            yield return null;
        }

        restrictTimerSlider.value = 0f;

        restrictTimerSlider.gameObject.SetActive(false);
    }

    public void RestrictAndRelease_When_Start_And_End(bool Start)
    {
        if (Start == true)
        {
            isDragRestricted = false;
        }
        else
        {
            isDragRestricted = true;
        }
    }

    public void RestrictTouchWhenGameEnded()
    {
        if (PlayerControllerSingle.Instance != null)
        {
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= PlayerControllerSingle.Instance.OnFingerDown;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= PlayerControllerSingle.Instance.OnFingerMove;
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= PlayerControllerSingle.Instance.OnFingerUp;
        }

        EnhancedTouchSupport.Disable();
    }
}