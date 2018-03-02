﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class CharController : MonoBehaviour {

    PlayerInput input;
    Rigidbody2D rb;
    bool bGrounded = false;
    Animator anim;
    CapsuleCollider2D coll;
    
    [SerializeField] float speed = 1;
    [SerializeField] float jumpPower;
    [SerializeField] float jumpCap = 3f;
    [SerializeField] float fallMultiplier = 2f;
    [SerializeField] float dodgePower = 100f;
    [SerializeField] float dodgeUpPower = 20f;

    // Use this for initialization
    void Start () {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<CapsuleCollider2D>();
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        transform.position += new Vector3(input.Horizontal * speed * Time.deltaTime, 0f);
        //rb.velocity += new Vector2(input.Horizontal * speed * Time.deltaTime, 0f);
        CheckForJump();
        CheckForInput();
        CheckForDodge();
        CheckForAttack();
    }

    #region Input

    private void CheckForInput()
    {
        if(input.Horizontal < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            anim.SetBool("Idling", false);
        }
        else if(input.Horizontal > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            anim.SetBool("Idling", false);
        }
        else
        {
            anim.SetBool("Idling", true);
        }
    }

    #endregion

    #region Attack

    private void CheckForAttack()
    {
        if(input.Attack)
        {
            Attack();
        }
    }

    private void Attack()
    {
        rb.velocity += new Vector2(1f * transform.localScale.x, 0);
        anim.SetBool("Attacking", true);
    }

    private void EndAttack()
    {
        anim.SetBool("Attacking", false);
    }

    #endregion

    #region Dodge

    private void CheckForDodge()
    {
        if(input.Dodge && anim.GetBool("Dodging") == false)
        {
            Dodge();
        }
    }

    private void Dodge()
    {
        anim.SetBool("Dodging", true);
        coll.size = new Vector2(coll.size.x, coll.size.y / 2);
        rb.velocity += new Vector2(dodgePower * transform.localScale.x * speed * Time.deltaTime, dodgeUpPower * Time.deltaTime);
    }

    private void EndDodge()
    {
        anim.SetBool("Dodging", false);
        coll.size = new Vector2(coll.size.x, coll.size.y * 2);
    }

    #endregion

    #region Jump

    private void CheckForJump()
    {
        if (input.Jump == 2 && bGrounded)
        {
            Jump();
        }
        if (input.Jump == 1)
        {
            rb.velocity += new Vector2(0f, fallMultiplier * Time.deltaTime);
        }
        else
        {
            rb.velocity -= new Vector2(0f, fallMultiplier * Time.deltaTime);
        }
    }

    private void Jump()
    {
        while (rb.velocity.y <= jumpCap)
        {
            rb.velocity += new Vector2(0f, jumpPower * Time.deltaTime);
        }
    }

    #endregion

    #region Grounded

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (bGrounded == false)
        {
            bGrounded = true;
            anim.SetBool("Grounded", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        bGrounded = false;
        anim.SetBool("Grounded", false);
    }

    #endregion
}
