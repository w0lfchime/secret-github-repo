using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TimerScript : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float startingTime = 60f;
    [SerializeField] private float endingTime = 300f;
    [SerializeField] private string sceneToLoad = "WinScene";
    
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI timerText;
    
    private float currentTime;
    private bool timerEnded = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetTimer();
    }

    // Update is called once per frame
    void Update()
    {
        // Only update timer if game is not paused and timer hasn't ended
        if (Time.timeScale > 0 && !timerEnded)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
            
            // Check if timer has reached the ending time
            if (currentTime >= endingTime)
            {
                OnTimerEnd();
            }
        }
    }
    
    private void ResetTimer()
    {
        currentTime = startingTime;
        UpdateTimerDisplay();
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    private void OnTimerEnd()
    {
        timerEnded = true;
        Debug.Log("Timer ended! Loading scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
    
    public float GetCurrentTime()
    {
        return currentTime;
    }
}
