using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    public Vector3 currentCheckpoint;

    // TODO Level loading and all kinds of this stuff would be implemented here

    // Make the GameManger Instance a Singleton 
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Set the first checkpoint to the starting point of the player
        currentCheckpoint = Vector3.zero;
    }
}
