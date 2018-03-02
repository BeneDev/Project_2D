﻿using System.Collections;
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

    public int Jump
    {
        get
        {
            if (Input.GetButtonDown("Jump"))
            {
                return 2;
            }
            else if(Input.GetButton("Jump"))
            {
                return 1;
            }
            return 0;
        }
    }

    public bool Dodge
    {
        get
        {
            if(Input.GetButtonDown("Dodge"))
            {
                return true;
            }
            return false;
        }
    }
}
