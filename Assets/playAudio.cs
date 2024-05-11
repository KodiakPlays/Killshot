using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playAudio : MonoBehaviour
{
    public AudioSource src;
    public AudioClip clip;
    
    void Update()
    {
        if (src.isPlaying)
        {
            Debug.Log("is Playing");
        }
        else
        {
            Debug.Log("is not playing");
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            src.clip = clip;
            src.Play();
        }
        if (Input.GetKeyUp(KeyCode.V))
        {
            src.Stop();
        }
    }
}
