using Google.Play.AppUpdate;
using Google.Play.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://developer.android.com/guide/playcore/in-app-updates/unity?hl=ko <= docs
public class GoogleUpdateManager : MonoBehaviour
{ 

    AppUpdateManager appUpdateManager;
    // Start is called before the first frame update
    void Awake()
    {
        appUpdateManager = new AppUpdateManager();
    }

    private IEnumerator CheckForUpdate()
    {
        if(appUpdateManager == null)
        {
            Debug.Log("[GoogleUpdateManager] AppUpdateManager is NULL");
            yield break;
        } 

        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
          appUpdateManager.GetAppUpdateInfo();

        // Wait until the asynchronous operation completes.
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();
            var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
            // Check AppUpdateInfo's UpdateAvailability, UpdatePriority,
            // IsUpdateTypeAllowed(), ... and decide whether to ask the user
            // to start an in-app update.

            if (appUpdateInfoResult == null || appUpdateOptions == null) {
                Debug.Log("[GoogleUpdateManager] which result or options is NULL");
                yield break;
            }

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable &&
                 appUpdateInfoResult.IsUpdateTypeAllowed(appUpdateOptions))
            {
                showGoogleUpdatePopup(appUpdateInfoResult, appUpdateOptions);
            }


        }
        else
        {
            Debug.LogError($"[AppUpdate] Failed to get update info. Error: {appUpdateInfoOperation.Error}");
        }
    }

    public IEnumerator StartCheckGoogleUpdate()
    {
        yield return StartCoroutine(CheckForUpdate());
    }

    private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfoResult, AppUpdateOptions appUpdateOptions)
    {
        // Creates an AppUpdateRequest that can be used to monitor the
        // requested in-app update flow.
        var startUpdateRequest = appUpdateManager.StartUpdate(
          // The result returned by PlayAsyncOperation.GetResult().
          appUpdateInfoResult,
          // The AppUpdateOptions created defining the requested in-app update
          // and its parameters.
          appUpdateOptions);
        yield return startUpdateRequest;

        // If the update completes successfully, then the app restarts and this line
        // is never reached. If this line is reached, then handle the failure (for
        // example, by logging result.Error or by displaying a message to the user).
    }

    private void showGoogleUpdatePopup(AppUpdateInfo appUpdateInfoResult, AppUpdateOptions appUpdateOptions)
    {
        PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
        PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "새로운 업데이트가 있습니다! 스토어로 이동합니다!";
        PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.gameObject.SetActive(false);
        PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
        PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_touchEvent.enabled = true;
        PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_touchEvent.onClick.AddListener(() => 
        {
            StartCoroutine(StartImmediateUpdate(appUpdateInfoResult, appUpdateOptions));
            //PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_touchEvent.enabled = false;
        });
    }
}
