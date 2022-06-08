using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Sprite jumpUpSprite;
    [SerializeField] Sprite jumpDownSprite;

    [SerializeField] float runSpeed = 15f;

    CharacterController2D characterController;
    Rigidbody2D rigidbody2D;
    SpriteRenderer spriteRenderer;
    Animator animator;

    float horizontalMove = 0f;
    bool jump = false;
    bool crouch = false;
    float rigidbodyVelocity;

    private void Start()
    {
        characterController = GetComponent<CharacterController2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        } else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

        // Jump animation: Render up image when going up and vice verse.
        rigidbodyVelocity = rigidbody2D.velocity.y;
        if (rigidbody2D && (rigidbodyVelocity > 0.1f || rigidbodyVelocity < -0.1f))
        {
            if (rigidbodyVelocity > Mathf.Epsilon)
            {
                animator.enabled = false;
                spriteRenderer.sprite = jumpUpSprite;
            }
            if (rigidbodyVelocity < Mathf.Epsilon)
            {
                animator.enabled = false; 
                spriteRenderer.sprite = jumpDownSprite;
            }
        }
        else { animator.enabled = true; }
    }

    void FixedUpdate()
    {
        characterController.Move(horizontalMove * Time.deltaTime, crouch, jump);
        jump = false;
    }

    public void IsCrouching(bool _isCrouching)
    {
        animator.SetBool("isCrouching", _isCrouching);
    }

}
