using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    private void Start()
    {
        if(SQLiteManager.Instance.player != null)
            SQLiteManager.Instance.LoadAllData();
    }
}
