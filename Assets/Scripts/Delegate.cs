using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delegate : MonoBehaviour {

    public enum State
    {
        one,
        two,
        three
    }

    private void OnEnable()
    {
        GameState.instance.OnGameStateChange += ThisMethod;
    }

    private void OnDisable()
    {
        GameState.instance.OnGameStateChange -= ThisMethod;
    }

    public void ThisMethod(GameState param)
    {
        // stuff this method does
    }
}
