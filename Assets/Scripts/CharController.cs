using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class CharController : MonoBehaviour {

    PlayerInput input;
    Rigidbody2D rb;
    bool bGrounded = false;
    Animator anim;
    
    [SerializeField] float speed = 1;
    [SerializeField] float jumpPower;
    [SerializeField] float jumpCap = 3f;
    [SerializeField] float fallMultiplier = 2f;

    // Use this for initialization
    void Start () {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        transform.position += new Vector3(input.Horizontal * speed * Time.deltaTime, 0f);
        //rb.velocity += new Vector2(input.Horizontal * speed * Time.deltaTime, 0f);
        CheckForGrounded();
        CheckForJump();
        CheckForInput();
    }

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

    private void CheckForJump()
    {
        if (Input.GetButtonDown("Jump") && bGrounded)
        {
            Jump();
        }
        if (Input.GetButton("Jump"))
        {
            rb.velocity += new Vector2(0f, fallMultiplier * Time.deltaTime);
        }
        else
        {
            rb.velocity -= new Vector2(0f, fallMultiplier * Time.deltaTime);
        }
    }

    private void CheckForGrounded()
    {
        if (Physics2D.Raycast(transform.position + new Vector3(-0.04f, 0f, 0f), Vector2.down, 0.08f) || Physics2D.Raycast(transform.position + new Vector3(0.04f, 0f, 0f), Vector2.down, 0.08f))
        {
            bGrounded = true;
            anim.SetBool("Grounded", true);
        }
        else
        {
            bGrounded = false;
            anim.SetBool("Grounded", false);
        }
    }

    private void Jump()
    {
        while (rb.velocity.y <= jumpCap)
        {
            rb.velocity += new Vector2(0f, input.Jump * jumpPower * Time.deltaTime);
        }
    }

    //private void OnDrawGízmos()
    //{
    //    Gizmos.DrawLine(transform.position, (transform.position + Vector3.down) * 0.08f);
    //}
}
