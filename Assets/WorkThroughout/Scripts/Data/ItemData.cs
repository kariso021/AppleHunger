using UnityEngine;
[CreateAssetMenu(fileName = "ItemData", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    public int itemId;       // ������ �������� ���� ID
    public int playerId;     // �÷��̾� ID
    public int itemUniqueId; // ���� �� �������� ���� ID , icon�� 101~ , board�� 201~ �� ����
    public string itemType;  // ������ ���� ("icon", "board")
    public bool isUnlocked;  // ������ �ر� ����
    public string acquiredAt; // ������ ȹ�� ��¥ (JSON ��ȯ�� ���� ���ڿ�)
    public Sprite itemSprite; // ������ �̹���
    public Sprite itemLockedImage; // ������ ��� �̹���(������ �̹��� ���� ������)
}
