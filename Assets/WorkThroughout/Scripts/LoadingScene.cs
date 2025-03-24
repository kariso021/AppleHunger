using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public static string nextScene;

    public Slider loadingBar;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartLoadingScene());
    }

    IEnumerator StartLoadingScene()
    {
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = true;

        float timer = 0f;

        while (!op.isDone)
        {
            yield return null;

            timer += Time.deltaTime;

            if(op.progress < 0.9f)
            {
                loadingBar.value = Mathf.Lerp(loadingBar.value, op.progress, timer);

                if(loadingBar.value >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                loadingBar.value = Mathf.Lerp(loadingBar.value, 1f, timer);

                if(loadingBar.value == 1f)
                {
                    yield return new WaitForSeconds(2f);
                    NavManager.currentScene = nextScene;
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

}
