using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{

    public string gameSceneToLoad;

    public GameObject nextPage;

    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowNextPage()
    {
        if (nextPage != null)
        {
            nextPage.SetActive(true);
        }
    }
    
}
