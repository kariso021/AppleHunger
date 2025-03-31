using Unity.Netcode;
using UnityEngine;

public class ServerTimeline : MonoBehaviour
{
    private void Start()
    {
        // ������ ����� ��� Ŭ���̾�Ʈ ������ ���´�
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
        // ������ ���� ���̰ų�, ȣ��Ʈ(����+Ŭ���̾�Ʈ)�� ���
        return Application.isBatchMode || NetworkManager.Singleton.IsServer;
    }
}
