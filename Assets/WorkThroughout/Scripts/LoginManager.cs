using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    private DownManager downManager;
    // Start is called before the first frame update
    void Start()
    {
        downManager = FindAnyObjectByType<DownManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGuestLogin()
    {

    }

    public void OnGoogleLogin()
    {

    }
}
