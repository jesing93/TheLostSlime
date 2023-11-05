using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UIElements;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    //Singleton
    public static MenuManager instance;

    //Variables
    [SerializeField]
    private GameObject[] stomachBullets;

    private void Awake()
    {
        //Singleton
        instance = this;
    }

    /// <summary>
    /// Unpause the game
    /// </summary>
    public void OnContinue()
    {
        SoundManager.instance.playButtonClick();
        GameManager.instance.TogglePause();
    }

    /// <summary>
    /// Load next level
    /// </summary>
    public void OnNextLevel()
    {
        SoundManager.instance.playButtonClick();
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
        SoundManager.instance.playButtonClick();
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
        SoundManager.instance.playButtonClick();
        Time.timeScale = 1;
        //Fade In
        StartCoroutine(GameManager.instance.DoFade(1));

        StartCoroutine(LoadScene(1));
    }

    private IEnumerator LoadScene(int sceneId, float delay = 0.5f)
    {
        SoundManager.instance.playButtonClick();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneId);
    }

    public void UpdateStomachUI(int capacity, bool add)
    {
        if (add)
        {
            stomachBullets[capacity - 1].GetComponent<Image>().color = Color.white;
        }
        else
        {
            stomachBullets[capacity].GetComponent<Image>().color = Color.black;
        }
    }
}
