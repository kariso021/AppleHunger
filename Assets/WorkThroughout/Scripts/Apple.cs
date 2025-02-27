using FishNet.Object;
using TMPro;
using UnityEngine;

public class Apple : NetworkBehaviour
{
    private int value;
    private int roomId;
    private int scorevalue;

    public int Value => value;
    public int ScoreValue => scorevalue;
    public int RoomId => roomId;

    [SerializeField] private TextMeshPro numberText;

    /// <summary>
    /// Apple을 Room ID와 함께 초기화
    /// </summary>
    public void Initialize(int assignedRoomId, int assignedValue)
    {
        roomId = assignedRoomId;
        value = assignedValue;

        Debug.Log($"🍏 Apple Initialized - RoomID: {roomId}, Value: {value}");

        UpdateText();
    }

    private void UpdateText()
    {
        if (numberText != null)
        {
            numberText.text = value.ToString();
        }
        else
        {
            Debug.LogError("🚨 numberText가 할당되지 않았습니다! Inspector에서 확인하세요.");
        }
    }
}
