using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class CharController : MonoBehaviour {

    public event System.Action<int> OnHealthChanged;

    PlayerInput input;
    bool bGrounded = false;
    bool bOnWall = false;
    bool bKnockedBack = false; // Stores when the player is knocked back to prevent him from moving
    bool bAlreadyHit = false; // Makes sure, the player only hits one time with one attack
    Animator anim;
    private Vector3 velocity; // The value, which is solely allowed to manipulate the transform directly
    Vector3 knockBackForce;

    RaycastHit2D hit;

    private EnemyController enemy;

    // The attributes of the player
    [SerializeField] int maxHealth = 100;

    private int health = 100;
    private int attack = 5;
    private int defense = 5;

    struct PlayerRaycasts // To store the informations of raycasts around the player to calculate physics
    {
        public RaycastHit2D bottomLeft;
        public RaycastHit2D bottomRight;
        public RaycastHit2D upperLeft;
        public RaycastHit2D lowerLeft;
        public RaycastHit2D upperRight;
        public RaycastHit2D lowerRight;
        public RaycastHit2D top;
    }

    public enum State
    {
        freeToMove,
        dodging,
        attacking,
        knockedBack
    };

    public State playerState = State.freeToMove;

    private PlayerRaycasts raycasts;

    [SerializeField] float speed = 1;
    [SerializeField] float jumpPower = 10;
    [SerializeField] float fallMultiplier = 2f;
    [SerializeField] float dodgePower = 100f;
    [SerializeField] float dodgeUpPower = 20f;
    private float appliedDodgeUpPower;
    [SerializeField] Vector2 attackVelo;
    Vector3 appliedAttackVelo;
    [SerializeField] float attackCooldown = 1f;
    bool bAttackable = true;
    [SerializeField] float knockBackStrength = 3f;
    [SerializeField] float wallSlideSpeed = 3f;
    [SerializeField] float gravity = 2f;
    [SerializeField] float veloYLimit = 10f;
    
    void Start () {
        input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();

        // Make the player have full health
        health = maxHealth;
    }
	
	void FixedUpdate ()
    {
        #region Raycasts Initialization
        // Update all the different raycast hit values
        raycasts.bottomRight = Physics2D.Raycast(transform.position + Vector3.right * 0.01f + Vector3.down * 0.04f, Vector2.down, 0.01f);
        raycasts.bottomLeft = Physics2D.Raycast(transform.position + Vector3.right * -0.02f + Vector3.down * 0.04f, Vector2.down, 0.01f);

        raycasts.upperRight = Physics2D.Raycast(transform.position + Vector3.up * 0.03f + Vector3.right * 0.02f, Vector2.left, 0.01f);
        raycasts.lowerRight = Physics2D.Raycast(transform.position + Vector3.up * -0.04f + Vector3.right * 0.02f, Vector2.left, 0.01f);

        raycasts.upperLeft = Physics2D.Raycast(transform.position + Vector3.up * 0.03f + Vector3.right * -0.03f, Vector2.right, 0.01f);
        raycasts.lowerLeft = Physics2D.Raycast(transform.position + Vector3.up * -0.04f + Vector3.right * -0.03f, Vector2.right, 0.01f);

        raycasts.top = Physics2D.Raycast(transform.position + Vector3.right * -0.001f, Vector2.up, 0.02f);
        #endregion

        if (!bKnockedBack)
        {
            // Setting the x velocity
            velocity = new Vector3(input.Horizontal * speed * Time.deltaTime, velocity.y);
        }

        // Checking for collider close to the player
        CheckGrounded();
        CheckOnWall();

        if (!bKnockedBack)
        {
            // Checking for actions, the player can do
            CheckForInput();
            // Start the jumping process if wanted
            CheckForJump();
            // Start the dodging process if wanted
            CheckForDodge();
            // Checks if the player wants to attack of not
            if (input.Attack && bAttackable == true)
            {
                Attack();
                appliedAttackVelo = new Vector3(input.Horizontal * appliedAttackVelo.x * Time.deltaTime, input.Vertical * appliedAttackVelo.y * Time.deltaTime);
            }
        }

        // Apply gravity
        if (!bGrounded)
        {
            velocity += new Vector3(0, -gravity * Time.deltaTime);
        }

        // Apply attack velocity when attacking
        if (playerState == State.attacking)
        {
            velocity = Vector2.zero;
            velocity += appliedAttackVelo;
            appliedAttackVelo.x -= appliedAttackVelo.x / 100;
            appliedAttackVelo.y -= appliedAttackVelo.y / 100;
        }

        if(bKnockedBack)
        {
            velocity += knockBackForce * Time.deltaTime;
        }

        // Checking if the calculated velocity is fine with the world and restrictions
        CheckForValidVelocity();

        // Apply the velocity to the transform
        transform.position += velocity;

        // Debug feature to test quickly
        if (transform.position.y < -10f)
        {
            Reset();
        }
    }

    #region Helper Methods

    // Make sure the velocity does not violate the laws of physics in this game
    private void CheckForValidVelocity()
    {
        // Check for ground under the player
        if(bGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        // Checking for colliders to the sides
        if (WallInWay())
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

        // Check if something is above the player and let him bounce down again relative to the force he went up with
        if (raycasts.top.collider && velocity.y > 0)
        {
            velocity.y = -velocity.y / 2;
        }
    }

    // Checks if there are walls in the direction the player is facing
    private bool WallInWay()
    {
        if (transform.localScale.x < 0)
        {
            if (raycasts.upperLeft.collider || raycasts.lowerLeft.collider)
            {
                return true;
            }
        }
        else if (transform.localScale.x > 0)
        {
            if (raycasts.upperRight.collider || raycasts.lowerRight.collider)
            {
                return true;
            }
        }
        return false;
    }

    // Checks if the player is holding the direction, hes facing in
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

    #region Debugging Tools

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
    //    Gizmos.DrawRay(transform.position + Vector3.up * 0.03f + Vector3.right * -0.03f, Vector2.right * 0.01f);
    //    Gizmos.DrawRay(transform.position + Vector3.up * -0.04f + Vector3.right * -0.03f, Vector2.right * 0.01f);
    //}

    #endregion

    #region Input
    
    // Checks if the Player is giving directional input to walk or not
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

    // Make the player attack
    private void Attack()
    {
        Vector2 attackDirection = new Vector2(input.Horizontal, input.Vertical);
        if (attackDirection.x != 0 || attackDirection.y != 0)
        {
            AttackHitboxOut(attackDirection);
        }
        else
        {
            attackDirection = new Vector2(transform.localScale.x, 0f);
            AttackHitboxOut(attackDirection);
        }
        appliedAttackVelo = attackVelo;
        bAttackable = false;
        anim.SetBool("Attacking", true);
        playerState = State.attacking;
    }

    // Check if an enemy is hit with the ray in the direction of the attack
    private void AttackHitboxOut(Vector2 direction)
    {
        hit = Physics2D.Raycast(transform.position, direction, 0.08f);
        if (hit.collider && bAlreadyHit == false)
        {
            if(hit.collider.tag == "Enemy")
            {
                // Calculate the direction, the player has to knock the opponent away
                Vector3 knockDirection = hit.collider.gameObject.transform.position - transform.position;
                hit.collider.gameObject.GetComponent<EnemyController>().TakeDamage(attack, knockDirection * knockBackStrength);
                bAlreadyHit = true;
            }
        }
    }

    // End the attack
    private void EndAttack()
    {
        anim.SetBool("Attacking", false);
        playerState = State.freeToMove;
        bAlreadyHit = false;
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        bAttackable = true;
    }

    #endregion

    #region Dodge

    // Set up the dodging process
    private void CheckForDodge()
    {
        if(input.Dodge && playerState != State.dodging)
        {
            anim.SetBool("Dodging", true);
            playerState = State.dodging;
            appliedDodgeUpPower = dodgeUpPower;
            Dodge();
        }
        if(playerState == State.dodging)
        {
            appliedDodgeUpPower -= appliedDodgeUpPower / 10;
            Dodge();
        }
    }

    // The actual application of force whilst dodging
    private void Dodge()
    {
        velocity += new Vector3(dodgePower * transform.localScale.x * speed * Time.deltaTime, appliedDodgeUpPower * Time.deltaTime);
    }

    // End the Dodge process
    private void EndDodge()
    {
        anim.SetBool("Dodging", false);
        playerState = State.freeToMove;
    }

    #endregion

    #region Jump

    // Start the Jump process
    private void CheckForJump()
    {
        if (input.Jump == 2 && bGrounded || input.Jump == 2 && bOnWall)
        {
            Jump();
        }
        // Make the player fall less fast when still holding the jump button
        if (input.Jump == 1 && !bGrounded)
        {
            velocity += new Vector3(0f, fallMultiplier * Time.deltaTime);
        }
        // Make the player fall faster when not holding the jump button anymore
        else if(!bGrounded)
        {
            velocity -= new Vector3(0f, fallMultiplier * Time.deltaTime);
        }
    }

    // The application of the main jumping force
    private void Jump()
    {
        if(bGrounded)
        {
            velocity += new Vector3(0f, jumpPower * Time.deltaTime);
        }
    }

    #endregion

    #region Damage Calculation

    // Damages the player 
    public void TakeDamage(int damage, Vector3 knockBack)
    {
        if (playerState != State.attacking && playerState != State.dodging)
        {
            StartCoroutine(UntilKnockBackStops(0.05f));
            bKnockedBack = true;
            health -= damage;
            //if (OnHealthChanged != null)
            //{
            //    OnHealthChanged(health);
            //}
            knockBackForce = knockBack;
            print("new health player = " + health.ToString());
        }
    }

    IEnumerator UntilKnockBackStops(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        bKnockedBack = false;
    }

#endregion

    #region Grounded and OnWall

    // Checks if the player is on the ground or not
    private void CheckGrounded()
    {
        // When the bottom left collider hit something
        if (raycasts.bottomLeft.collider)
        {
            // And the actual object, which is hit, is tagged as ground
            if (raycasts.bottomLeft.collider.tag == "Ground")
            {
                bGrounded = true;
                velocity.y = 0f;
                anim.SetBool("Grounded", true);
                playerState = State.freeToMove;
            }
        }
        // When the bottom right collider hit something
        else if (raycasts.bottomRight.collider)
        {
            // And the actual object, which is hit, is tagged as ground
            if (raycasts.bottomRight.collider.tag == "Ground")
            {
                bGrounded = true;
                velocity.y = 0f;
                anim.SetBool("Grounded", true);
                playerState = State.freeToMove;
            }
        }
        // Otherwise the player is not grounded
        else
        {
            bGrounded = false;
            anim.SetBool("Grounded", false);
        }
    }

    // Checks if the player is up against a wall
    private void CheckOnWall()
    {
        // When the player is against a wall
        if (raycasts.lowerLeft.collider || raycasts.upperLeft.collider || raycasts.lowerRight.collider || raycasts.upperRight.collider)
        {
            bOnWall = true;
        }
        else
        {
            bOnWall = false;
        }
    }

    #endregion
}
