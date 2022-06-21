using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoaderFinder : MonoBehaviour
{
    public void LoadLevel(string levelName)
    {
        FindObjectOfType<LevelLoader>().LoadLevel(levelName);
    }
}
