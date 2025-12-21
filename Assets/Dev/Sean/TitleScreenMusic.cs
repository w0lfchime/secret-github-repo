using System.Collections;
using UnityEngine;

public class TitleScreenMusic : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip loop;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(startLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator startLoop()
    {
        yield return new WaitForSeconds(2f);
        audioSource.clip = loop;
        audioSource.loop = true;
        audioSource.Play();
    }
}
