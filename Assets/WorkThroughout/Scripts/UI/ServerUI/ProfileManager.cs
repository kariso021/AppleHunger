using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    private void OnEnable()
    {
        // ���� Ȱ��ȭ �Ǿ��ִ� ������ Profile ������Ʈ�� ���� ��ü�� ã��
        Profile enabledProfile = FindFirstObjectByType<Profile>();

        if (enabledProfile == null) return;

        //enabledProfile.SetProfile(������);

    }
}
