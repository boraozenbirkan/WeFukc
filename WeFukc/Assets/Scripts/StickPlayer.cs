using UnityEngine;

public class StickPlayer : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] private float movementSpeed = 20f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float flyingKickForce = 5f;
    [SerializeField] private float flyingKickUp = 5f;
    [SerializeField] private float flyingKickGravityScale = 5f;
    [SerializeField] private float TurningKickGravityScale = 1f;

    [Header("Components")]
    [SerializeField] private StickSensor groundSensor;

    private Animator animator;
    private Rigidbody2D rigidbody;

    private bool grounded = false;
    private bool facingRight = true;
    public bool canAnimate = true;
    private float inputX = 0;
    private float movement = 0;
    private float defaultGravityScale = 0;

    private bool isPunching = false;
    private bool isRunPunching = false;
    private bool isKicking = false;
    private bool isFlyKicking = false;
    private bool isTurningKicking = false;
    private bool isJumping = false;
    private bool isDefending = false;


    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        defaultGravityScale = rigidbody.gravityScale;
    }



    private void Update()
    {
        ///  ************************   ///
        ///        Status Check         ///
        ///  ************************   ///
        
        
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
                Debug.Log("Landed !!"); // Animate
            }
            //I was on land but now I am not
            else if (grounded && !groundSensor.State())
            {
                grounded = false;
                Debug.Log("On air");
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

        // Punching //
        if (Input.GetKeyDown("j"))
        {
            if (Mathf.Abs(inputX) > 0) isRunPunching = true;
            else
            {
                isPunching = true;
            }
        } // Kicking //
        else if (Input.GetKeyDown("l"))
        {
            if (Mathf.Abs(inputX) > 0 && grounded) isFlyKicking = true; // Movenign and on the ground
            else if (grounded) isKicking = true;    // Not moving but on the ground
            else isTurningKicking = true;           // None of them = On air
        } // Defense //
        else if (Input.GetKeyDown("k")) isDefending = true;

        if (Input.GetKeyUp("k")) isDefending = false;
    }

    private void Actions()
    {
        if (!canAnimate) return; // If we can't animate a new movement, then doesn't care about the input

        rigidbody.gravityScale = defaultGravityScale;


        if (isDefending)
        {
            animator.SetBool("Defending", true);
            rigidbody.velocity = new Vector2(0, 0);
            return;
        }
        else
            animator.SetBool("Defending", false);

        ///// FLIP /////
        // Change the facing direction according to input
        if (inputX > 0) transform.localScale = new Vector2(1, 1);
        else if (inputX < 0) transform.localScale = new Vector2(-1, 1);

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
        animator.SetFloat("Speed", Mathf.Abs(movement));

        // Jump
        animator.SetBool("OnAir", !grounded);

        // Punching //
        if (isRunPunching)
        {
            animator.SetTrigger("RunningPunch");
            isRunPunching = false;
        }
        if (isPunching)
        {
            animator.SetTrigger("Punch");
            isPunching = false;
            canAnimate = false;
        }
        // Kicking //
        if (isFlyKicking)
        {
            animator.SetTrigger("FlyingKick");

            // Adjusting fly settings. With gravity change, we have more natural fly
            if (inputX > 0) rigidbody.velocity = new Vector2(flyingKickForce, flyingKickUp);
            else rigidbody.velocity = new Vector2(-flyingKickForce, flyingKickUp);
            rigidbody.gravityScale = flyingKickGravityScale;

            isFlyKicking = false;
            canAnimate = false;
        }
        if (isKicking)
        {
            animator.SetTrigger("Kick");
            isKicking = false;
            canAnimate = false;
        }
        if (isTurningKicking)
        {
            animator.SetTrigger("TurningKick");

            rigidbody.velocity = new Vector2(0, rigidbody.velocity.y/3);
            rigidbody.gravityScale = TurningKickGravityScale;

            isTurningKicking = false;
            canAnimate = false;
        }



    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }



    private void PunchHit()
    {
        Debug.Log("Punched!");
    }
    private void toggleCanAnimate() { canAnimate = !canAnimate; }
}
