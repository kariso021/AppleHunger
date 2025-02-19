using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    private void OnEnable()
    {
        // 현재 활성화 되어있는 씬에서 Profile 컴포넌트를 가진 개체를 찾음
        Profile enabledProfile = FindFirstObjectByType<Profile>();

        if (enabledProfile == null) return;

        //enabledProfile.SetProfile(정보들);

    }
}
