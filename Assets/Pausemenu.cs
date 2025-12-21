using UnityEngine;

public class Pausemenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public SlowTime slowTime;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            slowTime.slowTime = !pauseMenu.activeSelf;
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }
    }
}
