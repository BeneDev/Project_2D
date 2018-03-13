using System.Collections;
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
            bLookLeft = value;
        }
    }

    #endregion

    #region Fields

    struct Raycasts
    {
        public RaycastHit2D bottomLeft;
        public RaycastHit2D bottomMid;
        public RaycastHit2D bottomRight;
        public RaycastHit2D left;
        public RaycastHit2D right;
        public RaycastHit2D upperLeft;
        public RaycastHit2D upperRight;
    }
    private Raycasts rays;

    private bool bLookLeft = true;

    [SerializeField] float moveSpeed = 0.1f;

    #endregion

    void Start () {
        // Call the General Initialization, inherited from the GeneralEnemy Script
        GeneralInitialization();
	}
	
	void Update () {

        #region Raycast Initialization
        // Update all of the rays, used to check for ground under the enemy
        rays.bottomRight = Physics2D.Raycast(transform.position + new Vector3(-0.04f, -0.02f), Vector2.down, 0.04f, layersToCollideWith);
        rays.bottomMid = Physics2D.Raycast(transform.position + new Vector3(0f, -0.02f), Vector2.down, 0.04f, layersToCollideWith);
        rays.bottomLeft = Physics2D.Raycast(transform.position + new Vector3(0.04f, -0.02f), Vector2.down, 0.04f, layersToCollideWith);
        rays.left = Physics2D.Raycast(transform.position + new Vector3(0.04f, 0.06f), Vector2.right, 0.04f, layersToCollideWith);
        rays.right = Physics2D.Raycast(transform.position + new Vector3(-0.04f, 0.06f), Vector2.left, 0.04f, layersToCollideWith);
        rays.upperLeft = Physics2D.Raycast(transform.position + new Vector3(0.04f, 0.12f), Vector2.right, 0.04f, layersToCollideWith);
        rays.upperRight = Physics2D.Raycast(transform.position + new Vector3(-0.04f, 0.12f), Vector2.left, 0.04f, layersToCollideWith);
        #endregion

        if (knockBackCounter <= 0f && stunnedCounter <= 0f)
        {
            // Call the General Behavior, inherited from the GeneralEnemy Script
            GeneralBehavior();
            // Make the enemy move
            SimpleMove();
        }
        else if(knockBackCounter > 0)
        {
            ApplyKnockBack();
        }

        // Count down the knockbackTimer when he is above 0 and after that count down stunned timer
        if (knockBackCounter > 0f)
        {
            knockBackCounter -= Time.deltaTime;
        }
        if (stunnedCounter > 0f && knockBackCounter <= 0f)
        {
            stunnedCounter -= Time.deltaTime;
        }
    }

    ///// <summary>
    ///// Make the enemy Move around the platform. The generally applied gravity would be a problem for this. Postponed for undefined time.
    ///// </summary>
    //private void Move()
    //{
    //    if (BLookLeft == true)
    //    {
    //        if (rays.bottomMid && rays.bottomRight)
    //        {
    //            moveDirection = -transform.right * moveSpeed;
    //        }
    //        else
    //        {
    //            moveDirection = Vector3.zero;
    //            if (transform.rotation.z < 90f)
    //            {
    //                transform.Rotate(new Vector3(0f, 0f, 1f));
    //            }
    //        }
    //    }
    //    else if (BLookLeft == false)
    //    {
    //        if (rays.bottomMid && rays.bottomLeft)
    //        {
    //            moveDirection = -transform.right * moveSpeed;
    //        }
    //        else
    //        {
    //            moveDirection = Vector3.zero;
    //            if (transform.rotation.z > -90f)
    //            {
    //                transform.Rotate(new Vector3(0f, 0f, -1f));
    //            }
    //        }
    //    }
    //    transform.position += moveDirection;
    //}

    /// <summary>
    /// Make the enemy turn around, whenever he faces the end of the platform he's walking on or a wall in front of him
    /// </summary>
    private void SimpleMove()
    {
        // When the enemy if flying even though he shouldn't, a decision is made. This decision can be wrong. It gets checked by CheckIfStillBugged after 0.2 seconds
        if(!rays.bottomRight && !rays.bottomLeft)
        {
            bLookLeft = true;
            StartCoroutine(CheckIfStillBugged());
        }
        // When the enemy comes across the end of the platform he is moving on
        else if(!rays.bottomMid)
        {
            BLookLeft = !bLookLeft;
        }
        // When the enemy has walls either to the right or left side of him
        if(bLookLeft)
        {
            if(rays.left || rays.upperLeft)
            {
                BLookLeft = !bLookLeft;
            }
        }
        else if(!bLookLeft)
        {
            if(rays.right || rays.upperRight)
            {
                BLookLeft = !bLookLeft;
            }
        }
        // Applies the movement after all the checks above
        transform.position += new Vector3(moveSpeed, 0f) * transform.localScale.x;
    }

    /// <summary>
    /// Check if the right decision was made in Simple Move as both the bottom left and bottom right raycast hit nothting. Change decision after 0.2 seconds if it was wrong
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckIfStillBugged()
    {
        yield return new WaitForSeconds(0.2f);
        if (!rays.bottomRight && !rays.bottomLeft)
        {
            bLookLeft = !BLookLeft;
        }
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position + new Vector3(-0.04f, 0.12f), Vector2.left * 0.04f);
        Debug.DrawRay(transform.position + new Vector3(0.04f, 0.12f), Vector2.right * 0.04f);
    }
}
