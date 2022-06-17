using UnityEngine;
using System.Collections;

public class StickPlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 20f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float flyingKickForce = 5f;
    [SerializeField] private float flyingKickUp = 5f;
    [SerializeField] private StickSensor groundSensor;

    [Header("Specs")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float punchHitPoint = 5f;
    [SerializeField] private float punchRunHitPoint = 5f;
    [SerializeField] private float kickHitPoint = 5f;
    [SerializeField] private float flyingKickHitPoint = 5f;
    [SerializeField] private float turningKickHitPoint = 5f;

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
    [SerializeField] private float takenPunchMove = 5f;
    [SerializeField] private float takenKickMove = 20f;

    private Animator animator;
    private Rigidbody2D rigidbody;

    // Health and Stamina
    private float health;
    private bool isDying = false;
    private string deathType;

    // Movement vars
    private bool grounded = false;
    private bool canAnimate = true;
    private float inputX = 0f;
    private float movement = 0f;

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


    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();

        health = maxHealth;
    }



    private void Update()
    {
        ///  ************************   ///
        ///        Status Check         ///
        ///  ************************   ///
        ///  

        // Debug Fights
        //if (Input.GetMouseButtonDown(0)) { TakenDamage(PUNCH_RUN, 5); }
        //if (Input.GetMouseButtonDown(1)) { TakenDamage(FLYING_KICK, 5); }
        
        StatusCheck();
        Actions();
    }
    private void FixedUpdate()
    {
        ///  ************************   ///
        ///          EXECUTIONS         ///
        ///  ************************   ///

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

        if (Input.GetButtonDown("Jump") && grounded)
        {
            isJumping = true;
        }

        ///// Fight /////
        if (!grounded) return;  // On air, not get fighting input
        // Punching //
        if (Input.GetKeyDown("j"))
        {
            if (Mathf.Abs(rigidbody.velocity.x) > 10) isRunPunching = true; // run punch after vel > 10
            else
            {
                isPunching = true;
            }
        } // Kicking //
        else if (Input.GetKeyDown("l")) 
        {
            if (Input.GetKey("w") && Mathf.Abs(rigidbody.velocity.x) < 2) isTurningKicking = true; //isTurningKicking = true;                 // if w key also pressed, turning kick
            else if (Mathf.Abs(inputX) > 0 && grounded) isFlyKicking = true;    // if player is moving, they fly kick
            else isKicking = true;           // None of them = Normal kick
        } // Defense //
        else if (Input.GetKeyDown("k")) isDefending = true;

        if (Input.GetKeyUp("k")) isDefending = false;
    }

    private void Actions()
    {
        if (!canAnimate) return; // If we can't animate a new movement, then doesn't care about the input

        if (isDefending)
        {
            animator.SetBool(DEFENDING, true);
            rigidbody.velocity = new Vector2(0, 0);
            return;
        }
        else
            animator.SetBool(DEFENDING, false);

        ///// FLIP /////
        // Change the facing direction according to input
        if (rigidbody.velocity.x > 0) transform.localScale = new Vector2(1, 1);
        else if (rigidbody.velocity.x < 0) transform.localScale = new Vector2(-1, 1);

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
        animator.SetFloat(SPEED, Mathf.Abs(rigidbody.velocity.x));

        // Jump
        animator.SetBool(ON_AIR, !grounded);

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
            if (inputX > 0) rigidbody.velocity = new Vector2(flyingKickForce, flyingKickUp);
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



    }

    private void toggleCanAnimate() { canAnimate = !canAnimate; }
    /*
    private void OnDrawGizmos()
    {
        Vector2 drawGismos = new Vector2(turningKickHitLocation.position.x + 2.6f, turningKickHitLocation.position.y);
        Gizmos.DrawWireSphere(drawGismos, turningKickHitRange);
    }
    */
    ///  ************************   ///
    ///        Giving Damage        ///
    ///  ************************   ///
    private void PunchHit()
    {
        hitEnemies = Physics2D.OverlapCircleAll
            (punchHitLocation.position, punchHitRange, enemyLayers);
        
        foreach (Collider2D hit in hitEnemies)
        {
            hit.GetComponent<StickBot>().TakenDamage(PUNCH_HIT, punchHitPoint);
        }
    }

    private void PunchRunHit()
    {   // Punch run uses the same location as normal punch but has different hit points
        hitEnemies = Physics2D.OverlapCircleAll
            (punchHitLocation.position, punchHitRange, enemyLayers);

        foreach (Collider2D hit in hitEnemies)
        {
            hit.GetComponent<StickBot>().TakenDamage(PUNCH_RUN, punchRunHitPoint);
        }
    }
    private void KickHit()
    {
        hitEnemies = Physics2D.OverlapCircleAll
            (kickHitLocation.position, kickHitRange, enemyLayers);

        foreach (Collider2D hit in hitEnemies)
        {
            hit.GetComponent<StickBot>().TakenDamage(KICK, kickHitPoint);
        }
    }
    private void FlyingKickHit()
    {
        hitEnemies = Physics2D.OverlapCircleAll
            (flyingKickHitLocation.position, flyingKickHitRange, enemyLayers);

        foreach (Collider2D hit in hitEnemies)
        {
            hit.GetComponent<StickBot>().TakenDamage(FLYING_KICK, flyingKickHitPoint);
        }
    }
    private void TurningKickHit()
    {

        if (!didKickFront)  // Kick the front first
        {
            hitEnemies = Physics2D.OverlapCircleAll
            (turningKickHitLocation.position, turningKickHitRange, enemyLayers);

            didKickFront = true;
        }
        else  // Then animation will call this second time. Kick the back then.
        {
            Vector2 backLocation = new Vector2    // Get the back location
                (turningKickHitLocation.position.x - 2.6f, turningKickHitLocation.position.y);

            hitEnemies = Physics2D.OverlapCircleAll
            (backLocation, turningKickHitRange, enemyLayers);

            didKickFront = false; // Reset the var
        }

        foreach (Collider2D hit in hitEnemies)
        {
            hit.GetComponent<StickBot>().TakenDamage(FLYING_KICK, flyingKickHitPoint);
        }
    }

    ///  ************************   ///
    ///       Taking Damage         ///
    ///  ************************   ///

    // TakenDamage is a one-size-fits-all method
    public void TakenDamage(string _takenDamageType, float _takenDamagePoint)
    {
        health -= _takenDamagePoint; 
        if (health <=    0)
        {
            isDying = true;
            deathType = _takenDamageType;
        }

        // Stop further animations and let the damage animation plays
        canAnimate = false;
        rigidbody.velocity = Vector3.zero; // And stop the character completely

        // Animate
        if (_takenDamageType == PUNCH_HIT || _takenDamageType == PUNCH_RUN) animator.SetTrigger(DAMAGE_HEAD);
        else animator.SetTrigger(DAMAGE_DOWN);

        // Move according to hit type
        if (_takenDamageType == PUNCH_RUN) 
            rigidbody.velocity = new Vector2(takenPunchMove, rigidbody.velocity.y);
        if (_takenDamageType == FLYING_KICK || _takenDamageType == TURNING_KICK) 
            rigidbody.velocity = new Vector2(takenKickMove, rigidbody.velocity.y);
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
