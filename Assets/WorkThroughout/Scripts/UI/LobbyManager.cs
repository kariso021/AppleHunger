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
            isFirstEnter = false;
        else
        {
            Debug.Log("로비 매니저 데이터 있음");
            SQLiteManager.Instance.LoadAllData();
        }
    }
}
