using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour {

    public static GameState instance;

    public event System.Action<GameState> OnGameStateChange;

    // Use this for initialization
    void Awake () {
        if(instance == null)
        {
            instance = this;
        }
	}

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update () {
		
	}
}
