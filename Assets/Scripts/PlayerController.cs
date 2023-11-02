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
    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer playerSprite;
    private GameObject playerFoot; //For ground check
    private Animator playerAnimator;
    private GameObject swallowPoint; //For swallowing check
    [SerializeField]
    private GameObject puffEffect; //Prefab of the puff effect
    [SerializeField]
    private GameObject teleportEffect; //Prefab of the teleport effect
    //Sounds
    [SerializeField]
    private AudioSource asJump;
    [SerializeField]
    private AudioSource asFall;
    [SerializeField]
    private AudioSource asSwallow;
    [SerializeField]
    private AudioSource asDeath;
    [SerializeField]
    private AudioSource asTeleport;
    [SerializeField]
    private AudioSource asMoving;

    //Layers
    private LayerMask groundLayer;
    private LayerMask obstacleLayer;
    private LayerMask itemLayer;

    //Move
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 5;
    private float ySpeed;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;

    //Inputs
    private float vAxis = 0;
    private float hAxis = 0;
    private bool jumpInput = false;
    private bool swallowInput = false;
    private bool spitInput = false;
    private bool pauseInput = false;
    private bool skillInput = false;

    //Jump
    //private float jumpSpeed;
    [SerializeField]
    private float jumpForce = 5f;
    private Vector3 jumpVector = Vector3.zero;

    //Data
    private int stomachCapacity = 3;
    private List<GameObject> stomachItems = new List<GameObject>();
    [SerializeField]
    private float jumpButtonGracePeriod;
    private float swallowRange = 0.5f;
    private SlimeType slimeType = SlimeType.basic;
    private bool isJumping;
    private bool isFalling;
    private bool isMoving;
    private bool isBusy; //Bool to block other actions while doing specific actions or animations
    private bool isDead;
    private bool skillInCooldown;

    public static PlayerController instance;

    public Rigidbody2D Rb { get => rb; set => rb = value; }

    #endregion;
    private void Awake()
    {
        //Singleton
        instance = this;

        //Initialization
        rb = GetComponent<Rigidbody2D>();
        playerSprite = GameObject.Find
            ("PlayerSprite").GetComponent<SpriteRenderer>();
        playerFoot = GameObject.Find("PlayerFoot");
        groundLayer = LayerMask.GetMask("Ground");
        obstacleLayer = LayerMask.GetMask("Obstacle");
        itemLayer = LayerMask.GetMask("Item");
        playerAnimator = GetComponentInChildren<Animator>();
        swallowPoint = GameObject.Find("SwallowPoint");

        Time.timeScale = 1;
    }

    private void Update()
    {
        GetInputs();
        Move();
        Swallow();
        Spit();
        Jump();
        Skill();
        Pause();
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
        skillInput = Input.GetKeyDown(KeyCode.Space);

        //UI inputs
        pauseInput = Input.GetKeyDown(KeyCode.P);
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    private void Pause()
    {
        if (pauseInput)
        {
            GameManager.instance.TogglePause();
        }
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
        if (!isBusy && !isJumping && !isDead)
        {
            //Update animation to idle
            if (hAxis == 0)
            {
                if (isMoving)
                {
                    isMoving = false;
                    asMoving.Stop();
                    playerAnimator.SetBool("isMoving", false);
                }
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

                if (!isMoving && !isFalling)
                {
                    isMoving = true;
                    asMoving.Play();
                    //Update animation to moving
                    playerAnimator.SetBool("isMoving", true);
                }
                else if (isMoving && isFalling)
                {
                    isMoving = false;
                    asMoving.Stop();
                    playerAnimator.SetBool("isMoving", false);
                }
            }

            //Change falling state and animation if grounded and viceversa
            if (IsGrounded() && isFalling)
            {
                isFalling = false;
                playerAnimator.SetBool("isFalling", false);
                asFall.Play();
            }
            else if (!IsGrounded() && !isFalling)
            {
                isFalling = true;
                playerAnimator.SetBool("isFalling", true);
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                asMoving.Stop();
                playerAnimator.SetBool("isMoving", false);
            }
        }
    }

    /// <summary>
    /// Check if the Player is on the ground
    /// </summary>
    /// <returns>Is grounded</returns>
    private bool IsGrounded()
    {
        bool groundObjects = Physics2D.Raycast(playerFoot.transform.position, Vector3.down, 0.5f, groundLayer | obstacleLayer);
        
        return groundObjects;
    }

    /// <summary>
    /// Player jump
    /// </summary>
    private void Jump()
    {
            if (jumpInput && !isFalling && !isBusy && !isJumping && slimeType == SlimeType.basic)
        {
            isJumping = true;
            asJump.Play();
            playerAnimator.SetTrigger("jump");
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
        if (swallowInput && !isFalling && !isJumping && !isBusy && !isDead)
        {
            isBusy = true;
            asSwallow.Play();
            rb.velocity = Vector2.zero;
            playerAnimator.SetTrigger("swallow");
            //Disable busy once animation finish
            StartCoroutine(EndSwallow());

            //Shearch if there is something to swallow
            GameObject item = SearchItemToSwallow();
            if (item != null)
            {
                if (item.CompareTag("Obstacle"))
                {
                    //If found something check if there is space in the stomach
                    if (stomachItems.Count < stomachCapacity)
                    {
                        //Actually swallow the item after the animation started
                        StartCoroutine(SwallowItem(item));
                    }
                }
                else
                {
                    //If power up type is different than own type
                    SlimeType newType = item.GetComponentInParent<PowerUpController>().Type;
                    if (newType != slimeType)
                    {
                        StartCoroutine(EndSwitch(item, newType));
                    }
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
        Collider2D[] items = Physics2D.OverlapCircleAll(swallowPoint.transform.position, swallowRange, obstacleLayer | itemLayer);
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

        //Prioritize obstacles in area and return only one GameObject
        if (obstacle != null)
        {
            return obstacle.GameObject();
        }
        else if (powerUp != null)
        {
            return powerUp.GameObject();
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

        //Spawn puff effect next to the swallow point
        StartCoroutine(SpawnPuff(swallowPoint.transform.position + (new Vector3(playerSprite.flipX ? -1 : 1, 0, 0))));

        //Hide delay
        yield return new WaitForSeconds(0.1f);
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
    /// Apply switch effects after animation ended
    /// </summary>
    private IEnumerator EndSwitch(GameObject item, SlimeType newType)
    {
        //Switcher power up change delay
        yield return new WaitForSeconds(0.7f);

        //Update switcher power up to my old type
        item.GetComponentInParent<PowerUpController>().updateType(slimeType);

        //Puff effect delay
        yield return new WaitForSeconds(0.25f);

        //Spawn puff effect over the player sprite
        StartCoroutine(SpawnPuff(this.transform.position + new Vector3(0, 0.5f, 0)));

        //Small delay before own change
        yield return new WaitForSeconds(0.1f);

        //Update my own type
        slimeType = newType;

        //Pick the new color id
        int typeId = 0;
        switch (slimeType)
        {
            case SlimeType.teleport:
                typeId = 1;
                break;
        }
        //Update animator
        playerAnimator.SetInteger("type", typeId);
        playerAnimator.SetTrigger("switchType");
    }

    /// <summary>
    /// Player spit skill
    /// </summary>
    private void Spit()
    {
        //Only spit if not doing something else
        if (spitInput && !isFalling && !isJumping && !isBusy && !isDead)
        {
            isBusy = true;
            asSwallow.Play();
            rb.velocity = Vector2.zero;
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
        yield return new WaitForSeconds(0.3f);

        //Spawn puff effect next to the swallow point
        StartCoroutine(SpawnPuff(swallowPoint.transform.position + (new Vector3(playerSprite.flipX ? -1 : 1, 0, 0))));

        //Activation delay
        yield return new WaitForSeconds(0.1f);

        //Get the last swallowed item
        GameObject item = stomachItems.Last();
        //Remove it from stomach
        stomachItems.Remove(item);

        //Depending on the orientation we spawn it at the left or the right
        if (playerSprite.flipX)
        {
            position = new Vector3(
            position.x - 1.1f,
            position.y,
            position.z);
        }
        else
        {
            position = new Vector3(
            position.x + 1.1f,
            position.y,
            position.z);
        }
        
        item.transform.position = position;
        item.SetActive(true);
    }

    /// <summary>
    /// Player teleport skill
    /// </summary>
    private void Skill()
    {
        //Only use skill if not doing something else
        if (skillInput && !isBusy && !isDead && !skillInCooldown)
        {
            //Check if this type have a active skill
            if (slimeType == SlimeType.teleport)
            {
                //Calculate the direction to teleport
                Vector3 teleportVector;
                if (hAxis != 0 || vAxis != 0)
                {
                    teleportVector = new Vector3(hAxis, vAxis, 0);
                    teleportVector.Normalize();
                }
                else
                {
                    if(playerSprite.flipX)
                    {
                        teleportVector = Vector3.left;
                    }
                    else
                    {
                        teleportVector = Vector3.right;
                    }
                }
                teleportVector *= 5; //Teleport distance
                //Check if the destination is a valid target location
                Collider2D[] items = Physics2D.OverlapBoxAll(this.transform.position + teleportVector + new Vector3(0, 0.5f, 0), new Vector2 (0.49f,0.49f), obstacleLayer | groundLayer);
                if (items.Length == 0)
                {
                    isBusy = true;
                    skillInCooldown = true;
                    asTeleport.Play();
                    //Spawn puff effect over the player sprite on origin location
                    StartCoroutine(SpawnTeleport(this.transform.position + new Vector3(0, 0.5f, 0)));

                    //TODO: Teleport animation
                    this.transform.position += teleportVector;

                    //Spawn puff effect over the player sprite on target location
                    StartCoroutine(SpawnTeleport(this.transform.position + new Vector3(0, 0.5f, 0)));

                    //Corroutine to end the skill
                    StartCoroutine(EndSkill(0.2f));
                    //Coroutine for the skill cooldown
                    StartCoroutine(EndCooldown(2f));
                }

            }
        }
    }

    /// <summary>
    /// Delay after skill use
    /// </summary>
    private IEnumerator EndSkill(float skillDelay = 1f)
    {
        //Animation delay
        yield return new WaitForSeconds(skillDelay);

        isBusy = false;
    }

    /// <summary>
    /// End the cooldown of the skill
    /// </summary>
    private IEnumerator EndCooldown(float skillCooldown = 1f)
    {
        //Animation delay
        yield return new WaitForSeconds(skillCooldown);

        skillInCooldown = false;
    }

    /// <summary>
    /// Spawn puff efect on the specified location. Wait for a delay if specified.
    /// </summary>
    /// <param name="spawnLocation">Spawn location</param>
    /// <param name="spawnDelay">Spawn delay</param>
    /// <returns></returns>
    private IEnumerator SpawnPuff(Vector3 spawnLocation, float spawnDelay = 0)
    {
        //Spawn delay
        yield return new WaitForSeconds(spawnDelay);

        GameObject item = Instantiate(puffEffect, spawnLocation, Quaternion.identity);
        
        //Despawn delay
        yield return new WaitForSeconds(0.42f);
        Destroy(item);
    }

    /// <summary>
    /// Spawn teleport efect on the specified location. Wait for a delay if specified.
    /// </summary>
    /// <param name="spawnLocation">Spawn location</param>
    /// <param name="spawnDelay">Spawn delay</param>
    /// <returns></returns>
    private IEnumerator SpawnTeleport(Vector3 spawnLocation, float spawnDelay = 0)
    {
        //Spawn delay
        yield return new WaitForSeconds(spawnDelay);

        GameObject item = Instantiate(teleportEffect, spawnLocation, Quaternion.identity);

        //Despawn delay
        yield return new WaitForSeconds(0.42f);
        Destroy(item);
    }

    /// <summary>
    /// Handle being hit
    /// </summary>
    public void ReceiveDamage(DamageType damageType)
    {
        if(!isDead)
        {
            isDead = true;
            isBusy = true;
            asDeath.Play();
            rb.velocity = Vector3.zero;
            playerAnimator.SetTrigger("die");
            StartCoroutine(GameManager.instance.LoseGame());
            //TODO handle different damage types
        }
    }
}

// Public type of slime
public enum SlimeType
{
    basic, teleport
}