using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The specific Class for the Tiny Enemy Type
/// </summary>
public class TinyEnemy : GeneralEnemy {
    
	void Start () {
        // Call the General Initialization, inherited from the GeneralEnemy Script
        GeneralInitialization();
	}
	
	void Update () {
        // Call the General Behavior, inherited from the GeneralEnemy Script
        GeneralBehavior();
	}
}
