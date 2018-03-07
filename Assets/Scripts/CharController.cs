using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class CharController : MonoBehaviour {

    #region Fields

    // Delegate for Healht changes
    public event System.Action<int> OnHealthChanged;

    PlayerInput input; // Stores the input giving class
    Animator anim; 

    bool bGrounded = false; // Stores if the player is on the ground or not
    bool bOnWall = false; // Stores if the player is on a wall or not
    bool bKnockedBack = false; // Stores when the player is knocked back to prevent him from moving
    bool bAlreadyHit = false; // Makes sure, the player only hits one time with one attack

    Vector3 velocity; // The value, which is solely allowed to manipulate the transform directly
    Vector3 knockBackForce; // The actual force with which the player is getting knocked back when being hit

    RaycastHit2D hit; // The ray cast hit in which the enemy under attack gets stored in 

    // The attributes of the player
    [SerializeField] int maxHealth = 100;
    private int health = 100;

    [SerializeField] int baseAttack = 5;
    private int attack = 5;

    [SerializeField] int baseDefense = 5;
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
    private PlayerRaycasts raycasts; // Stores the actual information of the raycasts to calculate physics

    public enum State
    {
        freeToMove,
        dodging,
        attacking,
        knockedBack
    }; // State machine for the player
    public State playerState = State.freeToMove; // Stores the current state of the player

    // Walking speed of the Player
    [SerializeField] float speed = 1;

    // Fields to manipulate the jump
    [SerializeField] float jumpPower = 10;
    [SerializeField] float fallMultiplier = 2f; // The higher this value, the slower the player will fall after jumping up, when still holding jump and the faster he will fall when not holding it
    [SerializeField] float gravity = 2f;
    [SerializeField] float veloYLimit = 10f; // The player cannot fall faster than this value to prevent him falling through hitboxes

    // Fields to manipulate the Dodge
    [SerializeField] float dodgePower = 100f; // Force forward when dodging
    [SerializeField] float dodgeUpPower = 20f; // This defines the applied Dodge Up Power
    private float appliedDodgeUpPower; // The actual force getting applied upwards when dodging
    [SerializeField] float dodgeCooldown = 1f;
    bool bDodgable = true; // Stores wether the player is able to dodge or not

    // Fields to manipulate the attack
    [SerializeField] float attackReach = 0.2f; // How far the attack hitbox reaches
    [SerializeField] Vector2 attackVelo; // this defines how big the actually applied force while attatcking will be
    Vector3 appliedAttackVelo; // the actual velocity which is applied to the player when attacking
    [SerializeField] float attackCooldown = 1f;
    bool bAttackable = true; // Stores wether the player is able to attack or not
    [SerializeField] float knockBackStrength = 3f; // The amount of knockback the player is applying to hit enemies
    Vector2 attackDirection; // The direction for the raycast, checking for enemies to hit

    [SerializeField] float wallSlideSpeed = 3f; // How fast the player slides down a wall while holding towards it

    #endregion

    void Start () {
        input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();

        if (OnHealthChanged != null)
        {
            OnHealthChanged(health);
        }

        // Make the player have full health
        health = maxHealth;
    }
	
	void FixedUpdate ()
    {
        #region Raycasts Initialization
        // Update all the different raycast hit values to calculate physics
        raycasts.bottomRight = Physics2D.Raycast(transform.position + Vector3.right * 0.01f + Vector3.down * 0.04f, Vector2.down, 0.01f);
        raycasts.bottomLeft = Physics2D.Raycast(transform.position + Vector3.right * -0.02f + Vector3.down * 0.04f, Vector2.down, 0.01f);

        raycasts.upperRight = Physics2D.Raycast(transform.position + Vector3.up * 0.03f + Vector3.right * 0.02f, Vector2.left, 0.01f);
        raycasts.lowerRight = Physics2D.Raycast(transform.position + Vector3.up * -0.04f + Vector3.right * 0.02f, Vector2.left, 0.01f);

        raycasts.upperLeft = Physics2D.Raycast(transform.position + Vector3.up * 0.03f + Vector3.right * -0.03f, Vector2.right, 0.01f);
        raycasts.lowerLeft = Physics2D.Raycast(transform.position + Vector3.up * -0.04f + Vector3.right * -0.03f, Vector2.right, 0.01f);

        raycasts.top = Physics2D.Raycast(transform.position + Vector3.right * -0.001f, Vector2.up, 0.02f);
        #endregion

        // Setting the x velocity when player is not knocked back
        if (!bKnockedBack && playerState != State.attacking)
        {
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
                if (input.Horizontal != 0f || input.Vertical != 0f)
                {
                    appliedAttackVelo = new Vector3(input.Horizontal * appliedAttackVelo.x * Time.deltaTime, input.Vertical * appliedAttackVelo.y * Time.deltaTime);
                }
                else
                {
                    appliedAttackVelo = new Vector3(transform.localScale.x * appliedAttackVelo.x * Time.deltaTime, 0f);
                }
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
            AttackHitboxOut(attackDirection);
        }

        // Apply knockback when the player is currently getting knocked back
        if(bKnockedBack)
        {
            velocity += knockBackForce * Time.deltaTime;
        }

        // Checking if the calculated velocity is fine with the world and restrictions
        CheckForValidVelocity();

        // Apply the velocity to the transform
        transform.position += velocity;

        // Reset the player to Debug quickly
        if (transform.position.y < -5f)
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
    
    // Checks if the Player is giving directional input to walk or not and turn him accordingly
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

    // Make the player attack, setting the direction of attack, hitbox and animation fields
    private void Attack()
    {
        if (input.Horizontal != 0f || input.Vertical != 0f)
        {
            attackDirection = new Vector2(input.Horizontal, input.Vertical);
        }
        else
        {
            attackDirection = new Vector2(transform.localScale.x, 0f);
        }
        appliedAttackVelo = attackVelo;
        bAttackable = false;
        anim.SetBool("Attacking", true);
        playerState = State.attacking;
    }

    // Check if an enemy is hit with the ray in the direction of the attack and damages him if so
    private void AttackHitboxOut(Vector2 direction)
    {
        hit = Physics2D.Raycast(transform.position, direction, attackReach);
        if (hit.collider && bAlreadyHit == false)
        {
            if(hit.collider.tag == "Enemy")
            {
                // Calculate the direction, the player has to knock the opponent away
                Vector3 knockDirection = hit.collider.gameObject.transform.position - transform.position;
                hit.collider.gameObject.GetComponent<EnemyController>().TakeDamage(attack, knockDirection.normalized * knockBackStrength);
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

    // Waits for the attack to be available again
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
        if(input.Dodge && playerState != State.dodging && bDodgable)
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
        bDodgable = false;
    }

    // End the Dodge process
    private void EndDodge()
    {
        anim.SetBool("Dodging", false);
        playerState = State.freeToMove;
        StartCoroutine(DodgeCooldown());
    }

    // Wait for the dodge to be available again
    IEnumerator DodgeCooldown()
    {
        yield return new WaitForSeconds(dodgeCooldown);
        bDodgable = true;
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
            // Wait for the knockback to stop and giving the player free to move again
            StartCoroutine(UntilKnockBackStops(0.05f));
            bKnockedBack = true;
            health -= damage;
            if (OnHealthChanged != null)
            {
                OnHealthChanged(health);
            }
            // Set the knockback force to be applied
            knockBackForce = knockBack;
        }
    }

    // Wait until able to move freely again
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
