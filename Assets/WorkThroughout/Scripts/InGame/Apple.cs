using Unity.Netcode;
using TMPro;
using UnityEngine;

public class Apple : NetworkBehaviour
{
    private readonly NetworkVariable<int> value = new NetworkVariable<int>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> scoreValue = new NetworkVariable<int>(
        10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private TextMeshPro numberText;


    [SerializeField] private float detectSize = 0.2f; // 탐지 범위 크기

    [SerializeField] private SpriteRenderer selectOutline;

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public int Value => value.Value; // Getter
    public int ScoreValue => scoreValue.Value; // Getter

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            value.Value= Random.Range(1, 10);
            scoreValue.Value = 10;
        }

        // UI 초기화
        UpdateText();

        // 값이 변경될 때 UI 업데이트 (클라이언트에서도 자동 적용됨)
        value.OnValueChanged += (oldValue, newValue) => UpdateText();
    }

    // ? SetValue 수정 (서버에서만 값 변경 가능)
    public void SetValue(int someValue)
    {
        if (IsServer)
        {
            value.Value = someValue;
        }
    }

    public void SetGridPosition(int y, int x)
    {
        GridX = x;
        GridY = y;
    }

    private void UpdateText()
    {
        if (numberText != null)
        {
            numberText.text = Value.ToString();
        }
        else
        {
            Debug.LogError("numberText가 할당되지 않았습니다! Inspector에서 확인하세요.");
        }
    }

    public void OnSelect()
    {
        selectOutline.color = new Color(255f, 255f, 255f, 255f);
    }
    public void OnDeselect()
    {
        selectOutline.color = new Color(255f, 255f, 255f, 0f);
    }

    //판정범위용
    public bool OverlapsBox(Bounds box)
    {
        // 원형 중심과 가장 가까운 box 위의 점을 구함
        Vector3 closest = box.ClosestPoint(transform.position);

        float radius = detectSize;
        float sqrDist = (transform.position - closest).sqrMagnitude;

        return sqrDist <= radius * radius;
    }

}
