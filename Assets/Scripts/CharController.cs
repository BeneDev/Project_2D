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
    private Vector3 velocity;

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
    [SerializeField] float jumpPower = 10;
    [SerializeField] float fallMultiplier = 2f;
    [SerializeField] float dodgePower = 100f;
    [SerializeField] float dodgeUpPower = 20f;
    [SerializeField] float attackVelocity = 10f;
    [SerializeField] float wallSlideSpeed = 3f;
    [SerializeField] float gravity = 2f;
    [SerializeField] float veloYLimit = 10f;

    // Use this for initialization
    void Start () {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        #region Raycasts Initialization
        // Update all the different raycast hit values
        raycasts.bottomLeft = Physics2D.Raycast(transform.position + Vector3.right * 0.01f, Vector2.down, 0.05f);
        raycasts.bottomRight = Physics2D.Raycast(transform.position + Vector3.right * -0.02f, Vector2.down, 0.05f);

        raycasts.upperLeft = Physics2D.Raycast(transform.position + Vector3.up * 0.03f, Vector2.left, 0.03f);
        raycasts.lowerLeft = Physics2D.Raycast(transform.position + Vector3.up * -0.04f, Vector2.left, 0.03f);

        raycasts.upperRight = Physics2D.Raycast(transform.position + Vector3.up * 0.03f, Vector2.right, 0.02f);
        raycasts.lowerRight = Physics2D.Raycast(transform.position + Vector3.up * -0.04f, Vector2.right, 0.02f);

        raycasts.top = Physics2D.Raycast(transform.position + Vector3.right * -0.001f, Vector2.up, 0.02f);
        #endregion

        // Setting the x velocity
        velocity = new Vector3(input.Horizontal * speed * Time.deltaTime, velocity.y);

        CheckGrounded();
        CheckOnWall();

        CheckForInput();
        CheckForJump();
        CheckForDodge();
        CheckForAttack();

        CheckForValidVelocity();

        // Apply gravity
        if (!bGrounded)
        {
            velocity += new Vector3(0, -gravity * Time.deltaTime);
        }

        // Apply the velocity to the transform after checking its validity
        transform.position += velocity;

        // Debug feature to test quickly
        if (transform.position.y < -10f)
        {
            Reset();
        }
    }

    private void CheckForValidVelocity()
    {
        // Checking for colliders to the sides
        if (raycasts.upperLeft.collider && raycasts.lowerLeft.collider && velocity.x < 0)
        {
            velocity.x = 0f;
        }
        else if (raycasts.upperRight.collider && raycasts.lowerRight.collider && velocity.x > 0)
        {
            velocity.x = 0f;
        }

        // Make sure, velocity in y axis does not get over limit
        if (velocity.y < veloYLimit)
        {
            velocity.y = veloYLimit;
        }

        // Check for possible Wallslide
        if(bOnWall && !bGrounded && HoldingInDirection() && velocity.y < 0)
        {
            velocity.y = -wallSlideSpeed;
        }

        // Checks if something is above the player and let him bounce down again relative to the force he went up with
        if (raycasts.top.collider && velocity.y > 0)
        {
            velocity.y = -velocity.y / 2;
        }
    }

    // Resets the players velocity and position to test quickly
    private void Reset()
    {
        transform.position = Vector3.zero;
        velocity = Vector3.zero;
    }

    //private void OnGUI()
    //{
    //    GUILayout.Label(raycasts.bottomLeft.collider + " right");
    //    GUILayout.Label(raycasts.bottomRight.collider+ " left");
    //}

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
        velocity += new Vector3(attackVelocity * transform.localScale.x * Time.deltaTime, 0);
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
        velocity += new Vector3(dodgePower * transform.localScale.x * speed * Time.deltaTime, dodgeUpPower * Time.deltaTime);
    }

    private void EndDodge()
    {
        anim.SetBool("Dodging", false);
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
            velocity += new Vector3(0f, fallMultiplier * Time.deltaTime);
        }
        else if(!bGrounded)
        {
            velocity -= new Vector3(0f, fallMultiplier * Time.deltaTime);
        }
    }

    private void Jump()
    {
        if(bGrounded)
        {
            velocity += new Vector3(0f, jumpPower * Time.deltaTime);
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
                velocity.y = 0f;
                anim.SetBool("Grounded", true);
            }
        }
        else if (raycasts.bottomRight.collider)
        {
            if (raycasts.bottomRight.collider.tag == "Ground")
            {
                bGrounded = true;
                velocity.y = 0f;
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
        if (input.Horizontal < 0 && transform.localScale.x < 0)
        {
            return true;
        }
        else if (input.Horizontal > 0 && transform.localScale.x > 0)
        {
            return true;
        }
        return false;
    }

    #endregion
}
