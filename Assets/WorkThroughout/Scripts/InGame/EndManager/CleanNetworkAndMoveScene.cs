using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CleanNetworkAndMoveScene : MonoBehaviour
{

    public GameObject NObj;
    public void CleanNetwork()
    {
        Destroy(NObj);
        SceneManager.LoadScene("Lobby");
    }
}
