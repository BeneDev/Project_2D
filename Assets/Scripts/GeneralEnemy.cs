using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Script of which all enemies inherit, providing them with general methods and attributes, necessary for an enemy to work properly
/// </summary>
public class GeneralEnemy : MonoBehaviour {

    #region Properties

    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            if(damageNumber)
            {
                DamageNumberController number = Instantiate(damageNumber, transform.position, transform.rotation);
                number.Init((health - value).ToString(), gameObject);
            }
            health = value;
        }
    }

    #endregion

    #region Fields

    // Enemy Attributes
    [SerializeField] protected int health = 20;
    [SerializeField] protected int attack = 2;
    [SerializeField] protected int defense = 2;

    // The amount of particles getting instantiated when the enemy dies
    [SerializeField] int particleCountAtDeath = 1;
    // The particle getting instantiated when the enemy dies
    [SerializeField] GameObject juiceParticle;
    // The offset a Juice Particle can have 
    [SerializeField] float spawnOffset = 0.3f;

    // The canvas used to show damage numbers
    [SerializeField] DamageNumberController damageNumber;

    // Variables to find the player
    private GameObject player;
    private Vector3 toPlayer;

    Rigidbody2D rb;

    [SerializeField] float hitRange = 2f;
    [SerializeField] float knockBackStrength = 3f;

    // The layer mask used to collide with only walls
    [SerializeField] LayerMask layersToCollideWith;

    #endregion

    /// <summary>
    /// The general things an enemy should do in his start or awake method
    /// </summary>
    public virtual void GeneralInitialization()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// The general things an enemy should do in his update method
    /// </summary>
    public virtual void GeneralBehavior()
    {
        // Store the vector towards the player
        toPlayer = player.transform.position - transform.position;

        // Attacks the player if he within reach
        if (toPlayer.magnitude <= hitRange)
        {
            player.GetComponent<CharController>().TakeDamage(attack, CalculateKnockback());
        }
    }

    #region Helper Functions

    /// <summary>
    /// This makes the enemy disappear and spawn juiceParticles for the player to fill up his Health Juice again
    /// </summary>
    public virtual void Die()
    {
        if (juiceParticle)
        {
            for (int i = 0; i < particleCountAtDeath; i++)
            {
                Instantiate(juiceParticle, transform.position, transform.rotation);
            }
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Subtract damage and apply knockback to the enemy
    /// </summary>
    /// <param name="damageToTake"></param>
    /// <param name="knockback"></param>
    public virtual void TakeDamage(int damageToTake, Vector3 knockback)
    {
        // When the damage is greater than defense, do that remaining damage
        if (damageToTake - defense > 0)
        {
            Health -= damageToTake - defense;
        }
        // Otherwise deal just one damage
        else
        {
            Health--;
        }
        if (Health > 0)
        {
            // Dont let the enemy goes through collider when knockback is applied
            if(!Physics2D.Raycast(transform.position, knockback, knockback.magnitude, layersToCollideWith))
            {
                transform.position += knockback;
            }
            else
            {
                // Get Knocked back onto the wall
                while(!Physics2D.Raycast(transform.position, knockback, knockback.magnitude / 10, layersToCollideWith))
                {
                    transform.position += knockback / 10;
                }
                Health -= damageToTake;
                if(Health <= 0)
                {
                    Die();
                }
            }
        }
        else
        {
            Die();
        }
    }

    /// <summary>
    /// Calculated the force which has to be given into the Take Damage function of the player, to cause the knockback for the player
    /// </summary>
    /// <returns></returns>
    public Vector3 CalculateKnockback()
    {
        // Sets normal knockback
        Vector3 knockBack = toPlayer.normalized * knockBackStrength;
        // When the player is too close on the x-axis(most of the times because the player is right on top of the enemy) make the knockback stronger
        if(GetOnlyValue(toPlayer.x) <= 0.065f)
        {
            if(toPlayer.x > 0)
            {
                toPlayer.x = 0.1f;
            }
            else
            {
                toPlayer.x = -0.1f;
            }
        }
        return knockBack;
    }

    /// <summary>
    /// Returns only the value of a number given in. This gets rid of the algebraic sign before a number
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public float GetOnlyValue(float number)
    {
        if(number > 0)
        {
            return number;
        }
        else if(number < 0)
        {
            return -number;
        }
        else
        {
            return 0;
        }
    }

    #endregion

}
