using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    //Components
    [SerializeField]
    private GameObject fadePanel;

    private void Start()
    {
        StartCoroutine(StartSequence());
    }

    /// <summary>
    /// Start sequence function
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartSequence()
    {
        //Delay before start
        yield return new WaitForSeconds(0.05f);

        //Fade Out
        StartCoroutine(DoFade(0, 1f));

        yield return new WaitForSeconds(3f);

        //Fade In
        StartCoroutine(DoFade(1, 1f));

        yield return new WaitForSeconds(1.5f);

        //Open main menu scene
        SceneManager.LoadScene(1);
    }

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
}
