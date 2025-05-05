using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AndroidInputManager : MonoBehaviour
{
    private static AndroidInputManager instance;
    public static AndroidInputManager Instance => instance;

    private float lastBackPressedTime;
    private float backPressInterval = 2f; // 2초 이내 두 번 누르면 종료

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Android 뒤로가기
        {
            Debug.Log("[Android] return input");
            if (PopupManager.Instance != null && PopupManager.Instance.activePopup != null)
            {
                // 팝업이 열려 있으면 팝업 닫기
                PopupManager.Instance.ClosePopup();
                Debug.Log("[Android] return input to CLOSE POPUP");
            }
            else if(SceneManager.GetActiveScene().name == "TestInGame")
            {
                PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "싱글 플레이를 그만두고 로비로 돌아가시겠습니까?";
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => SceneManager.LoadScene("TestLobby"));
                Debug.Log("[Android] return input to SHOW POPUPnsfhe");
            }
            else
            {
                // 팝업이 없을 때 → 종료 로직
                PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "정말 게임을 종료하시겠습니까?";
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => Application.Quit());
                Debug.Log("[Android] return input to SHOW POPUPnsfhe");
                //if (Time.time - lastBackPressedTime < backPressInterval)
                //{
                //    // 2초 이내 두 번째 누름 → 게임 종료
                //    Application.Quit();
                //}
                //else
                //{
                //    // 첫 번째 누름 → Toast로 경고 문구 띄우기
                //    AndroidToast.ShowToast("뒤로가기 버튼을 한 번 더 누르면 종료됩니다.");
                //    lastBackPressedTime = Time.time;
                //}
            }
        }
    }

    public void RestartApp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    using (var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
    {
        string packageName = currentActivity.Call<string>("getPackageName");
        AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
        launchIntent.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK

        currentActivity.Call("startActivity", launchIntent);

        currentActivity.Call("finishAffinity"); // 현재 액티비티 스택 종료
    }
#endif
    }

}

public static class AndroidToast
{
    public static void ShowToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                    toast.Call("show");
                }));
            }
        }
#endif
    }
}
