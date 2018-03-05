using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class CharController : MonoBehaviour {

    PlayerInput input;
    Rigidbody2D rb;
    bool bGrounded = false;
    bool bOnWall = false;
    Animator anim;
    CapsuleCollider2D coll;

    public struct PlayerRaycasts
    {
        public RaycastHit2D bottomLeft;
        public RaycastHit2D bottomRight;
        public RaycastHit2D upperLeft;
        public RaycastHit2D lowerLeft;
        public RaycastHit2D upperRight;
        public RaycastHit2D lowerRight;
        public RaycastHit2D top;
    }

    private PlayerRaycasts raycasts;

    [SerializeField] float speed = 1;
    [SerializeField] float jumpPower;
    [SerializeField] float jumpCap = 3f;
    [SerializeField] float fallMultiplier = 2f;
    [SerializeField] float dodgePower = 100f;
    [SerializeField] float dodgeUpPower = 20f;
    [SerializeField] float wallSlideSpeed = 3f;
    [SerializeField] float gravity = 2f;

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
        // Update all the different raycast hit values
        raycasts.bottomLeft = Physics2D.Raycast(transform.position + Vector3.right * 0.01f, Vector2.down, 0.08f);
        raycasts.bottomRight = Physics2D.Raycast(transform.position + Vector3.right * -0.02f, Vector2.down, 0.08f);

        raycasts.upperLeft = Physics2D.Raycast(transform.position + Vector3.up * 0.03f, Vector2.left, 0.03f);
        raycasts.lowerLeft = Physics2D.Raycast(transform.position + Vector3.up * -0.04f, Vector2.left, 0.03f);

        raycasts.upperRight = Physics2D.Raycast(transform.position + Vector3.up * 0.03f, Vector2.right, 0.02f);
        raycasts.lowerRight = Physics2D.Raycast(transform.position + Vector3.up * -0.04f, Vector2.right, 0.02f);

        raycasts.top = Physics2D.Raycast(transform.position + Vector3.right * -0.005f, Vector2.up, 0.06f);

        if (!raycasts.upperLeft.collider && !raycasts.lowerLeft.collider && input.Horizontal < 0)
        {
            transform.position += new Vector3(input.Horizontal * speed * Time.deltaTime, 0f);
        } 
        else if(!raycasts.upperRight.collider && !raycasts.lowerRight.collider && input.Horizontal > 0)
        {
            transform.position += new Vector3(input.Horizontal * speed * Time.deltaTime, 0f);
        }

        CheckGrounded();
        CheckOnWall();

        // Apply gravity
        if (!bGrounded)
        {
            transform.position += new Vector3(0, -gravity * Time.deltaTime);
        }

        CheckForJump();
        CheckForInput();
        CheckForDodge();
        CheckForAttack();
        CheckForWallSlide();
    }

    private void OnGUI()
    {
        GUILayout.Label(raycasts.bottomLeft.collider + " right");
        GUILayout.Label(raycasts.bottomRight.collider+ " left");
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawRay(transform.position + Vector3.right * -0.005f, Vector2.up * 0.06f);
    //}

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
        transform.position += new Vector3(1f * transform.localScale.x * Time.deltaTime, 0);
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
        if (input.Jump == 2 && bGrounded || input.Jump == 2 && bOnWall)
        {
            Jump();
        }
        if (input.Jump == 1 && !bGrounded)
        {
            transform.position += new Vector3(0f, fallMultiplier * Time.deltaTime);
        }
        else if(!bGrounded)
        {
            transform.position -= new Vector3(0f, fallMultiplier * Time.deltaTime);
        }
    }

    private void Jump()
    {
        if(bGrounded)
        {
            transform.position += new Vector3(0f, jumpPower * Time.deltaTime);
        }
        else if(bOnWall)
        {
            transform.position += new Vector3(jumpPower / 2 * -transform.localScale.x * Time.deltaTime, jumpPower * Time.deltaTime);
        }
    }

    #endregion

    #region OnWall

    private void CheckForWallSlide()
    {
        if(bOnWall && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region Grounded

    private void CheckGrounded()
    {
        if (raycasts.bottomLeft.collider)
        {
            if (raycasts.bottomLeft.collider.tag == "Ground")
            {
                bGrounded = true;
                anim.SetBool("Grounded", true);
            }
        }
        else if (raycasts.bottomRight.collider)
        {
            if (raycasts.bottomRight.collider.tag == "Ground")
            {
                bGrounded = true;
                anim.SetBool("Grounded", true);
            }
        }
        else
        {
            bGrounded = false;
            anim.SetBool("Grounded", false);
        }
    }

    private void CheckOnWall()
    {
        if (HoldingInDirection() && raycasts.lowerLeft.collider || raycasts.upperLeft.collider || raycasts.lowerRight.collider || raycasts.upperRight.collider)
        {
            bOnWall = true;
        }
        else
        {
            bOnWall = false;
        }
    }

    private bool HoldingInDirection()
    {
        if (input.Horizontal < 0 && transform.localScale.x == -1)
        {
            return true;
        }
        else if (input.Horizontal > 0 && transform.localScale.x == 1)
        {
            return true;
        }
        return false;
    }

    #endregion
}
