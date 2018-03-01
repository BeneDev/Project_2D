using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour, IInput {

    public float Horizontal
    {
        get
        {
            return Input.GetAxis("Horizontal");
        }
    }

    public float Jump
    {
        get
        {
            return 1f;
        }
    }
}
