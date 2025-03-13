using Unity.Netcode;
using UnityEngine;

public class NetworkDragBoxManager : NetworkBehaviour
{
    // 미리 생성된 DragBox를 참조
    public GameObject networkDragBox;

    // 서버에서 DragBox 시작 요청
    [ServerRpc(RequireOwnership = false)]
    public void SendDragStartServerRpc(Vector2 startPos, ulong clientId)
    {
        SendDragStartClientRpc(startPos, clientId);
    }

    // 클라이언트에서 DragBox 활성화
    [ClientRpc]
    private void SendDragStartClientRpc(Vector2 startPos, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) // 자신이면 무시
        {
            networkDragBox.SetActive(true);
            networkDragBox.transform.position = startPos;
            networkDragBox.transform.localScale = Vector3.zero; // 초기
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendDragUpdateServerRpc(Vector2 startPos, Vector2 endPos, ulong clientId)
    {
        Vector2 center = (startPos + endPos) / 2;
        Vector2 size = new Vector2(Mathf.Abs(endPos.x - startPos.x), Mathf.Abs(endPos.y - startPos.y));

        SendDragUpdateClientRpc(center, size, clientId);
    }

    [ClientRpc]
    private void SendDragUpdateClientRpc(Vector2 center, Vector2 size, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) // 자신이면 무시
        {
            networkDragBox.transform.position = center;
            networkDragBox.transform.localScale = new Vector3(size.x, size.y, 1);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendDragEndServerRpc(ulong clientId)
    {
        Debug.Log($"[Server] Deactivating DragBox for ClientId: {clientId}");
        SendDragEndClientRpc(clientId);
    }

    [ClientRpc]
    private void SendDragEndClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) // 자신이면 무시
        {
            networkDragBox.SetActive(false);
            Debug.Log($"[Client] Deactivated DragBox for ClientId: {clientId}");
        }
    }
}
