using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;
using FishNet;

public class NetworkDragBoxManager : NetworkBehaviour
{
    public GameObject networkDragBoxPrefab;
    private Dictionary<int, NetworkObject> networkDragBoxes = new Dictionary<int, NetworkObject>(); // 클라이언트별 DragBox 관리

    // 서버에서 DragBox 생성
    [ServerRpc(RequireOwnership = false)]
    public void SendDragStartServerRpc(Vector2 startPos, NetworkConnection conn)
    {
        if (networkDragBoxes.ContainsKey(conn.ClientId)) return; // 이미 생성된 경우 방지

        GameObject newBox = Instantiate(networkDragBoxPrefab, startPos, Quaternion.identity, this.transform);
        NetworkObject newBoxNetObj = newBox.GetComponent<NetworkObject>();
        Spawn(newBoxNetObj, conn); // ✅ 해당 클라이언트가 소유하도록 설정

        networkDragBoxes[conn.ClientId] = newBoxNetObj;
        Debug.Log($"[Server] Spawned DragBox for ClientId: {conn.ClientId}");

        // 모든 클라이언트에게 DragBox 생성 요청
        SendDragStartObserversRpc(startPos, conn.ClientId);
    }

    // 클라이언트에게 DragBox 생성 요청 (자신 제외)
    [ObserversRpc(BufferLast = true)]
    private void SendDragStartObserversRpc(Vector2 startPos, int clientId)
    {
        int localClientId = InstanceFinder.ClientManager.Connection.ClientId;
        if (localClientId == clientId) return; // 자기 자신이면 무시

        if (!networkDragBoxes.ContainsKey(clientId))
        {
            GameObject newBox = Instantiate(networkDragBoxPrefab, startPos, Quaternion.identity, this.transform);
            NetworkObject newBoxNetObj = newBox.GetComponent<NetworkObject>();

            networkDragBoxes[clientId] = newBoxNetObj;
            Debug.Log($"[Client] Created DragBox for ClientId: {clientId}");
        }
    }

    // 서버에서 DragBox 업데이트
    [ServerRpc(RequireOwnership = false)]
    public void SendDragUpdateServerRpc(Vector2 startPos, Vector2 endPos, NetworkConnection conn)
    {
        if (!networkDragBoxes.ContainsKey(conn.ClientId)) return;

        Vector2 center = (startPos + endPos) / 2;
        Vector2 size = new Vector2(Mathf.Abs(endPos.x - startPos.x), Mathf.Abs(endPos.y - startPos.y));

        networkDragBoxes[conn.ClientId].transform.position = center;
        networkDragBoxes[conn.ClientId].transform.localScale = new Vector3(size.x, size.y, 1);

        //모든 클라이언트에게 DragBox 업데이트 요청
        SendDragUpdateObserversRpc(center, size, conn.ClientId);
    }

    // 클라이언트에게 DragBox 업데이트 요청 (자신 제외)
    [ObserversRpc(BufferLast = true)]
    private void SendDragUpdateObserversRpc(Vector2 center, Vector2 size, int clientId)
    {
        int localClientId = InstanceFinder.ClientManager.Connection.ClientId;
        if (localClientId == clientId) return; // 자기 자신이면 무시

        if (networkDragBoxes.TryGetValue(clientId, out NetworkObject box))
        {
            box.transform.position = center;
            box.transform.localScale = new Vector3(size.x, size.y, 1);
        }
    }

    // 서버에서 DragBox 제거
    [ServerRpc(RequireOwnership = false)]
    public void SendDragEndServerRpc(NetworkConnection conn)
    {
        if (!networkDragBoxes.ContainsKey(conn.ClientId)) return;

        Despawn(networkDragBoxes[conn.ClientId]);
        networkDragBoxes.Remove(conn.ClientId);

        // ✅ 모든 클라이언트에게 DragBox 제거 요청
        SendDragEndObserversRpc(conn.ClientId);
    }

    // 클라이언트에게 DragBox 제거 요청 (자신 제외)
    [ObserversRpc(BufferLast = true)]
    private void SendDragEndObserversRpc(int clientId)
    {
        int localClientId = InstanceFinder.ClientManager.Connection.ClientId;
        if (localClientId == clientId) return; // 자기 자신이면 무시

        if (networkDragBoxes.ContainsKey(clientId))
        {
            Destroy(networkDragBoxes[clientId].gameObject);
            networkDragBoxes.Remove(clientId);
        }
    }
}
