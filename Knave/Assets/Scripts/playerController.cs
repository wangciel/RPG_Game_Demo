using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class playerController : MonoBehaviour
{
    public int winCoins; //this is used for the purpose of the demo. not used in actual game

    //player stats
    public int maxHealth;
    private int currentHealth;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public float invincibleTime; //for after taking damage
    private float currentInvincibleTime;
    public bool isInvincible;

    public Slider energyBar;
    public float maxEnergy;
    public float energyRegen;
    private float currentEnergy;

    private Vector2 lastCheckpoint;


    [Header("Movement")]
    public float movementSpeed;
    private float moveInput;

    [Header("Dashing")]
    public Image dashIcon;
    public Sprite dashReady;

    public Sprite dashNotReady;

    [Header("Powerup Activation")]
    public bool canDash;
    public bool canDoubleJump;
    public bool canGrapple;
    public bool _canWallJump;

    [Header("Dash Settings")]
    [Range(1f, 25f)]
    public float dashSpeed;
    [Range(0.05f, 1f)]
    public float dashTime;
    private float currentDashTime; //used to determine when to stop dashing
    public bool dashing;
    [Range(0.0f, 100f)]
    public float dashCost;

    [Header("Basic Jump Settings")]
    public bool canJump;
    private bool onGround;
    public LayerMask groundLayer;
    [Range(0.1f, 10f)]
    public float jumpForce;
    public Text jumpText;


    [Header("Double Jump Settings")]

    [Range(0.1f, 10f)]
    public float doubleJumpForce;
    private bool doubleJumping;
    int jumpCount = 1;
    public Image DoubleJumpIcon;
    public Sprite doubleJumpReady;
    public Sprite doubleJumpNotReady;



    [Header("Grappling Hook Settings")]
    public float grappleCost;
    [Range(0.01f, 0.2f)]
    public float grappleSpeed; //good idea to keep somewhere between 0 and 1 if you want it slow
    private bool grappling;
    private DistanceJoint2D dj;
    public Image GrappleIcon;
    public Sprite GrappleReadySprite;
    public Sprite GrappleNotReadySprite;
    public Sprite GrappleBreakSprite;

    [Header("Wall Jump Settings")]



    [FormerlySerializedAs("wallJumpCost")]
    [SerializeField]
    private float _wallJumpCost;

    [Header("Ladder Climbing Settings")]
    public bool onLadder;
    public float climbingSpeed;
    public float gravityStore;
    public bool canClimb;
    private bool climbing;

    [Header("Box Pushing Settings")]
    [SerializeField]
    public bool canPushPull;
    public float distance = 1f;
    public LayerMask boxMask;
    bool touchingBox;

    public GameObject box;
    public bool pullingPushing;




    //hazards
    private bool _touchingHurtZone;


    //used for physics
    private Rigidbody2D rb;

    //used for flipping player
    private bool facingRight = true;

    [Header("HUD Settings")]
    public Text chestKeysText;
    public int CurrentChestKeys;
    [Space]
    public int CurrentDoorKeys;
    public Text doorKeysText;
    [Space]
    public Text potionText;
    public int CurrentPotions;
    [Space]
    public Text coinsText;
    public int coins;
    //For interacting with Chests and Doors
    private GameObject interactable;

    //used for animation
    //Animator animator;
    private PlayerAnimator _animator; //replaced with my own implementation so we can stop faffing about.


    Collider2D col;
    SpriteRenderer sr;

    private bool paused;
    [Space]
    public Canvas pauseMenu;


    //sound stuff
    private AudioManagerScript _soundManager;

    //chain swinging stuff
    private bool _swinging;
    private HingeJoint2D _chainJoint;

    private void Start()
    {
        //animator = GetComponent<Animator>();
        _animator = GetComponent<PlayerAnimator>();

        _soundManager = GetComponent<AudioManagerScript>();

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        dj = GetComponent<DistanceJoint2D>();
        dj.enabled = false;


        currentHealth = maxHealth;
        energyBar.maxValue = maxEnergy;
        currentEnergy = maxEnergy;
        energyBar.value = currentEnergy;

        CurrentChestKeys = 0;
        setChestKeyText();
        CurrentDoorKeys = 0;
        setDoorKeyText();
        CurrentPotions = 0;
        setPotionText();
        coins = 0;
        setCoinText();

        setJumpText();


        //set ability stats
        canJump = true;
        //climbing
        canClimb = false;
        gravityStore = rb.gravityScale;
        //double jump
        canDoubleJump = false;
        //dash
        canDash = false;
        currentDashTime = dashTime;
        dashing = false;
        //grappling
        grappling = false;
        //for taking damage
        currentInvincibleTime = invincibleTime;
        isInvincible = false;
        lastCheckpoint = transform.position;
        //jumpForce = 4.5f; //why was this hardcoded?
        paused = false;
        //wallJump
        //touchingWall = false;
        _chainJoint = GetComponent<HingeJoint2D>();
        //pushing/pulling
        box = null;
               
	}


    void FixedUpdate()
    {
        doEnergyRegen();


        //check if they are on ground
        if (rb.velocity.y <= 0)
        {
            onGround = GroundCheck();
        }



        //MOVEMENT
        //left = -1, right = 1
        if (!dashing && !grappling && !(climbing && !onGround))
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            //animator.SetFloat("Speed", moveInput * moveInput);


            //rb.velocity = new Vector2(moveInput*movementSpeed, rb.velocity.y);
            if (Math.Abs(moveInput) > 0.001)
            {
                if (!_swinging)
                {
                    rb.velocity = new Vector2(moveInput * movementSpeed, rb.velocity.y);
                }
                else if (moveInput == Math.Sign(rb.velocity.x))
                {
                    rb.velocity = new Vector2(moveInput * movementSpeed * 2f, rb.velocity.y * 2f);
                }
            }
            else if (!_swinging)
            {
                rb.velocity = new Vector2(rb.velocity.x * 0.525f, rb.velocity.y);
            }

        }


        if (isInvincible)
        {
            sr.color = new Color(5, 0, 0, 1f);
            if (currentInvincibleTime <= 0)
            {
                isInvincible = false;
                currentInvincibleTime = invincibleTime;
                sr.color = new Color(255, 255, 255, 1f);
            }
            else
            {
                currentInvincibleTime -= Time.deltaTime;
            }
        }

        if (_swinging)
        {
            Vector2 targ = _chainJoint.connectedBody.position;
            transform.position = Vector2.Lerp(transform.position, targ, 0.2f);
        }


    }

    void Update()
    {
        ////////////////////////////////////////
        //USED ONLY FOR DEMO NOT FOR ACTUAL GAME
        if (coins >= winCoins)
        {
            SceneManager.LoadScene(3);
        }

        ////////////////////////////////////////
        doDashIcon();
        doDoubleJumpIcon();
        doGrappleIcon();
        if (_touchingHurtZone)
        {
            AttemptTakeDamage(1);
        }

        //DRAWING
        //Used for drawing the health
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }


        //determines if the player is on the ground
        if (onGround)
        {
            //animator.SetBool("onGround", true);
        }
        else
        {
            //animator.SetBool("onGround", false);
        }


        //facing
        //		if (moveInput > 0)
        //		{
        //			ChangeFacingTo(1);
        //		}
        //
        //		if (moveInput < 0)
        //		{
        //			ChangeFacingTo(-1);
        //		}
        if (Math.Abs(rb.velocity.x) > 0.01)
        {
            ChangeFacingTo(rb.velocity.x);
        }


        //Actions


        if (canGrapple)
        {
            if (grappling)
            {
                dj.autoConfigureDistance = false;
                if (dj.distance >= 0)
                {
                    dj.distance -= grappleSpeed;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!grappling && currentEnergy >= grappleCost)
                {
                    //  Physics2D.Linecast(transform.position, Input.mousePosition, groundLayer);
                    Vector2 from = transform.position;
                    Vector2 to = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    RaycastHit2D grappleDetector = Physics2D.Linecast(from, to, groundLayer);

                    //Debug.DrawLine(from, to, new Color(0, 1, 0, 0.5f), 0.1f); //only shows up when gizmos show up

                    if (grappleDetector.collider != null)
                    {
                        if (grappleDetector.collider.CompareTag("Wood"))
                        {
                            dj.connectedAnchor = grappleDetector.point;
                            dj.autoConfigureDistance = true; //set distance
                            dj.enabled = true;


                            grappling = true;
                            //animator.SetBool("Grappling", true);

                            spendEnergy(grappleCost);
                        }
                    }
                }
                else
                {
                    dj.enabled = false;
                    grappling = false;
                    //animator.SetBool("Grappling", false);
                }
            }
        }

        //DASH
        //https://www.youtube.com/watch?v=w4YV8s9Wi3w
        if (canDash)
        {
            if (!dashing && !grappling)
            {
                if (onGround && Input.GetKeyDown(KeyCode.LeftShift) && currentEnergy >= dashCost) //todo: replace this with the input system.
                {
                    dashing = true;
                    spendEnergy(dashCost);
                    //animator.SetBool("Dashing", true);
                    _soundManager.PlayDashSFX();
                    //jumpCount--;
                }
            }

            if (dashing)
            {
                if (currentDashTime <= 0)
                {
                    dashing = false;
                    //animator.SetBool("Dashing", false);
                    currentDashTime = dashTime;
                    rb.velocity = Vector2.zero;
                }
                else
                {
                    currentDashTime -= Time.deltaTime;
                    if (facingRight)
                    {
                        rb.velocity = Vector2.right * dashSpeed;
                    }
                    else
                    {
                        rb.velocity = Vector2.left * dashSpeed;
                    }
                }
            }
        }

        //WALL JUMPING
        if (_canWallJump && canJump && !onGround)
        {
            int wallDirection;
            if (TouchingWallCheck(out wallDirection) && AttemptingAscent())
            {
                if (currentEnergy >= _wallJumpCost)
                {
                    spendEnergy(_wallJumpCost);
                    rb.velocity = new Vector2(jumpForce * -wallDirection, doubleJumpForce);
                    _soundManager.PlayJumpSFX();
                }
            }
        }

        //DOUBLE JUMPS
        if (canJump && canDoubleJump)
        {
            if (onGround)
            {
                jumpCount = 1;
            }

            else if (AttemptingAscent() && !dashing && !grappling && !climbing && jumpCount > 0)
            {
                onGround = false;
                doubleJumping = true;
                jumpCount--;

                //rb.velocity = Vector2.up * doubleJumpForce;
                if (rb.velocity.y > 0)
                {
                    rb.AddForce(Vector2.up * doubleJumpForce * 25);
                }
                else
                {
                    rb.velocity = Vector2.up * doubleJumpForce;
                }

                _soundManager.PlayDashSFX();

            }
        }
        //REGULAR JUMPING
        if (canJump && onGround)
        {
            if (AttemptingAscent() && !grappling)
            {
                onGround = false;
                if (!climbing)
                {
                    Jump();
                }
            }
        }

        //LADDER CLIMBING

        if (onLadder && canClimb)
        {

            climbing = true;


        }
        if (!onLadder)
        {
            canClimb = false;
            rb.gravityScale = gravityStore;
            climbing = false;

        }
        if (climbing)
        {

            rb.gravityScale = 0f;


            //onGround = false;



            var climbVertical = Input.GetAxisRaw("Vertical");
            var climbHorizontal = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(climbHorizontal * climbingSpeed * 1.5f, climbVertical * climbingSpeed);


        }

        //PUSHING/PULLING
        if (canPushPull && box != null)
        {

            if (Input.GetKeyDown(KeyCode.C))
            {
                canGrapple = !canGrapple;
                pullingPushing = !pullingPushing;
                if (pullingPushing)
                {
                    box.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    box.GetComponent<RelativeJoint2D>().connectedBody = rb;
                }
            }

            else if (!pullingPushing && box != null)
            {

                box.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                box.GetComponent<FixedJoint2D>().enabled = false;
                box.GetComponent<RelativeJoint2D>().connectedBody = null;
            }
        }


            
           
        
		
		//Jumping off chains
		if (_swinging)
		{
			var swingScript = _chainJoint.connectedBody.GetComponent<ChainGrabScript>();
			if (swingScript.GetInteractionPermitted())
			{
				if (Input.GetAxisRaw("Vertical") < -0.1)
				{
					swingScript.TriggerInteractionTimer(0.2f);
					SetSwinging(false);
				}
				else if (Input.GetAxisRaw("Vertical") > 0.5)
				{
					swingScript.TriggerInteractionTimer();
					SetSwinging(false);
					Jump();
				}
			}
		}



        //HEALING
        if (Input.GetKeyDown(KeyCode.E) && CurrentPotions != 0 && currentHealth != maxHealth && currentHealth != 0)
        {
            heal(1);
            Camera.main.GetComponent<CameraHintingCore>().SetHurtEffect(0);
            CurrentPotions--;
            setPotionText();

            _soundManager.PlayHealSFX();
        }


        //INTERACTING WITH DOORS AND CHESTS
        if (Input.GetKeyDown(KeyCode.F) && interactable != null)
        {
            interactable.GetComponent<InteractiveObjectScript>().PlayerInteract(this);
            interactable = null;
        }


        //TESTS
        //TESTING LOSING HEALTH THIS IS TEMPORARY
        if (Input.GetKeyDown(KeyCode.O))
        {
            AttemptTakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            heal(1);
        }

        //TESTING GAINING KEYS
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (CurrentChestKeys != 0)
            {
                CurrentChestKeys--;
                setChestKeyText();
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            CurrentChestKeys++;
            setChestKeyText();
        }

        //TESTING GAINING POTIONS
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (CurrentPotions != 0)
            {
                CurrentPotions--;
                setPotionText();
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            CurrentPotions++;
            setPotionText();
        }

        //PAUSE BUTTON
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                paused = true;
                Time.timeScale = 0;
                pauseMenu.gameObject.SetActive(true);
            }
            else
            {
                paused = false;
                Time.timeScale = 1;
                pauseMenu.gameObject.SetActive(false);
            }
        }

        _animator.UpdateAnimationState();
    }

    private void Jump()
    {
        rb.velocity = Vector2.up * jumpForce;
        _soundManager.PlayJumpSFX();
    }
    private void Jump(bool playSound)
    {
        if (playSound)
        {
            Jump();
            return;
        }
        rb.velocity = Vector2.up * jumpForce;
    }


    // player takes damage
    public void AttemptTakeDamage(int damage)
    {
        if (!dashing && !isInvincible)
        {
            Camera.main.GetComponent<CameraHintingCore>().SetHurtEffect(1.0f - (currentHealth - 2) / 5.0f);
            if (rb.velocity.y <= 0.001f)
            {
                rb.velocity = 2.5f * Vector2.up;
                //rb.velocity = 1.75f * Vector2.up;
            }

            _soundManager.PlayHurtSFX();
            _animator.PlayerHealthAdjusted(true);

            if (currentHealth - damage <= 0)
            {
                currentHealth = 0;
                transform.position = lastCheckpoint;
                heal(5);
                //rb.velocity = new Vector2(0, 0);
            }
            else
            {
                currentHealth -= damage;
                isInvincible = true;
                currentInvincibleTime = invincibleTime;
            }
        }
    }

    void heal(int heal)
    {
        _animator.PlayerHealthAdjusted(false);
        if (currentHealth + heal >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += heal;
        }
    }

    bool GroundCheck()
    {
        //left ray
        Vector3 rayPos1 = transform.position;
        rayPos1.x -= col.bounds.extents.x;

        //right ray
        Vector3 rayPos2 = transform.position;
        rayPos2.x += col.bounds.extents.x;

        //middle ray
        Vector3 rayPos3 = transform.position;
        rayPos3.x = transform.position.x;

        RaycastHit2D groundDetector1 =
            Physics2D.Raycast(rayPos1, Vector2.down, col.bounds.extents.y + 0.1f, groundLayer);
        RaycastHit2D groundDetector2 =
            Physics2D.Raycast(rayPos2, Vector2.down, col.bounds.extents.y + 0.1f, groundLayer);
        RaycastHit2D groundDetector3 =
            Physics2D.Raycast(rayPos3, Vector2.down, col.bounds.extents.y + 0.1f, groundLayer);

        //debugging
        //Color c = new Color(0, 1, 0, 0.5f);
        //Debug.DrawRay(rayPos1, Vector2.down * (col.bounds.extents.y + 0.1f), c);
        //Debug.DrawRay(rayPos2, Vector2.down * (col.bounds.extents.y + 0.1f), c);
        //Debug.DrawRay(rayPos3, Vector2.down * (col.bounds.extents.y + 0.1f), c);
        //debugging over
        bool retValue = groundDetector1 || groundDetector2 || groundDetector3;
        //animator.SetBool("onGround", retValue);
        return retValue;
        //		{
        //			onGround = true;
        //		}
        //		else
        //		{
        //			onGround = false;
        //			animator.SetBool("onGround", false);
        //		}
    }

    bool TouchingWallCheck(out int wallDirection)
    {
        Vector3 rayPos1 = transform.position;
        rayPos1.y -= col.bounds.extents.y;

        Vector3 rayPos2 = transform.position;
        rayPos2.y += col.bounds.extents.y;

        Vector3 rayPos3 = transform.position;
        rayPos3.y = transform.position.y;

        RaycastHit2D wallDetector1 = Physics2D.Raycast(rayPos1, Vector2.left, col.bounds.extents.x + 0.1f, groundLayer);
        RaycastHit2D wallDetector2 = Physics2D.Raycast(rayPos1, Vector2.right, col.bounds.extents.x + 0.1f, groundLayer);

        RaycastHit2D wallDetector3 = Physics2D.Raycast(rayPos2, Vector2.left, col.bounds.extents.x + 0.1f, groundLayer);
        RaycastHit2D wallDetector4 = Physics2D.Raycast(rayPos2, Vector2.right, col.bounds.extents.x + 0.1f, groundLayer);

        RaycastHit2D wallDetector5 = Physics2D.Raycast(rayPos3, Vector2.left, col.bounds.extents.x + 0.1f, groundLayer);
        RaycastHit2D wallDetector6 = Physics2D.Raycast(rayPos3, Vector2.right, col.bounds.extents.x + 0.1f, groundLayer);

        wallDirection = wallDetector1 || wallDetector3 || wallDetector5 ? -1 : 1;
        return wallDetector1 || wallDetector2 || wallDetector3 || wallDetector4 || wallDetector5 || wallDetector6;
    }

    //trigger for interacting with things like keys and chests
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactive"))
        {
            interactable = collision.gameObject;
            collision.gameObject.GetComponent<InteractiveObjectScript>().OnCollisionBegin(this);
        }

        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            lastCheckpoint = transform.position;
        }
        if (collision.gameObject.CompareTag("Spikes"))
        {
            _touchingHurtZone = true;
        }
        if (collision.gameObject.CompareTag("Ladder"))
        {
            canClimb = true;
        }
        if (collision.gameObject.CompareTag("Box"))
        {
            box = collision.gameObject;
            touchingBox = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactive") && interactable == collision.gameObject)
        {
            interactable.GetComponent<InteractiveObjectScript>().OnCollisionEnd(this);
            interactable = null;
        }

        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            //collision.gameObject.collider.
        }

        if (collision.gameObject.CompareTag("Spikes"))
        {
            _touchingHurtZone = false;
        }

        if (collision.gameObject.CompareTag("Ladder"))
        {
            canClimb = false;
        }
        if(collision.gameObject.CompareTag("Box")){
            touchingBox = false;
        }
    }

    //	void OnCollisionEnter2D(Collision2D collision)
    //	{
    //
    //	}
    //
    //	private void OnCollisionExit2D(Collision2D collision)
    //	{
    //
    //	}


    //setting the chest key text that appears on UI
    public void setChestKeyText()
    {
        chestKeysText.text = "x" + CurrentChestKeys;
    }

    public void setDoorKeyText()
    {
        doorKeysText.text = "x" + CurrentDoorKeys;
    }

    //setting the potion text that appears on UI
    public void setPotionText()
    {
        potionText.text = "x" + CurrentPotions;
    }

    public void setCoinText()
    {
        coinsText.text = "" + coins + " / 400";
    }

    public void setJumpText()
    {
        jumpText.text = "x"; //+ totalJumps
    }

    //Energy Management
    void doEnergyRegen()
    {
        if (currentEnergy + energyRegen >= maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
        else
        {
            currentEnergy += energyRegen;
        }

        energyBar.value = currentEnergy;
    }

    void spendEnergy(float cost)
    {
        if (currentEnergy - cost <= 0)
        {
            currentEnergy = 0;
        }
        else
        {
            currentEnergy -= cost;
        }

        energyBar.value = currentEnergy;
    }




    void doDashIcon()
    {
        if (canDash && currentEnergy >= dashCost)
        {
            dashIcon.sprite = dashReady;
        }
        else
        {
            dashIcon.sprite = dashNotReady;
        }
    }

    void doDoubleJumpIcon()
    {
        if (canDoubleJump && canJump)
        {
            DoubleJumpIcon.sprite = doubleJumpReady;
        }
        else
        {
            DoubleJumpIcon.sprite = doubleJumpNotReady;
        }
    }


    void doGrappleIcon()
    {
        if (grappling)
        {
            GrappleIcon.sprite = GrappleBreakSprite;
        }
        else if (canGrapple && currentEnergy >= grappleCost)
        {
            GrappleIcon.sprite = GrappleReadySprite;
        }
        else
        {
            GrappleIcon.sprite = GrappleNotReadySprite;
        }
    }

    //used to flip the sprite horizontally so we dont need to make 2x the art
    void ChangeFacingTo(float newXDirection)
    {
        sr.flipX = !(facingRight = newXDirection > 0);
    }

    public void PickedUpItem()
    {
        _soundManager.PlayPickUpSFX();
    }

    public bool GetOnGround()
    {
        return onGround;
    }

    public bool AttemptingAscent()
    {
        return Input.GetAxisRaw("Vertical") > 0 || Input.GetAxisRaw("Jump") > 0;
    }


    public bool IsClimbing()
    {
        return climbing;
    }


    public void SetSwinging(bool b, Rigidbody2D rb)
    {
        _swinging = b;
        _chainJoint.enabled = b;
        if (b)
        {
            _chainJoint.connectedBody = rb;
        }
    }

    public void SetSwinging(bool b)
    {
        SetSwinging(b, _chainJoint.connectedBody);
    }

    public bool IsSwinging()
    {
        return _swinging;
    }


}