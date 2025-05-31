using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{

    static bool isFirstEnter = true;
    private void Awake()
    {
    }

    private void Start()
    {
        if (isFirstEnter)
        {
            isFirstEnter = false;
            Debug.Log("제일 처음");
        }
        else
        {
            Debug.Log("로비 매니저 데이터 있음");
            StartCoroutine(loadDataForFrame());
        }
    }

    private IEnumerator loadDataForFrame()
    {
        yield return new WaitForSeconds(0.1f);

        SQLiteManager.Instance.LoadAllData();
    }
}
