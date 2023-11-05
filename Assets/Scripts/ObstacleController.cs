using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    //Components
    [SerializeField]
    private AudioSource hitSound;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int[] layers = new[] { LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("Obstacle") };
        if (layers.Contains(collision.collider.gameObject.layer))
        {
            if (rb.velocity.magnitude > 0.8f)
            {
                hitSound.Play();
            }
        }
    }
}
