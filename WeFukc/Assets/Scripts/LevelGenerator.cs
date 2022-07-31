using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Elements")]
    [SerializeField] [Range(1, 20)] private int level = 1;
    [SerializeField] private StickBot bot;
    [SerializeField] private Snare bottomSnare;
    [SerializeField] private Snare turningBladeSnare;
    [SerializeField] private GameObject chainBladeSnare;
    [SerializeField] private Snare stoneSnare;
    [SerializeField] private GameObject ElevatorKey;

    [Header("Snare Tunening")]
    [SerializeField] private float maxWaitTime = 15f;

    // Each path element has spots as a game object. Bot spots have transform as spot.
    // Snare Spots have Snare object as spot except the chain snare. It has GameObject, then under it, Snare
    [SerializeField] private Path[] paths;

    // Other vars
    private bool isKeyAssigned = false;
    private bool passDefaultPathKey = false;

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
     * - Left at least 6.3 unit in Y axis between platform
     * 
     * Difficulty Difference when spawn a snare - These percentages differs according to number of possible spots
     * -- For instance, bottom snare have lots of spots to spawn. Therefore, they have less percentage to spawn
     * 
     * Bots: 10-20-30%              Place them in every 10 units        3 units above surface
     * Bottom: 5-10-15%           10 units                            2 units below surface
     * Turning Blade: 10-20-30%                                         3.5 units below surface
     * Chain Balde: 5-10-15%        10 units                            3.3 units above surface
     * Blade: 5-10-15%                                                  3 units above from the roof
     * Stone: 5-10-15%              10 units                            on the road
     * 
     */


    /* Levels and Bot Tiers
     *                                                   50% - 50%
   2 *  1: 75% Tier 1 - 25% Tier 2                      Tier 1 - 2
   2 *  2: 50% Tier 1 - 50% Tier 2                      Tier 2 - 3
   3 *  3: 25% Tier 1 - 50% Tier 2 - 25% Tier 3         Tier 3 - 4
   3 *  4: 50% Tier 2 - 50% Tier 3                      Tier 4 - 5
   4 *  5: 25% Tier 2 - 50% Tier 3 - 25% Tier 4         Tier 5 - 6
   4 *  6: 50% Tier 3 - 50% Tier 4                      Tier 6 - 7
   5 *  7: 25% Tier 3 - 50% Tier 4 - 25% Tier 5         Tier 7 - 8
   5 *  8: 50% Tier 4 - 50% Tier 5                      Tier 8 - 9
   6 *  9: 25% Tier 4 - 50% Tier 5 - 25% Tier 6         Tier 9 - 10
   6 *  10: 50% Tier 5 - 50% Tier 6                     Tier 10 - 11
   7 *  11: 25% Tier 5 - 50% Tier 6 - 25% Tier 7        Tier 11 - 12
   8 *  12: 25% Tier 6 - 50% Tier 7 - 25% Tier 8        Tier 12 - 13
   9 *  13: 25% Tier 7 - 50% Tier 8 - 25% Tier 9        Tier 13 - 14
  10 *  14: 25% Tier 8 - 50% Tier 9 - 25% Tier 10       Tier 14 - 15
  11 *  15: 25% Tier 9 - 50% Tier 10 - 25% Tier 11      Tier 15
  12 *  16: 25% Tier 10 - 50% Tier 11 - 25% Tier 12     
  13 *  17: 25% Tier 11 - 50% Tier 12 - 25% Tier 13     
  14 *  18: 25% Tier 12 - 50% Tier 13 - 25% Tier 14     
  15 *  19: 25% Tier 13 - 50% Tier 14 - 25% Tier 15
  15 *  20: 50% Tier 14 - 50% Tier 15
     */




    void Start()
    {
        // Assign Paths
        AssignDifficulties();

        // DEBUG: Set the path difficulties automatically
        //paths[2].pathDifficulty = 1; paths[3].pathDifficulty = 2; paths[4].pathDifficulty = 3;

        // Check if there are paths other than the default path to assign the key
        CheckOtherPaths();
        
        // 1. path is the default, 2-3-4. paths are normal paths | First index is empty due to Unity's bug
        for (int i = 1; i < paths.Length; i++)
        {
            // Spawn Bots
            if (paths[i].BotSpots == null) continue;    // if the path is empty, then skip
            BotGeneration(paths[i]);                    // Send path to generate bots, it will consider
                                                        // the difficulty of the path as well
            // Spawn Snares
            if (paths[i].BotSpots == null) continue;    
            SnareGeneration(paths[i]);
        }
    }

    private void AssignDifficulties()
    {
        // index 0 is empty, 1 is the defaulty path, 2-3-4 is the 3 paths
        paths[1].pathDifficulty = 1;

        // Get the first one
        paths[2].pathDifficulty = Random.Range(1, 4); // Take difficulty from 1 to 3

        // Get the second one
        do
        {
            paths[3].pathDifficulty = Random.Range(1, 4);
        }
        while (paths[3].pathDifficulty == paths[2].pathDifficulty);  // if path 2 is equal path 1, than take again

        // Get the last one
        do
        {
            paths[4].pathDifficulty = Random.Range(1, 4);
        }
        while (paths[4].pathDifficulty == paths[2].pathDifficulty || paths[4].pathDifficulty == paths[3].pathDifficulty);
    }

    private void CheckOtherPaths()
    {
        // Start from the first (2. index) path to check all 3 paths
        for (int i = 2; i < 5; i++) 
        {
            // If any of them has bots spots, then don't pass the default path key
            if (paths[i].BotSpots != null) passDefaultPathKey = true;
        }
    }

    private void SnareGeneration(Path _path)
    {
        // Get the spots
        GameObject bottomSpots = null;
        GameObject turningBladeSpots = null;
        GameObject chainSpots = null;
        GameObject stoneSpots = null;

        if (_path.BottomSpots != null) { 

            Transform[] spots = _path.BottomSpots.GetComponentsInChildren<Transform>();

            foreach (Transform spot in spots)
            {
                // 10-20-30% of the spots will generate bottom snares according to difficulty level (1-2-3)
                if (_path.pathDifficulty * 0.5f < Random.Range(0, 10)) continue; // if random number bigger then difficulty level, then skip

                if (spot.name.EndsWith("Spots")) continue; // if it get the spots' parent object, then skip it.

                Snare newSnare = Instantiate(bottomSnare, spot);

                spot.GetComponent<SnareSpot>().AssignSnare(true);           //Let the spot know we gave a snare to it
                newSnare.maxWaitTime = maxWaitTime * (4 - _path.pathDifficulty);  // Set the wait time 15-30-45 with 3-2-1 difficulty
            }
        }
        if (_path.TurningBladeSpots != null) { 

            Transform[] spots = _path.TurningBladeSpots.GetComponentsInChildren<Transform>();

            foreach (Transform spot in spots)
            {
                // 10-20-30% of the spots will generate bottom snares according to difficulty level (1-2-3)
                if (_path.pathDifficulty < Random.Range(0, 10)) continue; // if random number bigger then difficulty level, then skip

                if (spot.name.EndsWith("Spots")) continue; // if it get the spots' parent object, then skip it.

                Snare newSnare = Instantiate(turningBladeSnare, spot);

                spot.GetComponent<SnareSpot>().AssignSnare(true);           //Let the spot know we gave a snare to it

                if (_path.pathDifficulty == 1) newSnare.turningBlade_MoveSpeed = 10;
                else if (_path.pathDifficulty == 2) newSnare.turningBlade_MoveSpeed = 15;
                else newSnare.turningBlade_MoveSpeed = 20;

                newSnare.turningBlade_MaxSpawnTimeIndex = 4 - _path.pathDifficulty; // spawn time decreases while difficulty increase
                newSnare.maxWaitTime = maxWaitTime * (4 - _path.pathDifficulty);
            }
        }
        if (_path.ChainBladeSpots != null) { 

            Transform[] spots = _path.ChainBladeSpots.GetComponentsInChildren<Transform>();

            foreach (Transform spot in spots)
            {
                // 10-20-30% of the spots will generate bottom snares according to difficulty level (1-2-3)
                if (_path.pathDifficulty * 0.5f < Random.Range(0, 10)) continue; // if random number bigger then difficulty level, then skip

                if (spot.name.EndsWith("Spots")) continue; // if it get the spots' parent object, then skip it.

                GameObject newSnare = Instantiate(chainBladeSnare, spot);

                spot.GetComponent<SnareSpot>().AssignSnare(true);           //Let the spot know we gave a snare to it
                newSnare.GetComponentInChildren<Snare>().maxWaitTime = maxWaitTime * (4 - _path.pathDifficulty);
            }
        }
        if (_path.StoneSpots != null) { 

            Transform[] spots = _path.StoneSpots.GetComponentsInChildren<Transform>();

            foreach (Transform spot in spots)
            {
                // 10-20-30% of the spots will generate bottom snares according to difficulty level (1-2-3)
                if (_path.pathDifficulty * 0.5f < Random.Range(0, 10)) continue; // if random number bigger then difficulty level, then skip

                if (spot.name.EndsWith("Spots")) continue; // if it get the spots' parent object, then skip it.

                Snare newSnare = Instantiate(stoneSnare, spot);

                spot.GetComponent<SnareSpot>().AssignSnare(true);           //Let the spot know we gave a snare to it
                spot.GetComponent<SnareSpot>().SetActivationDistance(20f);
                newSnare.maxWaitTime = Random.Range(1f, 3f); // Random wait time
            }
        }
    }

    private void BotGeneration(Path _path)
    {
        GameObject _botSpots = _path.BotSpots;
        Transform[] spots = _botSpots.GetComponentsInChildren<Transform>();

        StickBot newBot = null;

        // If the path doesn't need a key to assign, then skip it by marking it already assigned
        if (!_path.assignKey) isKeyAssigned = true;

        int spotIndex = 1;
        foreach (Transform spot in spots)
        {
            // 10-20-30% of the spots will generate bots according to difficulty level (1-2-3)
            // if random number bigger then difficulty level, then skip
            // if we've reached the last spot in the spots, then spawn a bot definitely!
            if (_path.pathDifficulty < Random.Range(0, 10) && spots.Length != spotIndex)
            {
                // Increase the index 
                spotIndex++;

                continue;
            }

            // Increase the index 
            spotIndex++;

            if (spot.name.EndsWith("Spots")) continue; // if it get the spots' parent object, then skip it.

            // Spawn the bot
            newBot = Instantiate(bot, spot.transform.position, Quaternion.identity, spot);
            
            // Get a random number to set tier accordingly
            float randomNum = Random.Range(0f, 1f);

            // Set its tier according to level
            switch (level)
            {
                case 1:
                    // 75% Tier 1, 25% Tier 2
                    if (0.75f > randomNum) newBot.SetTier(1);
                    else newBot.SetTier(2);
                    break;

                case 2:
                    // 50% Tier 1, 50% Tier 2
                    if (0.50f > randomNum) newBot.SetTier(1);
                    else newBot.SetTier(2);
                    break;

                case 3:
                    // 25% Tier 1, 50% Tier 2, 25% Tier 3
                    if (0.25f > randomNum) newBot.SetTier(1);
                    else if (0.50f > randomNum) newBot.SetTier(2);
                    else newBot.SetTier(3);
                    break;

                default:
                    Debug.LogError("Bot generation couldn't detect the accurate level. Please check the Level Generation");
                    break;
            }

            // Give key randomly

            // Skip the default path if we should
            if (passDefaultPathKey && _path.pathName == "Default")
            {
                isKeyAssigned = true;
                continue;
            }

            if (!isKeyAssigned)
            {
                // Assign the key by 10%
                if (2f > Random.Range(0f, 10f)){
                    newBot.isKeyAssigned = true;
                    isKeyAssigned = true;
                }
            }
        }
        // If the key has not been assigned any of the bots, then assign it to the last one.
        if (!isKeyAssigned) newBot.isKeyAssigned = true;

        // Reset key assignment for the next paths
        isKeyAssigned = false;
    }



}
