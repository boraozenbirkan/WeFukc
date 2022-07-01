using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kill if the collider is player or the bot

        if (collision.gameObject.GetComponent<StickPlayer>() != null)
            collision.gameObject.GetComponent<StickPlayer>().CertainDeath();
        else if (collision.gameObject.GetComponent<StickBot>() != null)
            collision.gameObject.GetComponent<StickBot>().CertainDeath();        
    }
}
