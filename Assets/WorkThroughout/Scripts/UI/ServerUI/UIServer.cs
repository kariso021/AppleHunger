using UnityEngine;
using Unity.Netcode;

public class UIServer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 서버에서만 실행되도록 보장
        if (!IsServer)
        {
            enabled = false; // 🛑 서버가 아니면 비활성화
            return;
        }
    }
}
