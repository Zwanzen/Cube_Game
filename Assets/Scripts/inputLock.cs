
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputLock : MonoBehaviour
{
    [SerializeField]
    private int framerate = 60;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = framerate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
