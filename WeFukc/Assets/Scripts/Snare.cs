using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snare : MonoBehaviour
{
    // Test vars



    // General
    public float maxWaitTime = 15f;
    private float minWaitTime = 5f;
    private float currentWaitTime = 5f;
    private bool isPerforming = false;
    [SerializeField] private bool snareActivated = false;

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
    public float turningBlade_Range = 5f;
    public float turningBlade_MoveSpeed = 5f;
    public float turningBlade_MaxSpawnTimeIndex = 3f;
    private float turningBlade_MinSpawnTime = 10f;
    private float turningBlade_SpawnTime = 10f;
    private bool isTurningBladeMoving = false;
    private bool upComplete = false;
    private SpriteRenderer turningBladesprite;

    // Bottom Snare
    private bool bottomSnare_isMovingUp = false;
    private bool bottomSnare_upDone = false;

    // Stone Snare
    private bool didHit = false;

    // Blade snare
    private bool blade_movingDown = false;
    private bool balde_downDone = false;

    // Chain Blade
    private bool chainBlade_movingUp = false;
    private bool chainBlade_upDone = false;
    private SpriteRenderer chainBladeSprite;


    /* Snare Information
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
        if (turningBladeSnare)
        {
            turningBladesprite = GetComponentInChildren<SpriteRenderer>();  // Get the sprite to rotate

            // Get new spawn time
            turningBlade_MinSpawnTime = (turningBlade_Range / turningBlade_MoveSpeed) + 5f;
            turningBlade_SpawnTime = Random.Range(turningBlade_MinSpawnTime, turningBlade_MinSpawnTime * turningBlade_MaxSpawnTimeIndex);

        }
        else if (chainBladeSnare)
        {
            chainBladeSprite = GetComponentInChildren<SpriteRenderer>();  // Get the sprite to rotate
        }


        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
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

            didHit = true; // Avoid giving damage after hitting ground

            // Destroy stone 8 seconds afer collision
            StartCoroutine(DestroyIn(8f));
        }
    }


    private void BottomSnareActions()
    {
        if (snareActivated || currentWaitTime < 0f)
        {
            snareActivated = false;
            didHit = false;         // we can hit again
            bottomSnare_isMovingUp = true;
            isPerforming = true;

            // Get new wait time
            currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
        }
        else currentWaitTime -= Time.deltaTime;

        if (!isPerforming) return; // Don't execute any code below if we are not assigned to perform

        float yPos = transform.localPosition.y;

        if (bottomSnare_isMovingUp)
        {
            // Move up
            transform.Translate(Vector2.up * 20f * Time.deltaTime);

            // If reached up, done, come on to next step
            if (yPos > 0.3f)
            {
                bottomSnare_isMovingUp = false;
                bottomSnare_upDone = true;
            }
        }
        else if (bottomSnare_upDone)
        {
            // wait
            currentWaitTime -= 20f * Time.deltaTime;

            // If time is done
            if (currentWaitTime < 0) { 
                currentWaitTime = Random.Range(2f, 6f);    // Reset the time
                bottomSnare_upDone = false;                     // Move on to next step
            }
        }
        else
        {
            // Go down
            transform.Translate(Vector2.down * 2f * Time.deltaTime);

            // If reached down, stop the action
            if (yPos < -0.9f)
            {
                isPerforming = false;   // Finish performing
            }
        }
    }

    private void TurningBladeActions()
    {
        if (snareActivated || turningBlade_SpawnTime < 0f)    // Change this later with Level Manager
        {
            snareActivated = false;
            didHit = false;         // we can hit again
            isTurningBladeMoving = true;
            isPerforming = true;

            // Get new spawn time
            turningBlade_MinSpawnTime = (turningBlade_Range / turningBlade_MoveSpeed) + 5f;
            turningBlade_SpawnTime = Random.Range(turningBlade_MinSpawnTime, turningBlade_MinSpawnTime * turningBlade_MaxSpawnTimeIndex);
        }
        else turningBlade_SpawnTime -= Time.deltaTime;

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
                if (yPos < -1.7f && !upComplete)
                {
                    transform.Translate(Vector2.up * turningBlade_MoveSpeed * Time.deltaTime);
                }
                // If we reached the up
                else upComplete = true;

                // Move
                transform.Translate(Vector2.right * turningBlade_MoveSpeed * Time.deltaTime);

                // Start to go below when we come closer to the destination
                if ((turningBlade_Range - xPos) < 3f && yPos > -1.7f)
                {
                    transform.Translate(Vector2.down * turningBlade_MoveSpeed * Time.deltaTime);
                }
            }

            // Going Left
            else if (turningBlade_Range < 0f && xPos > turningBlade_Range)
            {
                // Rise if needed
                if (yPos < -1.7f && !upComplete)
                {
                    transform.Translate(Vector2.up * turningBlade_MoveSpeed * Time.deltaTime);
                }
                // If we reached the up
                else upComplete = true;

                // Move
                transform.Translate(Vector2.left * turningBlade_MoveSpeed * Time.deltaTime);

                // Start to go below when we come closer to the destination
                if ((turningBlade_Range - xPos) < 3f && yPos > -1.7f)
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
            if (yPos > -1.7f)
            {
                transform.Translate(Vector2.down * turningBlade_MoveSpeed * Time.deltaTime);
            }

            upComplete = false;     // Reset the upComplete to go up next time
            transform.localPosition = new Vector2(0, -1.7f);
            isPerforming = false;
        }
    }

    private void ChainBladeActions()
    {
        if (snareActivated || currentWaitTime < 0f)
        {
            snareActivated = false;
            didHit = false;         // we can hit again
            chainBlade_movingUp = true;
            isPerforming = true;

            currentWaitTime = Random.Range(minWaitTime, maxWaitTime);   // Set a new action time
        }
        else currentWaitTime -= Time.deltaTime;

        if (!isPerforming) return; // Don't execute any code below if we are not assigned to perform

        // Keep sprite turning
        chainBladeSprite.transform.Rotate(Vector3.back * 2000f * Time.deltaTime);

        float yPos = transform.localPosition.y;

        if (chainBlade_movingUp)
        {
            // Move up
            transform.Translate(Vector2.up * 10f * Time.deltaTime);

            // If reached up, done, come on to next step
            if (yPos > 4f)
            {
                chainBlade_movingUp = false;
                chainBlade_upDone = true;
            }
        }
        else if (chainBlade_upDone)
        {
            // wait
            currentWaitTime -= 3 * Time.deltaTime;

            // If time is done
            if (currentWaitTime < 0)
            {
                currentWaitTime = Random.Range(0.5f, 5f);  // Reset the time
                chainBlade_upDone = false;                     // Move on to next step
                didHit = false;                                 // we can hit again
            }
        }
        else
        {
            // Go down
            transform.Translate(Vector2.down * 10f * Time.deltaTime);

            // If reached down, stop the action
            if (yPos < -4f)
            {
                isPerforming = false;   // Finish performing
            }
        }
    }

    private void BladeSnareActions()
    {
        if (snareActivated || currentWaitTime < 0f)
        {
            snareActivated = false;
            didHit = false;         // we can hit again
            blade_movingDown = true;
            isPerforming = true;

            // Get new wait time
            currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
        }
        else currentWaitTime -= Time.deltaTime;

        if (!isPerforming) return; // Don't execute any code below if we are not assigned to perform

        float yPos = transform.localPosition.y;

        if (blade_movingDown)
        {
            // Go Down
            transform.Translate(Vector2.left * 10f * Time.deltaTime);

            // If reached down, done, come on to next step
            if (yPos < -1.7f)
            {
                blade_movingDown = false;
                balde_downDone = true;
            }
        }
        else if (balde_downDone)
        {
            // wait - same as bottom snares
            currentWaitTime -= 20f * Time.deltaTime;

            // If time is done
            if (currentWaitTime < 0)
            {
                currentWaitTime = Random.Range(5f, 6f);    // Reset the time
                balde_downDone = false;                     // Move on to next step
            }
        }
        else
        {
            // Go up
            transform.Translate(Vector2.right * 2f * Time.deltaTime);

            // If reached up, stop the action
            if (yPos > 2.1f)
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
