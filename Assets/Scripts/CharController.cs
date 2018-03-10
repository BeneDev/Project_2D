using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Script takes in the Input from the PlayerInput Script, and handles the player, interacting with the physics system and overall world
/// </summary>

[RequireComponent(typeof(PlayerInput))]
public class CharController : MonoBehaviour {

    #region Properties

    int Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            if (OnHealthChanged != null)
            {
                OnHealthChanged(health);
            }
        }
    }

    int HealthJuice
    {
        get
        {
            return healthJuice;
        }
        set
        {
            healthJuice = value;
            if (OnHealthJuiceChanged != null)
            {
                OnHealthJuiceChanged(healthJuice);
            }
        }
    }

    #endregion

    #region Fields

    // Delegate for Health changes
    public event System.Action<int> OnHealthChanged;

    // Delegate for Healht  Juice changes
    public event System.Action<int> OnHealthJuiceChanged;

    PlayerInput input; // Stores the input giving class
    Animator anim;

    [SerializeField] LayerMask layersToCollideWith;

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

    [SerializeField] int maxHealthJuice = 100;
    private int healthJuice = 100;

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
    private RaycastHit2D[] anyRaycast = new RaycastHit2D[7];

    public enum State
    {
        freeToMove,
        dodging,
        attacking,
        knockedBack,
        healing
    }; // State machine for the player
    public State playerState = State.freeToMove; // Stores the current state of the player

    // Walking speed of the Player
    [SerializeField] float speed = 1;

    // Fields to manipulate the jump
    [SerializeField] float jumpPower = 10;
    [SerializeField] float fallMultiplier = 2f; // The higher this value, the slower the player will fall after jumping up, when still holding jump and the faster he will fall when not holding it
    [SerializeField] float gravity = 2f;
    [SerializeField] float veloYLimit = 10f; // The player cannot fall faster than this value to prevent him falling through hitboxes

    [SerializeField] float knockBackCapY = 2f; // the highest velocity the player can be vertically knocked back
    [SerializeField] float knockBackDuration = 0.05f; // The amount of seconds, the player will be knocked back

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

    [SerializeField] int healDuration = 5; // The frames one has to wait in between one transfer of Health juice to health
    private int healCounter = 0; // The actual counter for the heal duration
    [SerializeField] int juiceRegenValue = 10;

    [SerializeField] float invincibilityTime = 1f; // The amount of seconds, the player is invincible after getting hit
    private float invincibilityCounter = 0f; // This counts down until player can be hit again. Only if this value is 0, the player can be hit.


    [SerializeField] float wallSlideSpeed = 3f; // How fast the player slides down a wall while holding towards it

    #endregion

    /// <summary>
    /// Getting some references and setting the Health and HealthJuice values right
    /// </summary>
    void Start () {
        input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();

        // Make the player have full health
        Health = maxHealth;

        // Make the player have full health Juice
        HealthJuice = maxHealthJuice;

        // Make the player have the base attack value at start
        attack = baseAttack;
    }

    /// <summary>
    /// Counting down the Invincibility Counter
    /// </summary>
    private void Update()
    {
        // Count down invincibility counter when he is over 0
        if(invincibilityCounter > 0)
        {
            invincibilityCounter -= Time.deltaTime;
        }
        else if(invincibilityCounter != 0)
        {
            invincibilityCounter = 0;
        }
    }

    /// <summary>
    /// Handling Physics, Actions and States of the player
    /// </summary>
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

        anyRaycast[0] = raycasts.bottomRight;
        anyRaycast[1] = raycasts.bottomLeft;
        anyRaycast[2] = raycasts.lowerLeft;
        anyRaycast[3] = raycasts.upperLeft;
        anyRaycast[4] = raycasts.lowerRight;
        anyRaycast[5] = raycasts.upperRight;
        anyRaycast[6] = raycasts.top;
        #endregion

        // Setting the x velocity when player is not knocked back
        if (!bKnockedBack && playerState != State.attacking && playerState != State.healing)
        {
            velocity = new Vector3(input.Horizontal * speed * Time.deltaTime, velocity.y);
        }

        CheckGrounded();

        if (!bKnockedBack)
        {
            if (playerState != State.healing)
            {
                // Checking for actions, the player can do
                CheckForInput();
            }
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
            // Checks for input for healing
            if(input.Heal && HealthJuice > 0 && Health < maxHealth)
            {
                playerState = State.healing;
                velocity = Vector3.zero;
                // TODO set anim boolean to healing to change animation when there is one
                Heal();
            }
            else if(playerState == State.healing)
            {
                playerState = State.freeToMove;
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
            if (bAlreadyHit)
            {
                velocity = Vector2.zero;
                playerState = State.freeToMove;
            }
            else
            {
                velocity = Vector2.zero;
                velocity += appliedAttackVelo;
                appliedAttackVelo.x -= appliedAttackVelo.x / 100;
                appliedAttackVelo.y -= appliedAttackVelo.y / 100;
                AttackHitboxOut(attackDirection);
            }
        }

        // Apply knockback when the player is currently getting knocked back
        if(bKnockedBack)
        {
            if (knockBackForce.y > knockBackCapY)
            {
                knockBackForce.y = knockBackCapY;
            }
            if(knockBackForce.y < 0)
            {
                knockBackForce.y = 0;
            }
            // Check if knockback would let player end up in wall, if not apply it
            if (!Physics2D.Raycast(transform.position, knockBackForce, knockBackForce.magnitude, layersToCollideWith))
            {
                transform.position += knockBackForce;
            }
            else
            {
                // Get Knocked back onto the wall
                while (!Physics2D.Raycast(transform.position, knockBackForce, knockBackForce.magnitude / 10, layersToCollideWith))
                {
                    transform.position += knockBackForce / 10;
                }
            }
        }

        if(WhichRaycastForTag("Juice", anyRaycast) != null)
        {
            if(HealthJuice + juiceRegenValue < maxHealthJuice)
            {
                HealthJuice += juiceRegenValue;
                RaycastHit2D hitJuiceParticle = (RaycastHit2D)WhichRaycastForTag("Juice", anyRaycast);
                Destroy(hitJuiceParticle.collider.gameObject);
            }
            else if(HealthJuice != maxHealth)
            {
                HealthJuice = maxHealthJuice;
                RaycastHit2D hitJuiceParticle = (RaycastHit2D)WhichRaycastForTag("Juice", anyRaycast);
                Destroy(hitJuiceParticle.collider.gameObject);
            }
        }

        // Checking if the calculated velocity is fine with the world and restrictions
        CheckForValidVelocity();

        // Apply the velocity to the transform
        transform.position += velocity;

        // Reset the player to Debug quickly
        if (transform.position.y < -5f)
        {
            Respawn();
        }

        if(WhichRaycastForTag("Checkpoint", anyRaycast) != null)
        {
            RaycastHit2D newCheckpoint = (RaycastHit2D)WhichRaycastForTag("Checkpoint", anyRaycast);
            GameManager.Instance.currentCheckpoint = newCheckpoint.collider.gameObject.transform.position;
        }

        // Respawns the player if health is gone
        if(Health <= 0)
        {
            Respawn();
        }
    }

    #region Helper Methods

    /// <summary>
    ///  Respawns the player at the currently activated checkpoint
    /// </summary>
    private void Respawn()
    {
        transform.position = GameManager.Instance.currentCheckpoint;
        bKnockedBack = false;
        Health = maxHealth;
        HealthJuice = maxHealthJuice;
    }

    /// <summary>
    /// Make sure the velocity does not violate the laws of physics in this game
    /// </summary>
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
        if (RaycastForTag("Ground", raycasts.top) && velocity.y > 0)
        {
            velocity.y = -velocity.y / 2;
        }
    }

    /// <summary>
    /// Checks if there are walls in the direction the player is facing
    /// </summary>
    /// <returns> True if there is a wall. False when there is none</returns>
    private bool WallInWay()
    {
        if (transform.localScale.x < 0)
        {
            if (RaycastForTag("Ground", raycasts.upperLeft, raycasts.lowerLeft))
            {
                bOnWall = true;
                return true;
            }
        }
        else if (transform.localScale.x > 0)
        {
            if (RaycastForTag("Ground", raycasts.upperRight, raycasts.lowerRight))
            {
                bOnWall = true;
                return true;
            }
        }
        bOnWall = false;
        return false;
    }

    /// <summary>
    /// Checks if the player is holding the direction, hes facing in
    /// </summary>
    /// <returns> True if the player is holding in the direction, he is facing. False if he is not.</returns>
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

    /// <summary>
    /// Checks if there is a raycast of the given in parameters hitting an object with the right tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="rayArray"></param>
    /// <returns> True if there was any raycast hitting an object with the right tag. False if there was none.</returns>
    private bool RaycastForTag(string tag, params RaycastHit2D[] rayArray)
    {
        for (int i = 0; i < rayArray.Length; i++)
        {
            if (rayArray[i].collider != null)
            {
                if (rayArray[i].collider.tag == tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Check every raycast from the raycasts struct and return the first one, which found an object which matched the tag 
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="rayArray"></param>
    /// <returns> The first raycast who hit an object with the right tag</returns>
    private RaycastHit2D? WhichRaycastForTag(string tag, params RaycastHit2D[] rayArray)
    {
        for (int i = 0; i < rayArray.Length; i++)
        {
            if (rayArray[i].collider != null)
            {
                if (rayArray[i].collider.tag == tag)
                {
                    return rayArray[i];
                }
            }
        }
        return null;
    }

    #endregion

    #region Debugging Tools

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

    /// <summary>
    /// Checks if the Player is giving directional input to walk or not and turn him accordingly
    /// </summary>
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

    #region Healing

    /// <summary>
    /// If the Health is not up to its maximum, the player takes one health juice and heals himself, gaining one health point. The healCounter prevents the healing from taking place too fast
    /// </summary>
    private void Heal()
    {
        if(healCounter < healDuration)
        {
            healCounter++;
        }
        else
        {
            healCounter = 0;
            HealthJuice--;
            Health++;
        }
    }

    #endregion

    #region Attack

    /// <summary>
    /// Make the player attack, setting the direction of attack, hitbox and animation fields
    /// </summary>
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

    /// <summary>
    /// Check if an enemy is hit with the ray in the direction of the attack and damages him if so
    /// </summary>
    /// <param name="direction"></param>
    private void AttackHitboxOut(Vector2 direction)
    {
        hit = Physics2D.Raycast(transform.position, direction, attackReach);
        if (hit.collider && bAlreadyHit == false)
        {
            if(hit.collider.tag == "Enemy")
            {
                // Calculate the direction, the player has to knock the opponent away
                Vector3 knockDirection = hit.collider.gameObject.transform.position - transform.position;
                hit.collider.gameObject.GetComponent<TinyEnemy>().TakeDamage(attack, knockDirection.normalized * knockBackStrength); // TODO dont search for tiny enemy, but script, inheriting from GeneralEnemy
                bAlreadyHit = true;
            }
        }
    }

    /// <summary>
    /// End the attack and setting the right state, animation boolean and starting the cooldown
    /// </summary>
    private void EndAttack()
    {
        anim.SetBool("Attacking", false);
        playerState = State.freeToMove;
        bAlreadyHit = false;
        StartCoroutine(AttackCooldown());
    }

    /// <summary>
    /// Waits for the attack to be available again
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        bAttackable = true;
    }

    #endregion

    #region Dodge

    /// <summary>
    /// Set up the dodging process
    /// </summary>
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

    /// <summary>
    /// The actual application of force whilst dodging
    /// </summary>
    private void Dodge()
    {
        velocity += new Vector3(dodgePower * transform.localScale.x * speed * Time.deltaTime, appliedDodgeUpPower * Time.deltaTime);
        bDodgable = false;
    }

    /// <summary>
    /// End the Dodge process
    /// </summary>
    private void EndDodge()
    {
        anim.SetBool("Dodging", false);
        playerState = State.freeToMove;
        StartCoroutine(DodgeCooldown());
    }

    /// <summary>
    /// Wait for the dodge to be available again
    /// </summary>
    /// <returns></returns>
    IEnumerator DodgeCooldown()
    {
        yield return new WaitForSeconds(dodgeCooldown);
        bDodgable = true;
    }

    #endregion

    #region Jump

    /// <summary>
    /// Start the Jump process
    /// </summary>
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

    /// <summary>
    /// The application of the main jumping force
    /// </summary>
    private void Jump()
    {
        if(bGrounded)
        {
            velocity += new Vector3(0f, jumpPower * Time.deltaTime);
        }
    }

    #endregion

    #region Damage Calculation

    /// <summary>
    /// Damages the player and sets the knockback
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="knockBack"></param>
    public void TakeDamage(int damage, Vector3 knockBack)
    {
        if (playerState != State.attacking && playerState != State.dodging)
        {
            // Wait for the knockback to stop and giving the player free to move again
            StartCoroutine(UntilKnockBackStops(knockBackDuration));
            bKnockedBack = true;
            if (invincibilityCounter == 0)
            {
                Health -= damage;
                invincibilityCounter = invincibilityTime;
            }
            // Set the knockback force to be applied
            knockBackForce = knockBack;
        }
    }

    /// <summary>
    /// Wait until able to move freely again
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    IEnumerator UntilKnockBackStops(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        bKnockedBack = false;
    }

    #endregion

    #region Grounded

    /// <summary>
    /// Checks if the player is on the ground or not
    /// </summary>
    private void CheckGrounded()
    {
        // When the bottom left collider hit something tagged as ground
        if (RaycastForTag("Ground", raycasts.bottomLeft) || RaycastForTag("Ground", raycasts.bottomRight))
        {
            bGrounded = true;
            velocity.y = 0f;
            anim.SetBool("Grounded", true);
        }
        // Otherwise the player is not grounded
        else
        {
            bGrounded = false;
            anim.SetBool("Grounded", false);
        }
    }

    #endregion
}
