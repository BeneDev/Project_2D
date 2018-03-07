using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinyEnemy : GeneralEnemy {

	// Use this for initialization
	void Start () {
        GeneralInitialization();
	}
	
	// Update is called once per frame
	void Update () {
        GeneralBehavior();
	}
}
