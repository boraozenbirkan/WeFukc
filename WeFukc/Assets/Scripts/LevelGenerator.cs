using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Elements")]
    [SerializeField] [Range(1, 20)] private int level = 1;
    [SerializeField] private StickBot bot;
    [SerializeField] private Snare bottomWoodSnare;
    [SerializeField] private Snare bottomMetalSnare;
    [SerializeField] private Snare turningBladeSnare;
    [SerializeField] private GameObject chainBladeSnare;
    [SerializeField] private Snare bladeSnare;
    [SerializeField] private Snare stoneSnare;

    #region Paths
    [Header("Path 1")]
    [SerializeField] private SnareSpot[] pathOneTurningBladeSpots;
    [SerializeField] private SnareSpot[] pathOneChainBladeSpots;
    [SerializeField] private SnareSpot[] pathOneBottomSpots;
    [SerializeField] private SnareSpot[] pathOneStoneSpots;
    [SerializeField] private SnareSpot[] pathOneBladeSpots;
    [SerializeField] private Transform[] pathOneBotSpots;

    [Header("Path 2")]
    [SerializeField] private SnareSpot[] pathTwoTurningBladeSpots;
    [SerializeField] private SnareSpot[] pathTwoChainBladeSpots;
    [SerializeField] private SnareSpot[] pathTwoBottomSpots;
    [SerializeField] private SnareSpot[] pathTwoStoneSpots;
    [SerializeField] private SnareSpot[] pathTwoBladeSpots;
    [SerializeField] private Transform[] pathTwoBotSpots;

    [Header("Path 3")]
    [SerializeField] private SnareSpot[] pathThreeTurningBladeSpots;
    [SerializeField] private SnareSpot[] pathThreeChainBladeSpots;
    [SerializeField] private SnareSpot[] pathThreeBottomSpots;
    [SerializeField] private SnareSpot[] pathThreeStoneSpots;
    [SerializeField] private SnareSpot[] pathThreeBladeSpots;
    [SerializeField] private Transform[] pathThreeBotSpots;

    [Header("Default Path")]
    [SerializeField] private SnareSpot[] pathDefaultTurningBladeSpots;
    [SerializeField] private SnareSpot[] pathDefaultChainBladeSpots;
    [SerializeField] private SnareSpot[] pathDefaultBottomSpots;
    [SerializeField] private SnareSpot[] pathDefaultStoneSpots;
    [SerializeField] private SnareSpot[] pathDefaultBladeSpots;
    [SerializeField] private Transform[] pathDefaultBotSpots;
    #endregion

    /* Snare TODOs
     * 
     * All of them
     * - set true isSnareAssigned
     * - Give max wait time 15 - 45 depending on the difficulty
     * 
     * Stone
     * - Set the spot's activation distance as 20
     * - Give a wait time between 1-3 seconds in all difficulties (give it to the spot)
     * - Difficulties only affects the number of stones, not spawn time
     * 
     * Turning Blade
     * - Set speed between 10 - 20
     * - Set max spawn time index as 3 - 9 | It will calculate the min and max spawn time
     * 
     * Chain Snare
     * - The snare is inside of the gameObject. Reach it by GetComponentInChildren
     * 
     */


    /* BOT TODOs
     * 
     * Set Bot's tier and they will set the remeaning features themselves
     * The name of the bot spot is the intensity level. 
     * 
     * Bot Intensities Levels (number of bots)
     * 1: 1 
     * 2: 1 - 3   
     * 3: 3 - 6
     * 4: 6 - 9
     * 5: 9 - 12
     * 
     * 
     */


    /* Levels and Bot Tiers
     * 
   2 *  1: 75% Tier 1 - 25% Tier 2
   2 *  2: 50% Tier 1 - 50% Tier 2
   3 *  3: 25% Tier 1 - 50% Tier 2 - 25% Tier 3
   3 *  4: 50% Tier 2 - 50% Tier 3
   4 *  5: 25% Tier 2 - 50% Tier 3 - 25% Tier 4
   4 *  6: 50% Tier 3 - 50% Tier 4
   5 *  7: 25% Tier 3 - 50% Tier 4 - 25% Tier 5
   5 *  8: 50% Tier 4 - 50% Tier 5
   6 *  9: 25% Tier 4 - 50% Tier 5 - 25% Tier 6
   6 *  10: 50% Tier 5 - 50% Tier 6
   7 *  11: 25% Tier 5 - 50% Tier 6 - 25% Tier 7
   8 *  12: 25% Tier 6 - 50% Tier 7 - 25% Tier 8
   9 *  13: 25% Tier 7 - 50% Tier 8 - 25% Tier 9
  10 *  14: 25% Tier 8 - 50% Tier 9 - 25% Tier 10
  11 *  15: 25% Tier 9 - 50% Tier 10 - 25% Tier 11
  12 *  16: 25% Tier 10 - 50% Tier 11 - 25% Tier 12
  13 *  17: 25% Tier 11 - 50% Tier 12 - 25% Tier 13
  14 *  18: 25% Tier 12 - 50% Tier 13 - 25% Tier 14
  15 *  19: 25% Tier 13 - 50% Tier 14 - 25% Tier 15
  15 *  20: 50% Tier 14 - 50% Tier 15
     */


    /* Algorithm
     * 
     *  /- Assign Path Difficulties
     *  /- Spawn Bots
     *  /- Spawn Snares
     * 
     */

    private int pathOneDiff = 0;
    private int pathTwoDiff = 0;
    private int pathThreeDiff = 0;

    void Start()
    {
        // ** Assign Paths ** //
        #region
        // Get the first one
        pathOneDiff = Random.Range(1, 4); // Take difficulty from 1 to 3

        // Get the second one
        do
        {
            pathTwoDiff = Random.Range(1, 4);
        }
        while (pathTwoDiff == pathOneDiff);  // if path 2 is equal path 1, than take again

        // Get the last one
        do
        {
            pathThreeDiff = Random.Range(1, 4);
        }
        while (pathThreeDiff == pathOneDiff || pathThreeDiff == pathTwoDiff);
        #endregion

        // ** Spawn Bots ** //
        #region
        // Spawn for Path one and default


        // Spawn for path two


        // Spawn for path three


        #endregion


    }

    void Update()
    {
        
    }

    // 
}
