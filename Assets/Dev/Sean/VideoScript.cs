using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(loadTheScene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator loadTheScene()
    {
        yield return new WaitForSeconds(20.5f);
        Debug.Log("Video over");
        LoadScene();
    }
    public void LoadScene()
    {
        SceneManager.LoadScene("H2P_Lore");
    }
}
