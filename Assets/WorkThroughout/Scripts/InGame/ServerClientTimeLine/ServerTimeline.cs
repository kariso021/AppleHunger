using Unity.Netcode;
using UnityEngine;

public class ServerTimeline : MonoBehaviour
{
    private void Start()
    {
        // 서버로 실행된 경우 클라이언트 시작을 막는다
        if (IsRunningAsServer()) return;

        StartClient();
    }

    private void StartClient()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    private bool IsRunningAsServer()
    {
        // 서버로 실행 중이거나, 호스트(서버+클라이언트)일 경우
        return Application.isBatchMode || NetworkManager.Singleton.IsServer;
    }
}
