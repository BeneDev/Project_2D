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

    public float Vertical
    {
        get
        {
            return Input.GetAxis("Vertical");
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

    public bool Attack
    {
        get
        {
            if(Input.GetButtonDown("Attack"))
            {
                return true;
            }
            return false;
        }
    }
}
