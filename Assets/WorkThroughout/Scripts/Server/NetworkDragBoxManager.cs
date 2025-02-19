using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;
using FishNet;

public class NetworkDragBoxManager : NetworkBehaviour
{
    // ✅ 미리 생성된 DragBox를 참조
    public GameObject networkDragBox;

    // ✅ 1️⃣ 서버에서 DragBox 시작 요청
    [ServerRpc(RequireOwnership = false)]
    public void SendDragStartServerRpc(Vector2 startPos, NetworkConnection conn)
    {
        Debug.Log($"[Server] Activating DragBox for ClientId: {conn.ClientId}");
        SendDragStartObserversRpc(startPos, conn.ClientId);
    }

    // ✅ 2️⃣ 클라이언트에서 DragBox 활성화
    [ObserversRpc(BufferLast = true)]
    private void SendDragStartObserversRpc(Vector2 startPos, int clientId)
    {
        int localClientId = InstanceFinder.ClientManager.Connection.ClientId;
        if (localClientId != clientId) // 자신이면 무시
        {
            networkDragBox.SetActive(true);
            networkDragBox.transform.position = startPos;
            networkDragBox.transform.localScale = Vector3.zero; // 초기 크기
            Debug.Log($"[Client] Activated DragBox for ClientId: {clientId}");
        }
    }

    // ✅ 3️⃣ 서버에서 DragBox 업데이트 요청
    [ServerRpc(RequireOwnership = false)]
    public void SendDragUpdateServerRpc(Vector2 startPos, Vector2 endPos, NetworkConnection conn)
    {
        Vector2 center = (startPos + endPos) / 2;
        Vector2 size = new Vector2(Mathf.Abs(endPos.x - startPos.x), Mathf.Abs(endPos.y - startPos.y));

        Debug.Log($"[Server] Updating DragBox for ClientId: {conn.ClientId}");

        SendDragUpdateObserversRpc(center, size, conn.ClientId);
    }

    // ✅ 4️⃣ 클라이언트에서 DragBox 위치/크기 업데이트
    [ObserversRpc(BufferLast = true)]
    private void SendDragUpdateObserversRpc(Vector2 center, Vector2 size, int clientId)
    {
        int localClientId = InstanceFinder.ClientManager.Connection.ClientId;
        if (localClientId != clientId) // 자신이면 무시
        {
            networkDragBox.transform.position = center;
            networkDragBox.transform.localScale = new Vector3(size.x, size.y, 1);
            Debug.Log($"[Client] Updated DragBox for ClientId: {clientId}");
        }
    }

    // ✅ 5️⃣ 서버에서 DragBox 비활성화 요청
    [ServerRpc(RequireOwnership = false)]
    public void SendDragEndServerRpc(NetworkConnection conn)
    {
        Debug.Log($"[Server] Deactivating DragBox for ClientId: {conn.ClientId}");
        SendDragEndObserversRpc(conn.ClientId);
    }

    // ✅ 6️⃣ 클라이언트에서 DragBox 비활성화
    [ObserversRpc(BufferLast = true)]
    private void SendDragEndObserversRpc(int clientId)
    {
        int localClientId = InstanceFinder.ClientManager.Connection.ClientId;
        if (localClientId != clientId) // 자신이면 무시
        {
            networkDragBox.SetActive(false);
            Debug.Log($"[Client] Deactivated DragBox for ClientId: {clientId}");
        }
    }
}
