using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{

    public string gameSceneToLoad;

    public GameObject nextPage;
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject optionsPanel;

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
    
    public void ShowCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void BackButton()
    {
        mainPanel.SetActive(true);
        creditsPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }
}
