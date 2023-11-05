using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource asMenuMusic;
    [SerializeField]
    private AudioSource asGameMusic;
    [SerializeField]
    private AudioSource asWin;
    [SerializeField]
    private AudioSource asButton;

    public static SoundManager instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += ChangeScene;
    }

    private void ChangeScene(Scene current, Scene next)
    {
        if (next.buildIndex == 0)
        {
            asGameMusic.Stop();
            asMenuMusic.Play();
        }
        else
        {
            asMenuMusic.Stop();
            asGameMusic.Play();
        }
    }

    public void playWinSound()
    {
        asGameMusic.Stop();
        asWin.Play();
    }

    public void playButtonClick()
    {
        asButton.Play();
    }
}
