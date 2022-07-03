using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnareSpot : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool isSnareAssigned = false;
    [SerializeField] private float snareActivationDistance = 100f;

    [Header("Turning Blade")]
    [SerializeField] private float turningBladeRange = 5f;
    [SerializeField] private bool isTurningBlade = false;

    [Header("Stone")]
    [SerializeField] private bool isStone = false;
    public float stoneWaitTime = 1f;

    private StickPlayer player;             // Get the player
    private float playerDistance = 100f;    // Large number is given as default to avoid possible bugs

    private Snare snare;

    /* Spot Location
     * 
     * Stone: level of the body
     * Blade: Bottom of the roof
     * Others: Surface of the floor
     * 
     */

    void Start()
    {
        // if no snare assigned, then don't execute any code
        if (!isSnareAssigned) return;

        player = FindObjectOfType<StickPlayer>();
        snare = GetComponentInChildren<Snare>();

        // turn snares off to increase performance
        if (snare) snare.gameObject.SetActive(false);

        // if it is the turning blade, then set the range
        if (snare && isTurningBlade) snare.turningBlade_Range = turningBladeRange;
    }


    void Update()
    {
        // if no snare assigned, then don't execute any code
        if (!isSnareAssigned) return;

        // If player is not present, try to find, if can't find, then return.
        if (player == null)
        {
            player = FindObjectOfType<StickPlayer>();

            return;
        }
        if (snare == null)
        {
            snare = GetComponentInChildren<Snare>();

            // turn snares off to increase performance
            if (snare) snare.gameObject.SetActive(false);

            // if it is the turning blade, then set the range
            if (snare && isTurningBlade) snare.turningBlade_Range = turningBladeRange;

            return;
        }

        // If player is present, check the distance
        playerDistance = Vector2.Distance(player.transform.position, transform.position);

        // If close enough, then activate the snare
        if (snare != null && playerDistance < snareActivationDistance)
        {
            stoneWaitTime -= Time.deltaTime;
            if (isStone)
            {
                if (stoneWaitTime < 0)  // If it is a stone, then wait for its time, and activate later.
                {
                    snare.gameObject.SetActive(true);   // Activate Snare
                    isSnareAssigned = false;            // And our job is done
                }
            }
            else
            {
                snare.gameObject.SetActive(true);   // Activate Snare
                isSnareAssigned = false;            // And our job is done
            }
        }
    }

    public void SetActivationDistance(float _newActivationDistance) { snareActivationDistance = _newActivationDistance; }
    public void AssignSnare(bool _snareAssigned) { isSnareAssigned = _snareAssigned; }
}
