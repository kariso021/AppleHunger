using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
public class UIServer : NetworkBehaviour
{
    // Start 같은 개념이네
    public override void OnStartServer() // ✅ FishNet의 서버 시작 이벤트 활용
    {
        base.OnStartServer();

        // 서버에서만 실행되도록 보장
        if (!IsServer)
        {
            enabled = false; // 🛑 서버가 아니면 비활성화
            return;
        }
    }
}
