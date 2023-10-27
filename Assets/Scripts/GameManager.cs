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

    public IEnumerator RestartLevel(float delay = 1)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

// Public type of damage
public enum DamageType
{
    piercing, bludgeoning, slashing
}
