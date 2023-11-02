using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager instance;
    private GameObject pausePanel;
    private GameObject winPanel;
    private GameObject losePanel;
    private GameObject endPanel;
    private GameObject fadePanel;
    private bool gamePaused = false;
    private bool gameEnded = false;

    private void Awake()
    {
        //Singleton
        instance = this;
    }

    private IEnumerator Start()
    {
        //Delay before start
        yield return new WaitForSeconds(0.05f);

        //Initialize vars
        pausePanel = GameObject.FindGameObjectWithTag("PausePanel");
        winPanel = GameObject.FindGameObjectWithTag("WinPanel");
        losePanel = GameObject.FindGameObjectWithTag("LosePanel");
        endPanel = GameObject.FindGameObjectWithTag("EndPanel");
        fadePanel = GameObject.FindGameObjectWithTag("FadePanel");

        //Hide all panels
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        endPanel.SetActive(false);

        //Fade Out
        StartCoroutine(DoFade(0));

        //Restart TimeScale
        Time.timeScale = 1;
    }

    /// <summary>
    /// Fade in/out
    /// </summary>
    /// <param name="canvasGroup">The canvas group to fade</param>
    /// <param name="start">Starting alpha</param>
    /// <param name="end">Ending alpha</param>
    /// <returns></returns>
    public IEnumerator DoFade(float end, float lerpTime = 0.5f)
    {
        CanvasGroup canvasGroup = fadePanel.GetComponent<CanvasGroup>();
        float start = canvasGroup.alpha;
        float _timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;
        float currentValue = 0;

        while (true)
        {
            timeSinceStarted = Time.time - _timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            currentValue = Mathf.Lerp(start, end, percentageComplete);

            canvasGroup.alpha = currentValue;

            if (percentageComplete >= 1) { break; }

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator RestartLevel(float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loose game panel
    /// </summary>
    public IEnumerator LoseGame()
    {
        gameEnded = true;
        //Delay lose screen
        yield return new WaitForSeconds(1f);

        Time.timeScale = 0;
        PlayerController.instance.Rb.gravityScale = 0;
        losePanel.SetActive(true);
    }

    /// <summary>
    /// Win game panel
    /// </summary>
    public void WinGame()
    {
        gameEnded = true;
        SoundManager.instance.playWinSound();
        if(SceneManager.GetActiveScene().buildIndex == 3)
        {
            endPanel.SetActive(true);
        }
        else
        {
            winPanel.SetActive(true);
        }
        Time.timeScale = 0;
    }

    /// <summary>
    /// Pause game panel
    /// </summary>
    public void TogglePause()
    {
        if (!gameEnded)
        {
            if (gamePaused)
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0;
            }
            gamePaused = !gamePaused;
        }
    }

    /// <summary>
    /// Change the scene by name
    /// </summary>
    /// <param name="sceneName">Name of the new scene</param>
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

// Public type of damage
public enum DamageType
{
    piercing, bludgeoning, slashing, worldFall
}
