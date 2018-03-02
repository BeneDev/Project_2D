using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class CharController : MonoBehaviour {

    PlayerInput input;
    Rigidbody2D rb;
    bool bGrounded = false;
    Animator anim;
    CapsuleCollider2D collider;
    
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
        collider = GetComponent<CapsuleCollider2D>();
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        transform.position += new Vector3(input.Horizontal * speed * Time.deltaTime, 0f);
        //rb.velocity += new Vector2(input.Horizontal * speed * Time.deltaTime, 0f);
        CheckForJump();
        CheckForInput();
        CheckForDodge();
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

    #region Dodge

    private void CheckForDodge()
    {
        if(input.Dodge && anim.GetBool("Dodging") == false)
        {
            if (transform.localScale == new Vector3(1f, 1f, 1f))
            {
                Dodge(1);
            }
            else if(transform.localScale == new Vector3(-1f, 1f, 1f))
            {
                Dodge(-1);
            }
        }
    }

    private void Dodge(int direction)
    {
        anim.SetBool("Dodging", true);
        collider.size = new Vector2(collider.size.x, collider.size.y / 2);
        rb.velocity += new Vector2(dodgePower * speed * direction * Time.deltaTime, dodgeUpPower * Time.deltaTime);
    }

    private void EndDodge()
    {
        anim.SetBool("Dodging", false);
        collider.size = new Vector2(collider.size.x, collider.size.y * 2);
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
