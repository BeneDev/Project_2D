﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The specific Class for the Tiny Enemy Type
/// </summary>
public class TinyEnemy : GeneralEnemy {

    #region Properties

    bool BLookLeft
    {
        get
        {
            return bLookLeft;
        }
        set
        {
            if(value == false)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }

    #endregion

    #region Fields

    struct Raycasts
    {
        public RaycastHit2D bottomLeft;
        public RaycastHit2D bottomMid;
        public RaycastHit2D bottomRight;
    }
    private Raycasts rays;

    private bool bLookLeft = true;

    private Vector3 moveDirection;

    [SerializeField] float moveSpeed = 0.1f;

    #endregion

    void Start () {
        // Call the General Initialization, inherited from the GeneralEnemy Script
        GeneralInitialization();
	}
	
	void Update () {
        // Update all of the rays, used to check for ground under the enemy
        rays.bottomLeft = Physics2D.Raycast(transform.position + new Vector3(-0.04f, -0.02f), Vector2.down, 0.04f, layersToCollideWith);
        rays.bottomMid = Physics2D.Raycast(transform.position + new Vector3(0f, -0.02f), Vector2.down, 0.04f, layersToCollideWith);
        rays.bottomRight = Physics2D.Raycast(transform.position + new Vector3(0.04f, -0.02f), Vector2.down, 0.04f, layersToCollideWith);

        // Call the General Behavior, inherited from the GeneralEnemy Script
        GeneralBehavior();
        Move();
	}

    private void Move()
    {
        if(BLookLeft == true)
        {
            if(rays.bottomMid && rays.bottomRight)
            {
                moveDirection = -transform.right * moveSpeed;
            }
            else
            {
                moveDirection = Vector3.zero;
                if (transform.rotation.z < 90f)
                {
                    transform.Rotate(new Vector3(0f, 0f, 1f));
                }
            }
        }
        else if(BLookLeft == false)
        {
            if(rays.bottomMid && rays.bottomLeft)
            {
                moveDirection = -transform.right * moveSpeed;
            }
            else
            {
                moveDirection = Vector3.zero;
                if (transform.rotation.z > -90f)
                {
                    transform.Rotate(new Vector3(0f, 0f, -1f));
                }
            }
        }
        transform.position += moveDirection;
    }

    //private void OnDrawGizmos()
    //{
    //    Debug.DrawLine(transform.position, new Vector3(transform.position.x + 0.08f * Mathf.Cos(transform.rotation.z), transform.position.y + 0.08f * Mathf.Sin(transform.rotation.z)));
    //}
}
