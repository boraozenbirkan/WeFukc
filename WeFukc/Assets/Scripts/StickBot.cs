using UnityEngine;
using System.Collections;

public class StickBot : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 20f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float flyingKickForce = 5f;
    [SerializeField] private float flyingKickUp = 5f;
    [SerializeField] private StickSensor groundSensor;
    [SerializeField] private StickSensor endUpSensor;
    [SerializeField] private StickSensor endDownSensor;

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

    // Other game objects and components
    private Animator animator;
    private Rigidbody2D rigidbody;

    // Health and Stamina
    private float health;
    private bool isDying = false;
    private string deathType;


    // Movement vars
    private bool grounded = false;
    private bool canAnimate = true;
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

    private const string SPEED = "Speed";
    private const string ON_AIR = "OnAir";
    private const string PUNCH_HIT = "PunchHit";
    private const string PUNCH_RUN = "PunchRun";
    private const string FLYING_KICK = "FlyingKick";
    private const string KICK = "Kick";
    private const string TURNING_KICK = "TurningKick";
    private const string DEFENDING = "Defending";
    private const string DAMAGE_HEAD = "DamageHead";
    private const string DAMAGE_DOWN = "DamageDown";
    private const string KICK_FALL = "KickFall";

    // Bot vars
    private bool isWalkingRight = false;
    private bool isRunningRight = false;
    private bool isChasing = false;
    private bool isStopped = false;
    private float patrolMaxChangeTime = 25f;   // Change time 5-25
    private float patrolMaxStopTime = 10f;     // Stop time 3-10
    private float patrolChangeTime;   // Change time 5-25
    private float patrolStopTime;     // Stop time 3-10


    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();

        health = maxHealth;

        // Get the initial facing direction
        if (transform.localScale.x > 0f) facingRightInt = 1f;
        else facingRightInt = -1f;

        patrolStopTime = patrolMaxStopTime;
        patrolChangeTime = patrolMaxChangeTime;
    }



    private void Update()
    {
        // if scene is not ready, do not execute anything
        if (!FindObjectOfType<LevelLoader>().isSceneReady()) return;

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

        StatusCheck();
        States();
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

        

        if (!canAnimate) return;

        /*
        if (Input.GetButtonDown("Jump") && grounded)
        {
            isJumping = true;
        }
        */

        // Gettin velocity
        velocityABS = Mathf.Abs(rigidbody.velocity.x);
        /*
        ///// Fight /////
        if (!grounded) return;  // On air, not get fighting input
        // Punching // 
        if (Input.GetKeyDown("j") && velocityABS > 10f) isRunPunching = true;
        else if (Input.GetKeyDown("j") && velocityABS < 1f) isPunching = true;

        // Kicking //
        else if (Input.GetKeyDown("l") && grounded && Input.GetKey("w")) isTurningKicking = true;
        else if (Input.GetKeyDown("l") && grounded && velocityABS > 2f) isFlyKicking = true;
        else if (Input.GetKeyDown("l") && grounded && velocityABS < 1f) isKicking = true;

        // Defense //
        else if (Input.GetKeyDown("s") || Input.GetKey("s")) isDefending = true;
        // If the key is released
        if (Input.GetKeyUp("s")) isDefending = false;
        */
    }

    private void States()
    {
        // is chasing?
            // is needed to jump
                // Jump
            // is close combat
                // Slow Walk
                    // is inRange?
                        // Hit
            // else
                // Run
        // Patrol
            // Slow Walk w/o Guard
        // isStopped


        if (isChasing)
        {

        }

        // Movement = speeds
        else if (isWalkingRight && !isStopped)
        {
            movement = movementSpeed / 8;

            // Don't check sensors when they are disabled
            if (endDownSensor.isSensorDisabled()) return;

            // If bot comes an edge
            if (!endDownSensor.State())
            {
                ChangePatrolDirection();
            }
            // if bot comes to a wall
            if (endDownSensor.State() && endUpSensor.State())
            {
                ChangePatrolDirection();
            }
        }
        else if (!isWalkingRight && !isStopped)
        {
            movement = -movementSpeed / 8;

            // Don't check sensors when they are disabled
            if (endDownSensor.isSensorDisabled()) return;

            // If bot comes an edge
            if (!endDownSensor.State())
            {
                ChangePatrolDirection();
            }
            // if bot comes to a wall
            if (endDownSensor.State() && endUpSensor.State())
            {
                ChangePatrolDirection();
            }
        }

        else if (isStopped) { movement = 0; }

        // Patrol State Change - Making a natural patrolling
        if (!isChasing)
        {
            if (isStopped)
            {   // Check if our stop time is finished
                if (patrolStopTime < 0)
                {
                    // Take new change time for actions
                    patrolChangeTime = Random.Range(5, patrolMaxChangeTime);

                    // Change the direction by 30% chance
                    if (Random.Range(0f, 10f) < 3)
                    {
                        if (isWalkingRight) isWalkingRight = false;
                        else isWalkingRight = true;
                    }

                    isStopped = false;
                }
                else patrolStopTime -= Time.deltaTime; // If not finished, then decrease
            }
            else  // if we are on move
            {   // Check if our patrol time is finihed
                if (patrolChangeTime < 0)
                {
                    // Take new stop time
                    patrolStopTime = Random.Range(3, patrolMaxStopTime);
                    isStopped = true;
                }
                else patrolChangeTime -= Time.deltaTime; // If not finished, then decrese the time
            }
        }
    }

    private void ChangePatrolDirection()
    {
        if (isWalkingRight)
        {
            isWalkingRight = false;
            endDownSensor.Disable(0.1f);
            endUpSensor.Disable(0.1f);
        }
        else
        {
            isWalkingRight = true;
            endDownSensor.Disable(0.1f);
            endUpSensor.Disable(0.1f);
        }

        // Get new patrol time because we have turned into a new direction
        patrolChangeTime = Random.Range(5, patrolMaxChangeTime);
    }

    private void Actions()
    {
        if (!canAnimate) return; // If we can't animate a new movement, then doesn't care about the input

        if (isDefending)
        {
            animator.SetBool(DEFENDING, true);      // Set animator
            rigidbody.velocity = new Vector2(0, 0); // Stop the char
            return;
        }
        else
        {
            animator.SetBool(DEFENDING, false);     // Set animator
        }

        ///// FLIP /////
        // Change the facing direction according to input
        if (rigidbody.velocity.x > 0f) FlipCharacter(true);
        else if (rigidbody.velocity.x < 0f) FlipCharacter(false);

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
        }



        ///  ************************   ///
        ///         Animations          ///
        ///  ************************   ///

        ///// Move /////
        animator.SetFloat(SPEED, velocityABS);

        // Jump
        animator.SetBool(ON_AIR, !grounded);

        #region Fighting Animation Checks
        // Punching //
        if (isRunPunching)
        {
            animator.SetTrigger(PUNCH_RUN);
            isRunPunching = false;
        }
        if (isPunching)
        {
            animator.SetTrigger(PUNCH_HIT);
            isPunching = false;
            canAnimate = false;

            // Make player stop to avoid unwanted moves
            rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);
        }
        // Kicking //
        if (isFlyKicking)
        {
            animator.SetTrigger(FLYING_KICK);

            // Adjusting fly settings. With gravity change, we have more natural fly
            if (rigidbody.velocity.x > 0) rigidbody.velocity = new Vector2(flyingKickForce, flyingKickUp);
            else rigidbody.velocity = new Vector2(-flyingKickForce, flyingKickUp);


            isFlyKicking = false;
            canAnimate = false;
        }
        if (isKicking)
        {
            animator.SetTrigger(KICK);
            isKicking = false;
            canAnimate = false;
        }
        if (isTurningKicking)
        {
            animator.SetTrigger(TURNING_KICK);

            rigidbody.velocity = new Vector2(rigidbody.velocity.x, flyingKickUp);

            isTurningKicking = false;
            canAnimate = false;
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
    /*
    private void OnDrawGizmos()
    {
        Vector2 drawGismos = new Vector2(turningKickHitLocation.position.x, turningKickHitLocation.position.y);
        Gizmos.DrawWireSphere(drawGismos, turningKickHitRange);

        if (facingRightInt > 0) 
            drawGismos = new Vector2(turningKickHitLocation.position.x - 2.6f, turningKickHitLocation.position.y);
        else
            drawGismos = new Vector2(turningKickHitLocation.position.x + 2.6f, turningKickHitLocation.position.y);
        Gizmos.DrawWireSphere(drawGismos, turningKickHitRange); 
    }*/

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
            enemy.GetComponent<StickBot>().TakenDamage(FLYING_KICK, flyingKickHitPoint, damageFromRight);
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
            enemy.GetComponent<StickBot>().TakenDamage(TURNING_KICK, turningKickHitPoint, damageFromRight);
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
        if (!canAnimate || isDefending) return; // If the char even can't move, don't take any damage

        health -= _takenDamagePoint;
        if (health <= 0f)
        {
            isDying = true;
            deathType = _takenDamageType;
        }

        // Stop further animations and let the damage animation plays
        canAnimate = false;
        rigidbody.velocity = Vector3.zero; // And stop the character completely

        // Flip the character to the direction where the damage comes from
        if (_DamageDirection && facingRightInt != 1) FlipCharacter(true);
        if (!_DamageDirection && facingRightInt != -1) FlipCharacter(false);

        // Animate
        if (_takenDamageType == PUNCH_HIT || _takenDamageType == PUNCH_RUN) animator.SetTrigger(DAMAGE_HEAD);
        else if (_takenDamageType == FLYING_KICK || _takenDamageType == TURNING_KICK) animator.SetTrigger(KICK_FALL);
        else animator.SetTrigger(DAMAGE_DOWN);

        // Make attack sound (even though we got attacked, we make it)
        FindObjectOfType<AudioManager>().PlayAttackSound();

        // Move away according to hit type
        if (_takenDamageType == PUNCH_RUN)
            rigidbody.velocity = new Vector2(-takenPunchMove * facingRightInt, rigidbody.velocity.y);
        if (_takenDamageType == FLYING_KICK || _takenDamageType == TURNING_KICK)
            rigidbody.velocity = new Vector2(-takenKickMove * facingRightInt, rigidbody.velocity.y);
    }
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

    IEnumerator DestroyLater()
    {
        // Place death SFX here

        yield return new WaitForSeconds(10f);

        Destroy(gameObject);
    }

}
