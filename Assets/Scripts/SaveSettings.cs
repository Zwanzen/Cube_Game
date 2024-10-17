using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSettings : MonoBehaviour
{
    public bool music = false;

    private AudioSource musicSource;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();

        GameObject[] objs = GameObject.FindGameObjectsWithTag("settings");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            if(!music)
            {
                music = true;
            }
            else
            {
                music = false;
            }
        }

        if(music)
        {
            musicSource.volume = 0.05f;
        }
        else
        {
            musicSource.volume = 0f;
        }
    }

    
}
