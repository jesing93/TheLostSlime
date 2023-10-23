using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager instance;

    private void Awake()
    {
        //Singleton
        instance = this;

        //Restart TimeScale
        Time.timeScale = 1;
    }

    private IEnumerator Start()
    {
        //Delay before start
        yield return new WaitForSeconds(0.05f);
    }
}
