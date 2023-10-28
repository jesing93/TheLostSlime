using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private GameObject fadePanel;

    private IEnumerator Start()
    {
        //Delay before start
        yield return new WaitForSeconds(0.05f);

        //Initialize
        fadePanel = GameObject.FindGameObjectWithTag("FadePanel");

        //Fade Out
        StartCoroutine(DoFade(0));
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void OnNewGame()
    {
        //Fade In
        StartCoroutine(DoFade(1));
        StartCoroutine(LoadScene(1));
    }

    /// <summary>
    /// Exit the game
    /// </summary>
    public void OnExitGame()
    {
        //Fade In
        StartCoroutine(DoFade(1));
        StartCoroutine(ExitGame());
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

    private IEnumerator LoadScene(int sceneId, float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneId);
    }
    private IEnumerator ExitGame(float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();
    }
}
