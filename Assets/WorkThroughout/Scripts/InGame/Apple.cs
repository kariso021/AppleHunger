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


    [SerializeField] private float detectSize = 0.2f; // Ž�� ���� ũ��

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

        // UI �ʱ�ȭ
        UpdateText();

        // ���� ����� �� UI ������Ʈ (Ŭ���̾�Ʈ������ �ڵ� �����)
        value.OnValueChanged += (oldValue, newValue) => UpdateText();
    }

    // ? SetValue ���� (���������� �� ���� ����)
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
            Debug.LogError("numberText�� �Ҵ���� �ʾҽ��ϴ�! Inspector���� Ȯ���ϼ���.");
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

    //����������
    public bool OverlapsBox(Bounds box)
    {
        // ���� �߽ɰ� ���� ����� box ���� ���� ����
        Vector3 closest = box.ClosestPoint(transform.position);

        float radius = detectSize;
        float sqrDist = (transform.position - closest).sqrMagnitude;

        return sqrDist <= radius * radius;
    }

}
