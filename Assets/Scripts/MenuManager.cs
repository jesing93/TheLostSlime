using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] stomachBullets;

    /// <summary>
    /// Unpause the game
    /// </summary>
    public void OnContinue()
    {
        GameManager.instance.TogglePause();
    }

    /// <summary>
    /// Load next level
    /// </summary>
    public void OnNextLevel()
    {
        Time.timeScale = 1;
        //Fade In
        StartCoroutine(GameManager.instance.DoFade(1));

        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
    }

    /// <summary>
    /// Restart current level
    /// </summary>
    public void OnRestartLevel()
    {
        Time.timeScale = 1;
        //Fade In
        StartCoroutine(GameManager.instance.DoFade(1));

        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void OnBackToMenu()
    {
        Time.timeScale = 1;
        //Fade In
        StartCoroutine(GameManager.instance.DoFade(1));

        StartCoroutine(LoadScene(0));
    }

    private IEnumerator LoadScene(int sceneId, float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneId);
    }

    public void UdateStomachUI(int capacity)
    {
        for(int i = 0; i < 3; i++)
        {
            if (i < capacity)
            {
                stomachBullets[i].GetComponent<Image>().tintColor = Color.white;
            }
            else
            {
                stomachBullets[i].GetComponent<Image>().tintColor = Color.black;
            }
        }
    }
}
