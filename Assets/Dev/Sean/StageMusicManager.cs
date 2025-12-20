using System.Collections;
using UnityEngine;

public class StageMusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip intro;
    public AudioClip loop;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(switchToLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator switchToLoop()
    {
        yield return new WaitForSeconds(42f);
        audioSource.clip = loop;
        audioSource.loop = true;
        audioSource.Play();
    }
}
