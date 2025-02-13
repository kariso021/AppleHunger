using UnityEngine;
using System.Collections.Generic;

public class AppleDragHandler : MonoBehaviour
{
    private Camera mainCamera;
    private List<GameObject> selectedApples = new List<GameObject>(); // 드래그한 사과 저장
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>(); // 사과 원래 색상 저장
    private int currentSum = 0; // 드래그된 사과 숫자의 합
    private Vector2 dragStartPos; // 드래그 시작 위치
    private Vector2 dragEndPos; // 드래그 끝 위치
    private bool isDragging = false;

    public GameObject dragBox; // 드래그 영역을 표시할 SpriteRenderer 오브젝트
    private SpriteRenderer dragBoxRenderer;

    private void Start()
    {
        mainCamera = Camera.main;

        if (dragBox == null)
        {
            dragBox = GameObject.Find("DragBox"); // 자동 연결
        }

        if (dragBox != null)
        {
            dragBoxRenderer = dragBox.GetComponent<SpriteRenderer>();
            dragBoxRenderer.enabled = false; // 처음에는 안 보이게 설정
        }
        else
        {
            Debug.LogError("🚨 DragBox가 씬에 존재하지 않습니다! Hierarchy에서 확인하세요.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 드래그 시작
        {
            dragStartPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
            dragBoxRenderer.enabled = true; // 드래그 박스 표시
            selectedApples.Clear(); // 이전 선택된 사과 초기화
            currentSum = 0;
        }

        if (Input.GetMouseButton(0)) // 드래그 중
        {
            dragEndPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            UpdateDragBox();
            DetectAppleUnderCursor();
        }

        if (Input.GetMouseButtonUp(0)) // 드래그 끝
        {
            CheckAndRemoveApples();
            dragBoxRenderer.enabled = false; // 드래그 박스 숨기기
            isDragging = false;
        }
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
        // 드래그 박스의 영역을 계산 (사과의 "중앙값" 기준)
        Bounds dragBounds = new Bounds((dragStartPos + dragEndPos) / 2,
                                       new Vector3(Mathf.Abs(dragEndPos.x - dragStartPos.x), Mathf.Abs(dragEndPos.y - dragStartPos.y), 1));

        // 🌟 현재 선택된 사과 중에서 드래그 영역을 벗어난 사과 찾기
        List<GameObject> applesToDeselect = new List<GameObject>();

        foreach (GameObject apple in selectedApples)
        {
            if (apple == null) continue;

            Vector2 appleCenter = apple.transform.position; // 사과 중앙 위치

            // 🛑 드래그 박스 밖에 있으면 원래 색상으로 복구
            if (!dragBounds.Contains(appleCenter))
            {
                applesToDeselect.Add(apple);
            }
        }

        // 원래 색상으로 복구
        foreach (GameObject apple in applesToDeselect)
        {
            if (apple != null && originalColors.ContainsKey(apple))
            {
                apple.GetComponent<SpriteRenderer>().color = originalColors[apple]; // 원래 색 복구
                selectedApples.Remove(apple); // 선택 목록에서 제거
                currentSum -= apple.GetComponent<Apple>().value; // 숫자 합계에서 제외
            }
        }

        // 🌟 새로운 사과 탐색 (드래그 박스 안에 있는 사과만 추가)
        foreach (GameObject apple in GameObject.FindGameObjectsWithTag("Apple"))
        {
            if (apple == null) continue;

            Vector2 appleCenter = apple.transform.position;

            // 🔥 사과의 중앙값이 드래그 박스 안에 포함될 때만 선택
            if (dragBounds.Contains(appleCenter))
            {
                Apple appleComponent = apple.GetComponent<Apple>();
                if (!selectedApples.Contains(apple) && appleComponent != null)
                {
                    SpriteRenderer appleRenderer = apple.GetComponent<SpriteRenderer>();

                    // 🌟 사과의 원래 색상 저장 (처음 선택될 때만)
                    if (!originalColors.ContainsKey(apple))
                    {
                        originalColors[apple] = appleRenderer.color;
                    }

                    selectedApples.Add(apple);
                    currentSum += appleComponent.value;
                    appleRenderer.color = Color.yellow; // 드래그된 사과 색상 변경
                }
            }
        }
    }


    private void CheckAndRemoveApples()
    {
        if (currentSum == 10) // 합이 10이면 제거
        {
            int cachedScorebyRemovedApple = 0;

            foreach (GameObject apple in selectedApples)
            {
                if (apple != null)
                {
                    int appleValue = apple.GetComponent<Apple>().scorevalue;
                    cachedScorebyRemovedApple += appleValue;
                    Destroy(apple);
                    originalColors.Remove(apple); // 제거된 사과는 원래 색상 목록에서도 삭제
                }
            }

            // 제거된 사과 개수* 각자의 AppleValue만큼
            GameManager.Instance.AddScore(cachedScorebyRemovedApple);
        }

        // 🌟 선택된 사과의 색상을 원래 색으로 복구
        foreach (GameObject apple in selectedApples)
        {
            if (apple != null && originalColors.ContainsKey(apple))
            {
                apple.GetComponent<SpriteRenderer>().color = originalColors[apple]; // 원래 색상 복구
            }
        }

        selectedApples.Clear();
        currentSum = 0;
    }

}