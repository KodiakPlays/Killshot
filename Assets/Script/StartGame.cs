using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clip;

    public void StartGameButton()
    {
        OnClick();
        Invoke("loadScene", 0.5f);
    }
    void loadScene()
    {
        SceneManager.LoadScene("GameScene3");
    }
    public void OnClick()
    {
        audioSource.clip = clip;
        audioSource.Play();
        audioSource.loop = false;
    }
}
