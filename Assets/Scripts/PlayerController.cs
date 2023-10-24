using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables;
    //Components
    private Rigidbody2D rb;
    private SpriteRenderer playerSprite;
    private GameObject playerFoot; //For ground check
    private Animator playerAnimator;

    //Ground Check
    private LayerMask layer;

    //Move
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 5;
    private float ySpeed;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    private bool isJumping;
    private bool isFalling;
    private bool isSwallowing;

    [SerializeField]
    private float jumpButtonGracePeriod;

    //Inputs
    private float vAxis = 0;
    private float hAxis = 0;
    private bool jumpInput = false;
    private bool swallowInput = false;

    //Jump
    //private float jumpSpeed;
    [SerializeField]
    private float jumpForce = 5f;
    private Vector3 jumpVector = Vector3.zero;

    #endregion;
    private void Awake()
    {
        //Initialization
        rb = GetComponent<Rigidbody2D>();
        playerSprite = GameObject.Find
            ("PlayerSprite").GetComponent<SpriteRenderer>();
        playerFoot = GameObject.Find("PlayerFoot");
        layer = LayerMask.GetMask("Ground");
        playerAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {

    }

    private void Update()
    {
        GetInputs();
        Move();
        Swallow();
        Jump();
    }

    private void FixedUpdate()
    {
        AnimUpdate();
    }

    /// <summary>
    /// Read the Vertical and Horizontal Axis from Input
    /// </summary>
    private void GetInputs()
    {
        vAxis = Input.GetAxisRaw("Vertical");
        hAxis = Input.GetAxisRaw("Horizontal");

        jumpInput = Input.GetKeyDown(KeyCode.W);
        swallowInput = Input.GetKeyDown(KeyCode.E);
    }

    /// <summary>
    /// Change the Player position with axis and speed
    /// </summary>
    private void Move()
    {
        if (!isSwallowing)
        {
            rb.velocity = new Vector2(hAxis * moveSpeed, rb.velocity.y);
        }
    }

    /// <summary>
    /// Update the states of the animation
    /// </summary>
    private void AnimUpdate()
    {
        if (!isSwallowing && !isJumping)
        {
            //Update moving state
            if (hAxis == 0)
            {
                playerAnimator.SetBool("isMoving", false);
            }
            else
            {
                //Flip sprite
                if (hAxis > 0)
                {
                    playerSprite.flipX = false;
                }
                else if (hAxis < 0)
                {
                    playerSprite.flipX = true;
                }

                playerAnimator.SetBool("isMoving", true);
            }

            if (IsGrounded() && isFalling)
            {
                isFalling = false;
                playerAnimator.SetBool("isFalling", false);
            }
            else if (!IsGrounded() && !isFalling)
            {
                isFalling = true;
                playerAnimator.SetBool("isFalling", true);
            }
        }
    }

    /// <summary>
    /// Check if the Player is on the ground
    /// </summary>
    /// <returns>Is grounded</returns>
    private bool IsGrounded()
    {
        return Physics2D.Raycast(playerFoot.transform.position, Vector3.down, 0.5f, layer);
    }

    /// <summary>
    /// Player jump
    /// </summary>
    private void Jump()
    {
        if (jumpInput && !isFalling && !isSwallowing && !isJumping)
        {
            playerAnimator.SetTrigger("jump");
            isJumping = true;
            StartCoroutine(JumpDelayed());
        }
    }

    /// <summary>
    /// Delay for the jump skill
    /// </summary>
    private IEnumerator JumpDelayed()
    {
        //Animation delay
        yield return new WaitForSeconds(0.16f);

        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isJumping = false;
    }

    /// <summary>
    /// Player swallow skill
    /// </summary>
    private void Swallow()
    {
        if (swallowInput && !isFalling && !isJumping)
        {
            isSwallowing = true;
            playerAnimator.SetTrigger("swallow");
            StartCoroutine(EndSwallow());
        }
    }

    /// <summary>
    /// Delay for the swallowing skill
    /// </summary>
    private IEnumerator EndSwallow()
    {
        //Animation delay
        yield return new WaitForSeconds(1.3f);

        isSwallowing = false;
    }
}
