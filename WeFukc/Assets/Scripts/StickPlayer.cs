using UnityEngine;

public class StickPlayer : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] private float movementSpeed = 20f;
    [SerializeField] private float jumpForce = 20f;

    [Header("Components")]
    [SerializeField] private StickSensor groundSensor;

    private Animator animator;
    private Rigidbody2D rigidbody;

    private bool grounded = false;
    private bool facingRight = true;
    private float inputX = 0;
    private float movement = 0;


    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
    }



    private void Update()
    {
        GroundStatusCheck();
        MoveAndJump();

        if (Input.GetMouseButtonDown(0))
        {
            if (Mathf.Abs(inputX) > 0) animator.SetTrigger("RunningPunch");
            else animator.SetTrigger("Punch");
        }
    }

    private void GroundStatusCheck()
    {
        // Don't check the ground if the player is going up
        if (rigidbody.velocity.y > 0f) { return; }

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

    private void MoveAndJump()
    {
        // Get the input
        inputX = Input.GetAxis("Horizontal");
        movement = inputX * movementSpeed;

        // Move left and right, but if I was not on wall, make it faster to jump the other wall
        rigidbody.velocity = new Vector2(movement, rigidbody.velocity.y);

        // Change the facing direction according to input
        if (inputX > 0 && !facingRight) Flip();           // flip if I move right but not facing right
        else if (inputX < 0 && facingRight) Flip();       // Vice versa

        // Jump if you are on ground
        if (Input.GetButtonDown("Jump") && grounded)
        {
            grounded = false;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpForce);
            groundSensor.Disable(0.2f);
            Debug.Log("Normal Jump"); // Animate same as falling
        }

        // Animations
        // Move
        animator.SetFloat("Speed", Mathf.Abs(movement));
        // Set animator speed to have correlated animation with the speed.
        if (Mathf.Abs(movement) > 0.01)
        {
            animator.speed = Mathf.Abs(movement) / movementSpeed;
        }
        else animator.speed = 1;

        // Jump
        animator.SetBool("OnAir", !grounded);
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
}
