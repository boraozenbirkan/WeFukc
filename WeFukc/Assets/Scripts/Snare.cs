using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snare : MonoBehaviour
{
    // Test vars



    // General
    private bool isPerforming = false;
    [SerializeField] private bool actionTest = false;

    // Damage Types
    private const string SNARE_HEAD = "SnareHead";
    private const string SNARE_DOWN = "SnareDown";
    private const string SNARE_BIG = "SnareBig";

    // Choose snare type
    [Header("Snare Type")]
    [SerializeField] private bool bottomSnare = false;
    [SerializeField] private bool turningBladeSnare = false;
    [SerializeField] private bool chainBladeSnare = false;
    [SerializeField] private bool stoneSnare = false;
    [SerializeField] private bool bladeSnare = false;

    // Turning Blade Snare
    [Header("Turning Blade")]
    [SerializeField] private float turningBlade_Range = 5f;
    [SerializeField] private float turningBlade_MoveSpeed = 5f;
    [SerializeField] private float turningBlade_MinSpawnTime = 10f;
    [SerializeField] private float turningBlade_MaxSpawnTime = 60f;
    private bool isTurningBladeMoving = false;
    private bool upComplete = false;
    private SpriteRenderer turningBladesprite;

    // Bottom Snare
    private bool bottomSnare_isMovingUp = false;
    private bool bottomSnare_upDone = false;
    private float bottomSnare_waitTime = 2f;

    // Stone Snare
    private bool didHit = false;

    // Blade snare
    private bool blade_movingDown = false;
    private bool balde_downDone = false;

    // Chain Blade
    private bool chainBlade_movingUp = false;
    private bool chainBlade_upDone = false;
    private float chainBlade_waitTime = 2f;
    private SpriteRenderer chainBladeSprite;


    /* Snare Information
     * 
     * Chain Snare
     * --- Randomize the action timing
     * --- Start and stop animator play back
     * - Its place is know.
     * 
     * Turning Blade
     * - Set min and max spawn time
     * 
     * 
     * 
     * - 
     * 
     * Standards
     * - Have isPerforming to avoid neccessary execution
     * - Set and reset isActionComplete in each action excet turning blade
     * 
     * 
     * DAMAGE
     * - Stone and Turning Blade: 50
     * - Blade and Chain Blade: 25
     * - Bottom Snares: 10
     * 
     */

    void Start()
    {
        if (bottomSnare || bladeSnare)
        {
            bottomSnare_waitTime = Random.Range(2f, 6f);
        }
        else if (turningBladeSnare)
        {
            turningBladesprite = GetComponentInChildren<SpriteRenderer>();  // Get the sprite to rotate
        }
        else if (chainBladeSnare)
        {
            chainBladeSprite = GetComponentInChildren<SpriteRenderer>();  // Get the sprite to rotate
            chainBlade_waitTime = Random.Range(0.5f, 5f);
        }
    }


    void Update()
    {
        if (bottomSnare)                BottomSnareActions();
        else if (turningBladeSnare)     TurningBladeActions();
        else if (chainBladeSnare)       ChainBladeActions();
        else if (bladeSnare)            BladeSnareActions();
    }

    // Collision
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Don't give damage if it is recently given or collider is not a player
        if (didHit || !collider.GetComponent<StickPlayer>()) return;

        if (bottomSnare)
        {
            // Decide the direction of the damage
            bool damageFromRight = false;
            if ((transform.position.x - collider.transform.position.x) > 0) damageFromRight = true;

            // Give damage
            collider.GetComponent<StickPlayer>().TakenDamage(SNARE_DOWN, 10f, damageFromRight);

            didHit = true;
        }
        else if (chainBladeSnare || bladeSnare)
        {
            // Decide the direction of the damage
            bool damageFromRight = false;
            if ((transform.position.x - collider.transform.position.x) > 0) damageFromRight = true;

            // Give damage
            collider.GetComponent<StickPlayer>().TakenDamage(SNARE_HEAD, 25f, damageFromRight);

            didHit = true;
        }
        else if (turningBladeSnare || stoneSnare)
        {
            // Decide the direction of the damage
            bool damageFromRight = false;
            if ((transform.position.x - collider.transform.position.x) > 0) damageFromRight = true;

            // Give damage
            collider.GetComponent<StickPlayer>().TakenDamage(SNARE_BIG, 50f, damageFromRight);

            didHit = true;
        }
    }

    // Special for stone because it has physics to perform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (stoneSnare)
        {
            // When hit the ground, play SFX
            if (collision.collider.gameObject.tag == "Ground")
            {
                FindObjectOfType<AudioManager>().PlaySFX("Impact_Fall");

                // Stop falling when hit the ground
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                GetComponent<Rigidbody2D>().gravityScale = 0;
            }

            // Destroy stone 8 seconds afer collision
            StartCoroutine(DestroyIn(8f));
        }
    }


    private void BottomSnareActions()
    {
        if (actionTest)
        {
            actionTest = false;
            didHit = false;         // we can hit again
            bottomSnare_isMovingUp = true;
            isPerforming = true;
        }

        if (!isPerforming) return; // Don't execute any code below if we are not assigned to perform

        float yPos = transform.localPosition.y;

        if (bottomSnare_isMovingUp)
        {
            // Move up
            transform.Translate(Vector2.up * 20f * Time.deltaTime);

            // If reached up, done, come on to next step
            if (yPos > 2.5f)
            {
                bottomSnare_isMovingUp = false;
                bottomSnare_upDone = true;
            }
        }
        else if (bottomSnare_upDone)
        {
            // wait
            bottomSnare_waitTime -= Time.deltaTime;

            // If time is done
            if (bottomSnare_waitTime < 0) { 
                bottomSnare_waitTime = Random.Range(2f, 6f);    // Reset the time
                bottomSnare_upDone = false;                     // Move on to next step
            }
        }
        else
        {
            // Go down
            transform.Translate(Vector2.down * 2f * Time.deltaTime);

            // If reached down, stop the action
            if (yPos < 0f)
            {
                isPerforming = false;   // Finish performing
            }
        }
    }

    private void TurningBladeActions()
    {
        if (actionTest)    // Change this later with Level Manager
        {
            actionTest = false;
            didHit = false;         // we can hit again
            isTurningBladeMoving = true;
            isPerforming = true;
        }

        if (!isPerforming) return;      // Don't execute any code below if we are not assigned to perform

        // Keep sprite turning
        turningBladesprite.transform.Rotate(Vector3.back * 2000f * Time.deltaTime);

        float xPos = transform.localPosition.x;
        float yPos = transform.localPosition.y;

        if (isTurningBladeMoving)
        {
            // Going right
            if (turningBlade_Range > 0f && xPos < turningBlade_Range)
            {
                // Rise if needed
                if (yPos < 3f && !upComplete)
                {
                    transform.Translate(Vector2.up * turningBlade_MoveSpeed * Time.deltaTime);
                }
                // If we reached the up
                else upComplete = true;

                // Move
                transform.Translate(Vector2.right * turningBlade_MoveSpeed * Time.deltaTime);

                // Start to go below when we come closer to the destination
                if ((turningBlade_Range - xPos) < 3f && yPos > 0f)
                {
                    transform.Translate(Vector2.down * turningBlade_MoveSpeed * Time.deltaTime);
                }
            }

            // Going Left
            else if (turningBlade_Range < 0f && xPos > turningBlade_Range)
            {
                if (yPos < 3f && !upComplete)
                {
                    transform.Translate(Vector2.up * turningBlade_MoveSpeed * Time.deltaTime);
                }
                // If we reached the up
                else upComplete = true;

                // Move
                transform.Translate(Vector2.left * turningBlade_MoveSpeed * Time.deltaTime);

                // Start to go below when we come closer to the destination
                if ((turningBlade_Range - xPos) < 3f && yPos > 0f)
                {
                    transform.Translate(Vector2.down * turningBlade_MoveSpeed * Time.deltaTime);
                }
            }

            // If none of them is true, then we have reached our destination
            else
            {
                isTurningBladeMoving = false;
            }
        }


        // If we are not moving, reached to the destination then go below the surface
        else
        {
            if (yPos > 0f)
            {
                transform.Translate(Vector2.down * turningBlade_MoveSpeed * Time.deltaTime);
            }

            upComplete = false;     // Reset the upComplete to go up next time
            transform.localPosition = Vector2.zero;
            isPerforming = false;
        }
    }

    private void ChainBladeActions()
    {
        if (actionTest)
        {
            actionTest = false;
            didHit = false;         // we can hit again
            chainBlade_movingUp = true;
            isPerforming = true;
        }

        if (!isPerforming) return; // Don't execute any code below if we are not assigned to perform

        // Keep sprite turning
        chainBladeSprite.transform.Rotate(Vector3.back * 2000f * Time.deltaTime);

        float yPos = transform.localPosition.y;

        if (chainBlade_movingUp)
        {
            // Move up
            transform.Translate(Vector2.up * 10f * Time.deltaTime);

            // If reached up, done, come on to next step
            if (yPos > 3.8f)
            {
                chainBlade_movingUp = false;
                chainBlade_upDone = true;
            }
        }
        else if (chainBlade_upDone)
        {
            // wait
            chainBlade_waitTime -= Time.deltaTime;

            // If time is done
            if (chainBlade_waitTime < 0)
            {
                chainBlade_waitTime = Random.Range(0.5f, 5f);  // Reset the time
                chainBlade_upDone = false;                     // Move on to next step
                didHit = false;                                 // we can hit again
            }
        }
        else
        {
            // Go down
            transform.Translate(Vector2.down * 10f * Time.deltaTime);

            // If reached down, stop the action
            if (yPos < -5.3f)
            {
                isPerforming = false;   // Finish performing
            }
        }
    }

    private void BladeSnareActions()
    {
        if (actionTest)
        {
            actionTest = false;
            didHit = false;         // we can hit again
            blade_movingDown = true;
            isPerforming = true;
        }
        
        if (!isPerforming) return; // Don't execute any code below if we are not assigned to perform

        float yPos = transform.localPosition.y;

        if (blade_movingDown)
        {
            // Go Down
            transform.Translate(Vector2.left * 10f * Time.deltaTime);

            // If reached down, done, come on to next step
            if (yPos < -4f)
            {
                blade_movingDown = false;
                balde_downDone = true;
            }
        }
        else if (balde_downDone)
        {
            // wait - same as bottom snares
            bottomSnare_waitTime -= Time.deltaTime;

            // If time is done
            if (bottomSnare_waitTime < 0)
            {
                bottomSnare_waitTime = Random.Range(5f, 6f);    // Reset the time
                balde_downDone = false;                     // Move on to next step
            }
        }
        else
        {
            // Go up
            transform.Translate(Vector2.right * 2f * Time.deltaTime);

            // If reached up, stop the action
            if (yPos > 0f)
            {
                isPerforming = false;   // Finish performing
            }
        }
        
    }

    private IEnumerator DestroyIn(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);

        Destroy(gameObject);
    }


}
