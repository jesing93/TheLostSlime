using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerController : MonoBehaviour
{
    #region Variables;
    //Components
    private Rigidbody2D rb;
    private SpriteRenderer playerSprite;
    private GameObject playerFoot; //For ground check
    private Animator playerAnimator;
    private GameObject swallowPoint; //For swallowing check
    private float swallowRange = 0.5f;

    //Layers
    private LayerMask groundLayer;

    //Move
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 5;
    private float ySpeed;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    private bool isJumping;
    private bool isFalling;
    private bool isBusy; //Bool to block other actions while doing specific actions

    [SerializeField]
    private float jumpButtonGracePeriod;

    //Inputs
    private float vAxis = 0;
    private float hAxis = 0;
    private bool jumpInput = false;
    private bool swallowInput = false;
    private bool spitInput = false;

    //Jump
    //private float jumpSpeed;
    [SerializeField]
    private float jumpForce = 5f;
    private Vector3 jumpVector = Vector3.zero;

    //Data
    private int stomachCapacity = 3;
    private List<GameObject> stomachItems = new List<GameObject>();

    #endregion;
    private void Awake()
    {
        //Initialization
        rb = GetComponent<Rigidbody2D>();
        playerSprite = GameObject.Find
            ("PlayerSprite").GetComponent<SpriteRenderer>();
        playerFoot = GameObject.Find("PlayerFoot");
        groundLayer = LayerMask.GetMask("Ground");
        playerAnimator = GetComponentInChildren<Animator>();
        swallowPoint = GameObject.Find("SwallowPoint");
    }

    private void Start()
    {

    }

    private void Update()
    {
        GetInputs();
        Move();
        Swallow();
        Spit();
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
        //Movement inputs
        vAxis = Input.GetAxisRaw("Vertical");
        hAxis = Input.GetAxisRaw("Horizontal");

        //Action inputs
        jumpInput = Input.GetKeyDown(KeyCode.W);
        swallowInput = Input.GetKeyDown(KeyCode.E);
        spitInput = Input.GetKeyDown(KeyCode.Q);
    }

    /// <summary>
    /// Change the Player position with axis and speed
    /// </summary>
    private void Move()
    {
        //Only muve when not doing something else
        if (!isBusy)
        {
            rb.velocity = new Vector2(hAxis * moveSpeed, rb.velocity.y);
        }
    }

    /// <summary>
    /// Update the states of the animation
    /// </summary>
    private void AnimUpdate()
    {
        if (!isBusy && !isJumping)
        {
            //Update animation to idle
            if (hAxis == 0)
            {
                playerAnimator.SetBool("isMoving", false);
            }
            else
            {
                //Flip sprite and swallow point
                if (hAxis > 0)
                {
                    playerSprite.flipX = false;
                    swallowPoint.transform.localPosition = new Vector3(
                    0.6f,
                    swallowPoint.transform.localPosition.y,
                    swallowPoint.transform.localPosition.z);
                }
                else if (hAxis < 0)
                {
                    playerSprite.flipX = true;
                    swallowPoint.transform.localPosition = new Vector3(
                    -0.6f,
                    swallowPoint.transform.localPosition.y,
                    swallowPoint.transform.localPosition.z);

                }

                //Update animation to moving
                playerAnimator.SetBool("isMoving", true);
            }

            //Change falling state and animation if grounded and viceversa
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
        return Physics2D.Raycast(playerFoot.transform.position, Vector3.down, 0.5f, groundLayer);
    }

    /// <summary>
    /// Player jump
    /// </summary>
    private void Jump()
    {
        if (jumpInput && !isFalling && !isBusy && !isJumping)
        {
            playerAnimator.SetTrigger("jump");
            isJumping = true;
            //Delay the jump after the animation started
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
        //Only swallow if not doing something else
        if (swallowInput && !isFalling && !isJumping && !isBusy)
        {
            isBusy = true;
            playerAnimator.SetTrigger("swallow");
            //Disable busy once animation finish
            StartCoroutine(EndSwallow());

            //Shearch if there is something to swallow
            GameObject item = SearchItemToSwallow();
            if (item != null)
            {
                //If found something check if there is space in the stomach
                if (stomachItems.Count < stomachCapacity)
                {
                    //Actually swallow the item after the animation started
                    StartCoroutine(SwallowItem(item));
                }

            }
        }
    }

    /// <summary>
    /// Detect swallowables in range
    /// </summary>
    private GameObject SearchItemToSwallow()
    {
        //Collider to check items in range
        Collider2D[] items = Physics2D.OverlapCircleAll(swallowPoint.transform.position, swallowRange, groundLayer);
        Collider2D obstacle = null, powerUp = null;
        foreach (Collider2D item in items)
        {
            //Check type of item to swallow
            if(item.CompareTag("Obstacle"))
            {
                obstacle = item;
            }
            else if (item.CompareTag("PowerUp"))
            {
                powerUp = item;
            }
        }

        //Prioritize power ups in area adn return only one GameObject
        if (powerUp != null)
        {
            return powerUp.GameObject();
        }
        else if (obstacle != null)
        {
            return obstacle.GameObject();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Delay before swallowing
    /// </summary>
    private IEnumerator SwallowItem(GameObject item)
    {
        //Animation delay
        yield return new WaitForSeconds(0.4f);

        //Save item for later use
        item.SetActive(false);
        stomachItems.Add(item);
    }

    /// <summary>
    /// Delay after swallowing
    /// </summary>
    private IEnumerator EndSwallow()
    {
        //Animation delay
        yield return new WaitForSeconds(1f);

        isBusy = false;
    }

    /// <summary>
    /// Player swallow skill
    /// </summary>
    private void Spit()
    {
        //Only spit if not doing something else
        if (spitInput && !isFalling && !isJumping && !isBusy)
        {
            isBusy = true;
            playerAnimator.SetTrigger("spit");
            //Use the same corroutine that swallow to disable the busy state
            StartCoroutine(EndSwallow());

            //If there is items in the stomach we spit the last one
            if (stomachItems.Count > 0)
            {
                //Coroutine to spit the item once the animation started
                StartCoroutine(SpitItem());
            }
        }
    }

    /// <summary>
    /// Delay before swallowing
    /// </summary>
    private IEnumerator SpitItem()
    {
        Vector3 position = swallowPoint.transform.position;
        //Animation delay
        yield return new WaitForSeconds(0.4f);

        //Get the last swallowed item
        GameObject item = stomachItems.Last();
        //Remove it from stomach
        stomachItems.Remove(item);

        //Depending on the orientation we spawn it at the left or the right
        if (playerSprite.flipX)
        {
            position = new Vector3(
            position.x - 1f,
            swallowPoint.transform.localPosition.y,
            swallowPoint.transform.localPosition.z);
        }
        else
        {
            position = new Vector3(
            position.x + 1f,
            position.y,
            position.z);
        }
        
        item.transform.position = position;
        item.SetActive(true);
    }
}
