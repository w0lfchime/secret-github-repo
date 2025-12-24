using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{

    public string gameSceneToLoad;

    public GameObject nextPage;
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject optionsPanel;
    public GameObject titleScreen;
    public GameObject charSelect;
    private GameData gameData;

    void Start()
    {
        gameData = GameObject.Find("GameData").GetComponent<GameData>();
    }

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

    public void showCharSelect()
    {
        mainPanel.SetActive(false);
        charSelect.SetActive(true);
    }
    
    public void ShowCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
        optionsPanel.SetActive(false);
        titleScreen.SetActive(false);
    }

    public void BackButton()
    {
        mainPanel.SetActive(true);
        creditsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        charSelect.SetActive(false);
        titleScreen.SetActive(true);
    }

    public void ShowOptions()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(false);
        optionsPanel.SetActive(true);
        titleScreen.SetActive(false);
    }
}
