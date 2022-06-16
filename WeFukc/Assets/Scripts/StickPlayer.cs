using UnityEngine;

public class StickPlayer : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] private float movementSpeed = 20f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float flyingKickForce = 5f;
    [SerializeField] private float flyingKickUp = 5f;

    [Header("Components")]
    [SerializeField] private StickSensor groundSensor;
    [SerializeField] private GameObject DeadBody;
    [SerializeField] private GameObject DeadBody_Head;
    [SerializeField] private GameObject DeadBody_Leg;

    private Animator animator;
    private Rigidbody2D rigidbody;

    private bool grounded = false;
    public bool canAnimate = true;
    private float inputX = 0;
    private float movement = 0;

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
    }



    private void Update()
    {
        ///  ************************   ///
        ///        Status Check         ///
        ///  ************************   ///
        ///  
        /*
        if (Input.GetKeyDown("4"))  // Down Damage
        {
            DeadBody.SetActive(true);
            DeadBody_Leg.GetComponent<Rigidbody2D>().AddForce(new Vector2(-1500, 0));
            DeadBody_Head.GetComponent<Rigidbody2D>().AddForce(new Vector2(500, 0));
        }
        if (Input.GetKeyDown("6")) // HEad Damage
        {
            DeadBody.SetActive(true);
            DeadBody_Head.GetComponent<Rigidbody2D>().AddForce(new Vector2(-500, 0));
        }*/
        
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
            if (Mathf.Abs(inputX) > 0) isRunPunching = true;
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

            rigidbody.velocity = new Vector2(rigidbody.velocity.x, flyingKickUp);

            isTurningKicking = false;
            canAnimate = false;
        }



    }

    ///  ************************   ///
    ///        Giving Damage        ///
    ///  ************************   ///
    private void PunchHit()
    {
        Debug.Log("Punched!");
    }
    private void toggleCanAnimate() { canAnimate = !canAnimate; }
}
