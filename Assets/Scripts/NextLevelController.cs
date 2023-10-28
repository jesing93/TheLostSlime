using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelController : MonoBehaviour
{
    /// <summary>
    /// On player enter: win the game
    /// </summary>
    /// <param name="collision">Only detects the "Player" tag</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.instance.WinGame();
        }
    }
}
