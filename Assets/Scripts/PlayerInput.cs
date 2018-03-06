using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour, IInput {

    [Range(0, 1)] [SerializeField] float controllerThreshhold;

    public float Horizontal
    {
        get
        {
            if (Input.GetAxis("Horizontal") >= controllerThreshhold || Input.GetAxis("Horizontal") <= -controllerThreshhold)
            {
                return Input.GetAxis("Horizontal");
            }
            return 0f;
        }
    }

    public float Vertical
    {
        get
        {
            if (Input.GetAxis("Vertical") >= controllerThreshhold || Input.GetAxis("Vertical") <= -controllerThreshhold)
            {
                return Input.GetAxis("Vertical");
            }
            return 0f;
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
