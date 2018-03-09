﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Singleton Script, handling the general game related tasks
/// </summary>
public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    public Vector3 currentCheckpoint; // Stores the Checkpoint which is currently activated, to send the player there if he dies

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
