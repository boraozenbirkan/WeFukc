using UnityEngine;

public class StickPlayer : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] float movementSpeed = 20f;
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float sideJumpForceUp = 5f;
    [SerializeField] float sideJumpForceSide = 5f;

    [Header("Components")]
    [SerializeField] StickSensor groundSensor;
    [SerializeField] StickSensor leftSensor1;
    [SerializeField] StickSensor leftSensor2;
    [SerializeField] StickSensor rightSensor1;
    [SerializeField] StickSensor rightSensor2;




    Animator animator;
    Rigidbody2D rigidbody;
    BoxCollider2D boxCollider;
    PhysicsMaterial2D defaultMaterial;


    public bool grounded = false;
    public bool onWall = false;
    public bool wasOnWall = false;
    bool facingRight = true;
    float inputX = 0;
    StickSensor[] allSensors;



    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        defaultMaterial = boxCollider.sharedMaterial;

        allSensors = new StickSensor[] {groundSensor, leftSensor1, leftSensor2, rightSensor1, rightSensor2};
    }



    void Update()
    {
        //  ********                ********  //
        //            Status Check
        //  ********                ********  //

        GroundStatusCheck();
        // DEBUG
        //if (Mathf.Abs(rigidbody.velocity.y) > 20f) Debug.Log("Vel: " + rigidbody.velocity.y);
        WallHangingStatusCheck();


        //  ********                ********  //
        //              Execution    
        //  ********                ********  //

        MovingActions();
        JumpingActions();
    }

    private void GroundStatusCheck()
    {
        //I was on air but now I landed
        if (!grounded && groundSensor.State())
        {
            grounded = true;
            wasOnWall = false;
            onWall = false;
            Debug.Log("Landed !!"); // Animate
        }
        //I was on land but now I am not
        else if (grounded && !groundSensor.State())
        {
            grounded = false;
            Debug.Log("On air");
        }
    }

    private void WallHangingStatusCheck()
    {
        // Hand on the wall by sticking it
        if (!grounded &&
            ((leftSensor1.State() && leftSensor2.State()) || (rightSensor1.State() && rightSensor2.State())))
        {
            grounded = false;
            boxCollider.sharedMaterial = null;
            onWall = true;
            wasOnWall = false;
            if (Mathf.Abs(rigidbody.velocity.y) > 0 && Mathf.Abs(inputX) <= 0)
                Debug.Log("Falling!!"); // Animate
            else
                Debug.Log("Stikc to the WALL !!"); // Animate
        }
        else { boxCollider.sharedMaterial = defaultMaterial; onWall = false; }
    }

    private void MovingActions()
    {
        // Get the input
        inputX = Input.GetAxis("Horizontal");

        // Move left and right, but if I was on wall, make it faster to jump the other wall
        if (!wasOnWall || onWall) rigidbody.velocity = new Vector2(inputX * movementSpeed, rigidbody.velocity.y);

        // Change the facing direction according to input. Expect we are hangin on the wall!
        if (!onWall && !wasOnWall)
        {
            if (inputX > 0 && !facingRight) Flip();           // flip if I move right but not facing right
            else if (inputX < 0 && facingRight) Flip();       // Vice versa
        }
    }

    private void JumpingActions()
    {
        // Jump if you are on ground
        if (Input.GetButtonDown("Jump") && grounded)
        {
            grounded = false;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpForce);
            groundSensor.Disable(0.2f);
            Debug.Log("Normal Jump"); // Animate same as falling
        }
        else if (Input.GetButtonDown("Jump") && onWall)
        {
            if ((rightSensor1.State() || rightSensor2.State()))
            {
                // Enable default (slippery) material to leave the wall
                boxCollider.sharedMaterial = defaultMaterial;
                // Diable sensors briefly to leave the wall
                foreach (StickSensor sensor in allSensors) { sensor.Disable(0.2f); }
                // Then jump up move
                //rigidbody.velocity = new Vector2(rigidbody.velocity.x * 5, sideJumpForce);
                rigidbody.AddForce(new Vector2(-sideJumpForceSide, sideJumpForceUp), ForceMode2D.Impulse);
                Flip();

                onWall = false;
                wasOnWall = true;
                grounded = false;
                Debug.Log("Jump to Left"); // Animate same as stick to the wall
            }
            if ((leftSensor1.State() || leftSensor2.State()))
            {
                // Enable default (slippery) material to leave the wall
                boxCollider.sharedMaterial = defaultMaterial;
                // Diable sensors briefly to leave the wall
                foreach (StickSensor sensor in allSensors) { sensor.Disable(0.2f); }
                // Then jump up move
                //rigidbody.velocity = new Vector2(rigidbody.velocity.x * 5, sideJumpForce);
                rigidbody.AddForce(new Vector2(sideJumpForceSide, sideJumpForceUp), ForceMode2D.Impulse);
                Flip();

                onWall = false;
                wasOnWall = true;
                grounded = false;
                Debug.Log("Jump to Right"); // Animate same as stick to the wall
            }
        }
    }

    void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        // When we flip the whole object, we need to switch sensor to avoid annoying bug
        StickSensor tempL1 = leftSensor1;
        StickSensor tempL2 = leftSensor2;
        leftSensor1 = rightSensor1;
        leftSensor2 = rightSensor2;
        rightSensor1 = tempL1;
        rightSensor2 = tempL2;
    }
}
