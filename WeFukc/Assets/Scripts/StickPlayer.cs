using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StickPlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 15f;
    [SerializeField] private float jumpForce = 30f;
    [SerializeField] private float flyingKickForce = 15f;
    [SerializeField] private float flyingKickUp = 15f;
    [SerializeField] private StickSensor groundSensor;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider rageSlider;

    [Header("Specs")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaIncrement = 4f;
    [SerializeField] private float punchHitPoint = 5f;
    [SerializeField] private float punchRunHitPoint = 10f;
    [SerializeField] private float kickHitPoint = 5f;
    [SerializeField] private float flyingKickHitPoint = 15f;
    [SerializeField] private float turningKickHitPoint = 25f;

    [Header("Death Components")]
    [SerializeField] private BoxCollider2D characterCollider;
    [SerializeField] private GameObject deadBody;
    [SerializeField] private GameObject deadBody_Head;
    [SerializeField] private GameObject deadBody_Leg;
    [SerializeField] private GameObject[] originalBodyParts;

    [Header("Fight Components")]
    [SerializeField] private Transform punchHitLocation;
    [SerializeField] private Transform kickHitLocation;
    [SerializeField] private Transform flyingKickHitLocation;
    [SerializeField] private Transform turningKickHitLocation;
    [SerializeField] private float punchHitRange = 1f;
    [SerializeField] private float kickHitRange = 0.8f;
    [SerializeField] private float flyingKickHitRange = 1f;
    [SerializeField] private float turningKickHitRange = 1f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float takenPunchMove = 3f;
    [SerializeField] private float takenKickMove = 6f;
    [SerializeField] private FloatingDamage damageCanvas;

    [SerializeField] private LayerMask elevatorLayer;
    [SerializeField] private Image keyImage;

    // Other game objects and components
    private Animator animator;
    private Rigidbody2D rigidbody;

    // Health and Stamina
    private float health;
    private bool isDying = false;
    private string deathType;
    public float stamina;
    public float staminaJump = 3f;


    // Movement vars
    private bool grounded = false;
    private bool canAnimate = true;
    private float inputX = 0f;
    private float movement = 0f;
    private float facingRightInt = 0f;
    private float velocityABS = 0f;

    // Fighting vars
    private bool isPunching = false;
    private bool isRunPunching = false;
    private bool isKicking = false;
    private bool isFlyKicking = false;
    private bool isTurningKicking = false;
    private bool isJumping = false;
    private bool isDefending = false;
    private bool didKickFront = false;
    Collider2D[] hitEnemies;
    private bool allowMissSound = true;
    private float missSoundDelay = 1f;

    // Other vars
    private bool isElevatorActivated = false;
    private bool fixFukcingPosBug = false;
    [SerializeField] private bool hasKey = true;

    private const string SPEED = "Speed"; 
    private const string ON_AIR= "OnAir"; 
    private const string PUNCH_HIT = "PunchHit"; 
    private const string PUNCH_RUN = "PunchRun"; 
    private const string FLYING_KICK = "FlyingKick"; 
    private const string KICK = "Kick"; 
    private const string TURNING_KICK = "TurningKick"; 
    private const string DEFENDING = "Defending"; 
    private const string DAMAGE_HEAD = "DamageHead"; 
    private const string DAMAGE_DOWN = "DamageDown"; 
    private const string KICK_FALL = "KickFall";
    private const string SNARE_HEAD = "SnareHead";
    private const string SNARE_DOWN = "SnareDown";
    private const string SNARE_BIG = "SnareBig";


    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();

        health = maxHealth;
        healthSlider.value = health;
        stamina = maxStamina;
        staminaSlider.value = stamina;

        // Get the initial facing direction
        if (transform.localScale.x > 0f) facingRightInt = 1f;
        else facingRightInt = -1f;

        // Make player always chasing to avoid it get into slow walk anim like bots
        animator.SetBool("isChasing", true);
    }



    private void Update()
    {
        // if scene is not ready, do not execute anything
        if (!FindObjectOfType<LevelLoader>().isSceneReady()) return;

        // fix the player's position after hoping into the elevator
        if (fixFukcingPosBug)
        {
            transform.localPosition = Vector2.zero;
            return;
        }

        // If dying, stop and return
        if (isDying)
        {
            rigidbody.velocity = Vector3.zero;
            return;
        }

        if (!allowMissSound)
        {
            missSoundDelay -= Time.deltaTime;
            if (missSoundDelay <= 0f)
            {
                allowMissSound = true;
                missSoundDelay = 1f;
            }
        }

        // Debug.Log("Collider Transform: " + GetComponent<Collider2D>().GetComponentInParent<Transform>().position);

        // Debug Fights
        //if (Input.GetMouseButtonDown(0)) { TakenDamage(PUNCH_RUN, 5, true); }
        //if (Input.GetMouseButtonDown(1)) { TakenDamage(FLYING_KICK, 5, true); }

        if (stamina < 100f) stamina += Time.deltaTime * staminaIncrement;  // Increase stamina 1 point every second
        else stamina = 100f;                            // If it exceed 100, set it as 100
        staminaSlider.value = stamina;

        StatusCheck();
        Actions();
    }

    private void StatusCheck()
    {
        // GROUND CHECK
        // Check the ground if only player stays stable or goes down in Y axis
        if (rigidbody.velocity.y <= 0.001f) 
        {
            //I was on air but now I landed
            if (!grounded && groundSensor.State())
            {
                grounded = true;
            }
            //I was on land but now I am not
            else if (grounded && !groundSensor.State())
            {
                grounded = false;
            }
        }

        // Get the input
        inputX = Input.GetAxis("Horizontal");
        movement = inputX * movementSpeed;

        if (!canAnimate) return;

        if (Input.GetButtonDown("Jump") && grounded && stamina > staminaJump)
        {
            isJumping = true;
        }

        // Gettin velocity
        velocityABS = Mathf.Abs(rigidbody.velocity.x);

        ///// Fight /////
        if (!grounded) return;  // On air, not get fighting input
        // Punching // 
        if (Input.GetKeyDown("j") && velocityABS > 10f && stamina > punchRunHitPoint) isRunPunching = true;
        else if (Input.GetKeyDown("j") && velocityABS < 1f && stamina > punchHitPoint) isPunching = true;
        
        // Kicking //
        else if (Input.GetKeyDown("l") && grounded && Input.GetKey("w") && stamina > turningKickHitPoint) isTurningKicking = true;
        else if (Input.GetKeyDown("l") && grounded && velocityABS > 2f && stamina > flyingKickHitPoint) isFlyKicking = true;
        else if (Input.GetKeyDown("l") && grounded && velocityABS < 1f && stamina > kickHitPoint) isKicking = true;
        
        // Elevator Check - But not after activated!
        else if (Input.GetButton("Submit"))
        {
            if (hasKey) isElevatorActivated = true;
            else
            {
                FindObjectOfType<Elevator>().ShowKeyWarning(); 
            }
        }

        // Defense //
        else if (Input.GetKeyDown("s") || Input.GetKey("s")) isDefending = true;

        // If none of them pressed, reset all
        else
        {
            isPunching = false;
            isRunPunching = false;
            isTurningKicking = false;
            isFlyKicking = false;
            isKicking = false;
            isElevatorActivated = false;
        }
        // If the key is released
        if (Input.GetKeyUp("s")) isDefending = false;

        
    }

    private void Actions()
    {
        if (!canAnimate) return; // If we can't animate a new movement, then doesn't care about the input

        if (isElevatorActivated)
        {
            // Check if there is an elevator in front of us
            Collider2D elevator = Physics2D.OverlapCircle(punchHitLocation.position, punchHitRange, elevatorLayer);

            if (elevator == null)
            {
                isElevatorActivated = false;
                return;
            }

            // Stop player 
            rigidbody.gravityScale = 0f;
            rigidbody.velocity = Vector2.zero;

            // Make player child of elevator
            transform.parent = elevator.gameObject.transform;

            // Move it into the elevator and turn in to left
            transform.localPosition = new Vector2(0f, transform.localPosition.y);
            transform.localScale = new Vector2(-1f, 1f);

            // Move Elevator
            elevator.GetComponent<Elevator>().ActivateElevator();

            isElevatorActivated = false;
            fixFukcingPosBug = true;
        }

        if (isDefending)
        {
            animator.SetBool(DEFENDING, true);      // Set animator
            rigidbody.velocity = new Vector2(0, 0); // Stop the char
            stamina += Time.deltaTime * staminaIncrement;              // Boost the stamina
            return;
        }
        else
        {
            animator.SetBool(DEFENDING, false);     // Set animator
        }

        ///// FLIP /////
        // Change the facing direction according to input
        if (inputX > 0f) FlipCharacter(true);
        else if (inputX < 0f) FlipCharacter(false);

        ///// Move /////
        // Move left and right if we can animate new movement
        rigidbody.velocity = new Vector2(movement, rigidbody.velocity.y);

        // Jump if you are on ground
        if (isJumping)
        {
            grounded = false;
            isJumping = false;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpForce);
            groundSensor.Disable(0.2f);
            stamina -= staminaJump;
            FindObjectOfType<AudioManager>().PlaySFX("Jump");
        }



        ///  ************************   ///
        ///         Animations          ///
        ///  ************************   ///

        ///// Move /////
        animator.SetFloat(SPEED, Mathf.Abs(rigidbody.velocity.x));

        // Jump
        animator.SetBool(ON_AIR, !grounded);

        #region Animation Check
        // Punching //
        if (isRunPunching)
        {
            animator.SetTrigger(PUNCH_RUN);
            isRunPunching = false;

            stamina -= punchRunHitPoint;

            FindObjectOfType<AudioManager>().PlaySFX("Attack_Effort");
        }
        if (isPunching)
        {
            animator.SetTrigger(PUNCH_HIT);
            isPunching = false;
            canAnimate = false;

            stamina -= punchHitPoint;

            // Make player stop to avoid unwanted moves
            rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);

            FindObjectOfType<AudioManager>().PlaySFX("Attack_Effort");
        }
        // Kicking //
        if (isFlyKicking)
        {
            animator.SetTrigger(FLYING_KICK);

            // Adjusting fly settings. With gravity change, we have more natural fly
            if (inputX > 0) rigidbody.velocity = new Vector2(flyingKickForce, flyingKickUp);
            else rigidbody.velocity = new Vector2(-flyingKickForce, flyingKickUp);

            stamina -= flyingKickHitPoint;

            isFlyKicking = false;
            canAnimate = false;

            FindObjectOfType<AudioManager>().PlaySFX("Jump");
        }
        if (isKicking)
        {
            animator.SetTrigger(KICK);
            isKicking = false;
            canAnimate = false;

            stamina -= kickHitPoint;

            FindObjectOfType<AudioManager>().PlaySFX("Attack_Effort");
        }
        if (isTurningKicking)
        {
            animator.SetTrigger(TURNING_KICK);

            rigidbody.velocity = new Vector2(rigidbody.velocity.x, flyingKickUp);

            stamina -= turningKickHitPoint;

            isTurningKicking = false;
            canAnimate = false;

            FindObjectOfType<AudioManager>().PlaySFX("Jump");
        }
        #endregion


    }

    private void toggleCanAnimate() { canAnimate = !canAnimate; }
    private void FlipCharacter(bool flipRight)
    {
        if (flipRight)
        {
            transform.localScale = new Vector2(1, 1);
            facingRightInt = 1; // Indicate that we are facing right
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            facingRightInt = -1; // Indicate that we are NO facing right
        }
    }
    
    private void OnDrawGizmos()
    {
        //Gizmos.DrawCube(transform.position, new Vector2(10f, 2f));
        /*
        Vector2 drawGismos = new Vector2(turningKickHitLocation.position.x, turningKickHitLocation.position.y);
        Gizmos.DrawWireSphere(drawGismos, turningKickHitRange);

        if (facingRightInt > 0) 
            drawGismos = new Vector2(turningKickHitLocation.position.x - 2.6f, turningKickHitLocation.position.y);
        else
            drawGismos = new Vector2(turningKickHitLocation.position.x + 2.6f, turningKickHitLocation.position.y);
        Gizmos.DrawWireSphere(drawGismos, turningKickHitRange); 
        */
    }
    
    ///  ************************   ///
    ///      Animation Events       ///
    ///  ************************   ///
    private void PunchHit()
    {
        hitEnemies = Physics2D.OverlapCircleAll
            (punchHitLocation.position, punchHitRange, enemyLayers);

        bool damageFromRight;

        foreach (Collider2D enemy in hitEnemies)
        {
            if ((transform.position.x - enemy.transform.position.x) > 0) damageFromRight = true;
            else damageFromRight = false;
            enemy.GetComponent<StickBot>().TakenDamage(PUNCH_HIT, punchHitPoint, damageFromRight);
        }

        if (hitEnemies.Length < 1) FindObjectOfType<AudioManager>().PlaySFX("Attack_Miss");
    }
    private void PunchRunHit()
    {   // Punch run uses the same location as normal punch but has different hit points
        hitEnemies = Physics2D.OverlapCircleAll
            (punchHitLocation.position, punchHitRange, enemyLayers);

        bool damageFromRight;

        foreach (Collider2D enemy in hitEnemies)
        {
            if ((transform.position.x - enemy.transform.position.x) > 0) damageFromRight = true;
            else damageFromRight = false;
            enemy.GetComponent<StickBot>().TakenDamage(PUNCH_RUN, punchRunHitPoint, damageFromRight);
        }

        // Flygin Kick and running punch has allowAttackSound restriction to avoid multiple sounds in one shot
        if (allowMissSound)
        {
            if (hitEnemies.Length < 1) FindObjectOfType<AudioManager>().PlaySFX("Attack_Miss");
            allowMissSound = false;
        }
    }
    private void KickHit()
    {   // I use punchHit locaiton instead of Kick. Because it create an error somehow
        hitEnemies = Physics2D.OverlapCircleAll
            (punchHitLocation.position, kickHitRange, enemyLayers);

        bool damageFromRight;

        foreach (Collider2D enemy in hitEnemies)
        {
            if ((transform.position.x - enemy.transform.position.x) > 0) damageFromRight = true;
            else damageFromRight = false;
            enemy.GetComponent<StickBot>().TakenDamage(KICK, kickHitPoint, damageFromRight);
        }

        if (hitEnemies.Length < 1) FindObjectOfType<AudioManager>().PlaySFX("Attack_Miss");
    }
    private void FlyingKickHit()
    {
        hitEnemies = Physics2D.OverlapCircleAll
            (flyingKickHitLocation.position, flyingKickHitRange, enemyLayers);

        bool damageFromRight;

        foreach (Collider2D enemy in hitEnemies)
        {
            if ((transform.position.x - enemy.transform.position.x) > 0) damageFromRight = true;
            else damageFromRight = false;
            if (enemy) enemy.GetComponent<StickBot>().TakenDamage(FLYING_KICK, flyingKickHitPoint, damageFromRight);
        }

        // Flygin Kick and running punch has allowAttackSound restriction to avoid multiple sounds in one shot
        if (allowMissSound)
        {
            if (hitEnemies.Length < 1) FindObjectOfType<AudioManager>().PlaySFX("Attack_Miss");
            allowMissSound = false;
        }
    }
    private void TurningKickHit()
    {

        if (!didKickFront)  // Kick the front first
        {
            hitEnemies = Physics2D.OverlapCircleAll(turningKickHitLocation.position, turningKickHitRange, enemyLayers);

            didKickFront = true;
        }
        else  // Then animation will call this second time. Kick the back then.
        {
            Vector2 backLocation;
            if (facingRightInt > 0) // If we are facing right, then take -2.6 as our back, 
            {
                backLocation = new Vector2    // Get the back location
                (turningKickHitLocation.position.x - 2.6f, turningKickHitLocation.position.y);
            }
            else                    // If we are facing lect, take +2.6 as our back
            {
                backLocation = new Vector2    // Get the back location
                (turningKickHitLocation.position.x + 2.6f, turningKickHitLocation.position.y);
            }

            hitEnemies = Physics2D.OverlapCircleAll(backLocation, turningKickHitRange, enemyLayers);

            didKickFront = false; // Reset the var
        }

        bool damageFromRight;

        foreach (Collider2D enemy in hitEnemies)
        {
            if ((transform.position.x - enemy.transform.position.x) > 0) damageFromRight = true;
            else damageFromRight = false;

            if (enemy) enemy.GetComponent<StickBot>().TakenDamage(TURNING_KICK, turningKickHitPoint, damageFromRight);
        }

        if (hitEnemies.Length < 1) FindObjectOfType<AudioManager>().PlaySFX("Attack_Miss");
    }
    private void KickFallBackUp()
    {   // Push forward when its standing up again
        rigidbody.velocity = new Vector2(takenPunchMove * facingRightInt, rigidbody.velocity.y);
    }
    private void StopCharacter()
    {
        rigidbody.velocity = Vector2.zero;
    }

    ///  ************************   ///
    ///       Taking Damage         ///
    ///  ************************   ///

    // TakenDamage is a one-size-fits-all method
    public void TakenDamage(string _takenDamageType, float _takenDamagePoint, bool _DamageDirection)
    {
        if (!canAnimate) return; // If the char even can't move, don't take any damage

        if (isDefending)  // Don't take damage if we are defending against enemy
        {
            if (facingRightInt > 0 && _DamageDirection) return;
            else if (facingRightInt < 0 && !_DamageDirection) return;
            else
            {

                isDefending = false;
                animator.SetBool(DEFENDING, false);
            }
        }

        health -= _takenDamagePoint; 
        if (health <= 0f)
        {
            isDying = true;
            deathType = _takenDamageType;
            healthSlider.value = 0f;
        }
        healthSlider.value = health;

        // Stop further animations and let the damage animation plays
        canAnimate = false;
        rigidbody.velocity = Vector3.zero; // And stop the character completely

        // Flip the character to the direction where the damage comes from
        if (_DamageDirection && facingRightInt != 1) FlipCharacter(true);
        if (!_DamageDirection && facingRightInt != -1) FlipCharacter(false);

        // Animate
        if (_takenDamageType == PUNCH_HIT || _takenDamageType == PUNCH_RUN ||_takenDamageType == SNARE_HEAD) animator.SetTrigger(DAMAGE_HEAD);
        else if (_takenDamageType == FLYING_KICK ||_takenDamageType == TURNING_KICK || _takenDamageType == SNARE_BIG) animator.SetTrigger(KICK_FALL);
        else animator.SetTrigger(DAMAGE_DOWN);

        // Make attack sound (even though we got attacked, we make it)
        if (_takenDamageType == SNARE_BIG) { FindObjectOfType<AudioManager>().PlaySFX("Big_Pain"); }
        else if (_takenDamageType == SNARE_HEAD || _takenDamageType == SNARE_DOWN) { FindObjectOfType<AudioManager>().PlaySFX("Little_Pain"); }
        else { FindObjectOfType<AudioManager>().PlayAttackSound(); FindObjectOfType<AudioManager>().PlaySFX("Little_Pain"); }

        // Floating Damage
        Instantiate(damageCanvas, transform.position, Quaternion.identity).
            damageText.text = "-" + _takenDamagePoint.ToString();

        // Move away according to hit type
        if (_takenDamageType == PUNCH_RUN) 
            rigidbody.velocity = new Vector2(-takenPunchMove * facingRightInt, rigidbody.velocity.y);
        if (_takenDamageType == FLYING_KICK || _takenDamageType == TURNING_KICK) 
            rigidbody.velocity = new Vector2(-takenKickMove * facingRightInt, rigidbody.velocity.y);
    }
    
    // Death actions
    private void CheckDeath()
    {
        if (!isDying) return;

        // Disable the collider and original body parts
        characterCollider.enabled = false;
        foreach (GameObject bodyPart in originalBodyParts)
        {
            bodyPart.SetActive(false);
        }


        // Death style
        if (deathType == PUNCH_HIT || deathType == PUNCH_RUN || deathType == TURNING_KICK)
        {   // Head damage
            deadBody.SetActive(true);
            deadBody_Head.GetComponent<Rigidbody2D>().AddForce(new Vector2(-500, 0));
        }
        else // Down Damage
        {
            deadBody.SetActive(true);
            deadBody_Leg.GetComponent<Rigidbody2D>().AddForce(new Vector2(-1500, 0));
            deadBody_Head.GetComponent<Rigidbody2D>().AddForce(new Vector2(500, 0));
        }

        // Now Trigger common death actions
        StartCoroutine(DestroyLater());
    }

    // Occurs when a certain death comes like death wall
    public void CertainDeath() { health = 0f; isDying = true; CheckDeath(); }

    IEnumerator DestroyLater()
    {
        FindObjectOfType<AudioManager>().PlaySFX("Death");

        yield return new WaitForSeconds(10f);

        Destroy(gameObject);
    }

    public void PickUpKey()
    {
        hasKey = true;
        keyImage.enabled = true;
    }

}
