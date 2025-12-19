using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{

    public string gameSceneToLoad;

    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}
