using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorKey : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player collides (layer 3 is player)
        if (collision.gameObject.layer == 3)
        {
            // Trigger pick up animation
            GetComponent<Animator>().SetTrigger("keyPickUp");
        }
    }

    public void keyActionComplete()
    {
        // Trigger the key pick up action in player object
        FindObjectOfType<StickPlayer>().PickUpKey();
        Destroy(gameObject);
    }
}
