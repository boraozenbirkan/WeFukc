using UnityEngine;
using System.Collections;

public class StickBot : MonoBehaviour
{
    [Header("Movement")]
    private float movementSpeed = 10f;
    private float jumpForce = 30f;
    private float flyingKickForce = 15f;
    private float flyingKickUp = 15f;
    [SerializeField] private StickSensor groundSensor;
    [SerializeField] private StickSensor endUpSensor;
    [SerializeField] private StickSensor endDownSensor;
    [SerializeField] private StickSensor jumpUpSensor;
    [SerializeField] private StickSensor flyKickSensor;
    [SerializeField] private GameObject[] bodyColor;

    private float maxHealth = 100f;
    private float punchHitPoint = 5f;
    private float punchRunHitPoint = 10f;
    private float kickHitPoint = 5f;
    private float flyingKickHitPoint = 15f;
    private float turningKickHitPoint = 25f;

    [Header("Death Components")]
    [SerializeField] private BoxCollider2D characterCollider;
    [SerializeField] private GameObject deadBody;
    [SerializeField] private GameObject deadBody_Head;
    [SerializeField] private GameObject deadBody_Leg;
    [SerializeField] private GameObject[] originalBodyParts;
    [SerializeField] private GameObject[] deathBodyColor;
    [SerializeField] private GameObject key;

    [Header("Fight Components")]
    [SerializeField] private Transform punchHitLocation;
    [SerializeField] private Transform kickHitLocation;
    [SerializeField] private Transform flyingKickHitLocation;
    [SerializeField] private Transform turningKickHitLocation;
    private float punchHitRange = 1f;
    private float kickHitRange = 0.8f;
    private float flyingKickHitRange = 0.7f;
    private float turningKickHitRange = 2f;
    [SerializeField] private LayerMask enemyLayers;
    private float takenPunchMove = 2f;
    private float takenKickMove = 5f;
    [SerializeField] private FloatingDamage damageCanvas;

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
    private bool allowDamage = true;
    private float movement = 0f;
    private float facingRightInt = 0f;

    // Fighting vars
    private bool isPunching = false;
    private bool isRunPunching = false;
    private bool isKicking = false;
    private bool isFlyKicking = false;
    private bool isTurningKicking = false;
    private bool isJumping = false;
    private bool isDefending = false;
    private bool didKickFront = false;
    private Collider2D[] hitEnemies;
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
    private const string CHASE = "isChasing";
    private const string CLOSE_COMBAT = "CloseCombat";
    private const string FACE_TO_FACE = "FaceToFace";

    // Bot vars
    private bool isWalkingRight = false;
    private bool isChasing = false;
    private bool isStopped = false;
    public bool isKeyAssigned = false;

    private float patrolMaxChangeTime = 20f;   // Change time 5-20
    private float patrolMaxStopTime = 10f;     // Stop time 3-10
    private float patrolChangeTime;   // Change time 5-25
    private float patrolStopTime;     // Stop time 3-10

    private StickPlayer target = null;
    private Collider2D targetCollider = null;
    public float approachDistance = 5;

    [Header("Bot Specs")]
    private float hitDelay;
    private float currentHitDelay;
    private bool canFlyKick = false;
    private int flyKickChance = 2;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float followDistance = 30f;
    [SerializeField] private bool showRangeAndDistance = false;
    [SerializeField] [Range(1, 15)] private int botTier = 1;

    /*
     * Tier Colors: 
     * 1-3 Green, 0, 176, 80 
     * 4-6 Blue, 68, 114, 196
     * 7-9 Yellowish, 255, 192, 0
     * 10-12 Orange, 237, 125, 49
     * 13-15 Red, 192, 0, 0
     * 
     */
    Color32 tierColor1_3 = new Color32(0, 176, 80, 255);
    Color32 tierColor4_6 = new Color32(68, 114, 196, 255);
    Color32 tierColor7_9 = new Color32(255, 192, 0, 255);
    Color32 tierColor10_12 = new Color32(237, 125, 49, 255);
    Color32 tierColor12_15 = new Color32(192, 0, 0, 255);



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

        // Change max speed with a random number to avoid sentetic view of bots when they act together
        movementSpeed = Random.Range(8f, 15f);
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

        States();
        Actions();
    }

    private void States()
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

        if (!isChasing)
        {   
            // If we are not chasing, check the range if there is any
            targetCollider = Physics2D.OverlapBox(transform.position, new Vector2(detectionRange, 3), 0f, enemyLayers);

            if (targetCollider != null)
            {
                target = targetCollider.GetComponent<StickPlayer>();
                isChasing = true;
                animator.SetBool(CHASE, true);
            }
            else    // If there no enemy, then patrol
            {
                if (isWalkingRight && !isStopped)
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
                else if (isStopped) { 
                    movement = 0;

                    // Check if our stop time is finished
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
                else
                {
                    // Check if our patrol time is finihed
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
        // If we are chasing
        if (isChasing)
        {
            // if we lost the target then return
            if (target == null) { isChasing = false; return; }

            // Check if the target is still in range or not
            float distance = Vector2.Distance(target.transform.position, transform.position);
            if (distance > followDistance)
            {
                isChasing = false;  // Don't chase
                animator.SetBool(CHASE, false);
                return;             // Don't execute below code
            }


            // Decrease hit delay
            currentHitDelay -= Time.deltaTime;

            // Check the direction
            Vector2 targetPos = target.transform.position;
            Vector2 myPos = transform.position;



            // MOVING TO RIGHT

            if (targetPos.x > myPos.x) // If target is on the right
            {
                // Set speed according to distance
                float xDistance = Mathf.Abs(targetPos.x - myPos.x);

                movement = movementSpeed; // Normal Chasing speed
                animator.SetBool(CLOSE_COMBAT, false);
                animator.SetBool(FACE_TO_FACE, false);

                if (!canAnimate || !grounded) return;   // if can't animate or on air, don't take action

                // If we are approaching
                if (xDistance < approachDistance)
                {
                    movement = movementSpeed / 8;   // Walking / Approaching speed
                    animator.SetBool(CLOSE_COMBAT, true);

                    // Fly Kick Here
                    if (currentHitDelay < 0 && canFlyKick && Random.Range(0, 10) < flyKickChance && flyKickSensor.State())
                    {
                        if (Mathf.Abs(targetPos.y - myPos.y) < 3f)
                        {
                            currentHitDelay = hitDelay;     // Reset the hit delay
                            isFlyKicking = true;
                        }
                    }

                    if (xDistance < 2.5f)
                    {
                        movement = 0;    // If we are face-to-face then stop
                        animator.SetBool(FACE_TO_FACE, true);

                        // Kick and Punch here
                        if (currentHitDelay < 0)
                        {
                            if (Mathf.Abs(targetPos.y - myPos.y) < 3f)
                            {
                                currentHitDelay = hitDelay; // Reset the hit delay
                                // Kick or Punch by 50%
                                if (Random.Range(0, 10) < 5) isKicking = true;
                                else isPunching = true;
                            }
                        }
                    }
                }

                // JUMP Conditions

                if (!canAnimate || !grounded) return;   // if can't animate or on air, don't jump

                // If bot is too close to target, don't even check for jump
                if (xDistance < 3f) return;

                // Check if jump needed
                if ((((targetPos.y - myPos.y) > 5) && jumpUpSensor.State() && grounded)         // Needed for up?
                    || (!jumpUpSensor.State() && endDownSensor.State() && endUpSensor.State() && grounded) // need to follow but faced with obstacle. 
                    || (((targetPos.y - myPos.y) < -5) && !endDownSensor.State() && grounded))  // or down?
                {
                    isJumping = true;
                }
            }

            // MOVING TO LEFT
            else
            {
                float xDistance = Mathf.Abs(targetPos.x - myPos.x);

                movement = -movementSpeed; // Normal Chasing speed
                animator.SetBool(CLOSE_COMBAT, false);
                animator.SetBool(FACE_TO_FACE, false);

                if (!canAnimate || !grounded) return;   // if can't animate or on air, don't take action

                // If we are approaching
                if (xDistance < approachDistance)
                {
                    movement = -movementSpeed / 8;   // Walking / Approaching speed
                    animator.SetBool(CLOSE_COMBAT, true);

                    // Fly Kick Here
                    if (currentHitDelay < 0 && canFlyKick && Random.Range(0, 10) < flyKickChance && flyKickSensor.State())
                    {
                        if (Mathf.Abs(targetPos.y - myPos.y) < 3f)
                        {
                            currentHitDelay = hitDelay;     // Reset the hit delay
                            isFlyKicking = true;
                        }
                    }   


                    if (xDistance < 2.5f)
                    {
                        movement = 0;    // If we are face-to-face then stop
                        animator.SetBool(FACE_TO_FACE, true);

                        // Kick and Punch here
                        if (currentHitDelay < 0)
                        {
                            if (Mathf.Abs(targetPos.y - myPos.y) < 3f)
                            {
                                currentHitDelay = hitDelay; // Reset the hit delay
                                // Kick or Punch by 50%
                                if (Random.Range(0, 10) < 5) isKicking = true;
                                else isPunching = true;
                            }
                        }
                    }
                }

                if (!canAnimate || !grounded) return;   // if can't animate or on air, don't jump

                // If bot is too close to target, don't even check for jump
                if (xDistance < 3f) return;

                // Check if jump needed
                if ((((targetPos.y - myPos.y) > 5) && jumpUpSensor.State() && grounded)         // Needed for up?                    
                    || (!jumpUpSensor.State() && endDownSensor.State() && endUpSensor.State() && grounded) // need to follow but faced with obstacle. 
                    || (((targetPos.y - myPos.y) < -5) && !endDownSensor.State() && grounded))  // or down?
                {
                    isJumping = true;
                }
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
        allowDamage = true; // if we move, we can get damage, some bug fix ;)

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
        animator.SetFloat(SPEED, Mathf.Abs(rigidbody.velocity.x));

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

    private void toggleCanAnimate() { 
        canAnimate = !canAnimate;
        currentHitDelay += 1f; // Add a small delay after completing an action
    }
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
        //Vector2 drawGismos = new Vector2(turningKickHitLocation.position.x, turningKickHitLocation.position.y);
        if (showRangeAndDistance)
        {
            //Gizmos.DrawSphere(flyingKickHitLocation.position, flyingKickHitRange);
            
            Gizmos.DrawWireSphere(transform.position, followDistance);
            Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange, 3f, 0f));
            
        }
        
        //Physics2D.OverlapBoxAll(transform.position, new Vector2(maxRange, 5), 0f, enemyLayers);
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
            enemy.GetComponent<StickPlayer>().TakenDamage(PUNCH_HIT, punchHitPoint, damageFromRight);
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
            enemy.GetComponent<StickPlayer>().TakenDamage(PUNCH_RUN, punchRunHitPoint, damageFromRight);
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
            enemy.GetComponent<StickPlayer>().TakenDamage(KICK, kickHitPoint, damageFromRight);
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
            if (enemy) enemy.GetComponent<StickPlayer>().TakenDamage(FLYING_KICK, flyingKickHitPoint, damageFromRight);
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
            enemy.GetComponent<StickPlayer>().TakenDamage(TURNING_KICK, turningKickHitPoint, damageFromRight);
        }

        if (hitEnemies.Length < 1) FindObjectOfType<AudioManager>().PlaySFX("Attack_Miss");
    }
    private void KickFallBackUp()
    {   // Push forward when its standing up again
        rigidbody.velocity = new Vector2(takenPunchMove * facingRightInt, rigidbody.velocity.y);
        allowDamage = true;
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
        if (!allowDamage || isDefending) return; // If the char even can't move, don't take any damage

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
        if (_takenDamageType == PUNCH_HIT || _takenDamageType == PUNCH_RUN)
        {
            animator.SetTrigger(DAMAGE_HEAD);
        }
        else if (_takenDamageType == FLYING_KICK || _takenDamageType == TURNING_KICK)
        {
            animator.SetTrigger(KICK_FALL);
            allowDamage = false;
        }
        else animator.SetTrigger(DAMAGE_DOWN);

        // Make attack sound (even though we got attacked, we make it)
        FindObjectOfType<AudioManager>().PlayAttackSound();

        // Floating Damage
        Instantiate(damageCanvas, transform.position, Quaternion.identity).
            damageText.text = "-" + _takenDamagePoint.ToString();

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

        // Drop the key if assigned
        if (isKeyAssigned)
        {
            Instantiate(key, transform.position, Quaternion.identity);
        }

        // Remove the dead body later
        StartCoroutine(DestroyLater());
    }

    public void CertainDeath() { health = 0f; isDying = true; CheckDeath(); }
    IEnumerator DestroyLater()
    {
        yield return new WaitForSeconds(10f);

        Destroy(gameObject);
    }

    IEnumerator DestoryObject(GameObject _objectToDestroy, float _destroyTime)
    {
        yield return new WaitForSeconds(_destroyTime);
        Destroy(_objectToDestroy);
    }


    public void SetTier(int tier)
    {
        switch (tier)
        {
            case 1:
                botTier = 1;
                maxHealth = 30f;
                detectionRange = followDistance = 15f;
                foreach (GameObject bodyPart in bodyColor)
                {
                    bodyPart.GetComponent<SpriteRenderer>().color = tierColor1_3;
                }
                foreach (GameObject bodyPart in deathBodyColor)
                {
                    bodyPart.GetComponent<SpriteRenderer>().color = tierColor1_3;
                }

                hitDelay = 4f;
                canFlyKick = false;
                break;

            case 2:
                botTier = 2;
                maxHealth = 60f;
                detectionRange = followDistance = 30f;
                foreach (GameObject bodyPart in bodyColor)
                {
                    bodyPart.GetComponent<SpriteRenderer>().color = tierColor1_3;
                }
                foreach (GameObject bodyPart in deathBodyColor)
                {
                    bodyPart.GetComponent<SpriteRenderer>().color = tierColor1_3;
                }

                hitDelay = 3f;
                canFlyKick = true;
                flyKickChance = 2;
                break;

            case 3:
                botTier = 3;
                maxHealth = 90f;
                detectionRange = followDistance = 45f;
                foreach (GameObject bodyPart in bodyColor)
                {
                    bodyPart.GetComponent<SpriteRenderer>().color = tierColor1_3;
                }
                foreach (GameObject bodyPart in deathBodyColor)
                {
                    bodyPart.GetComponent<SpriteRenderer>().color = tierColor1_3;
                }

                hitDelay = 1f;
                canFlyKick = true;
                flyKickChance = 4;
                break;

            default:
                Debug.LogError("Tier undetected for " + gameObject.name);
                break;
        }
    }

}
