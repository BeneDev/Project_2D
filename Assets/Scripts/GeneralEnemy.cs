using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralEnemy : MonoBehaviour {

    #region Fields

    // Enemy Attributes
    private int health = 20;
    private int attack = 2;
    private int defense = 2;

    // Variables to find the player
    private GameObject player;
    private Vector3 toPlayer;

    [SerializeField] float hitRange = 2f;
    [SerializeField] float knockBackStrength = 3f;

    #endregion

    void Start ()
    {
        GeneralInitialization();
    }

    void Update ()
    {
        GeneralBehavior();
    }

    #region Helper Functions

    public virtual void GeneralInitialization()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public virtual void GeneralBehavior()
    {
        // Store the vector towards the player
        toPlayer = player.transform.position - transform.position;

        // Attacks the player if he within reach
        if (toPlayer.magnitude <= hitRange)
        {
            player.GetComponent<CharController>().TakeDamage(attack, CalculateKnockback());
            // TODO play the right animation and maybe make dedicated attack for enemies
        }
        // Die if health is gone
        if (health <= 0)
        {
            Die();
        }
    }

    // TODO Play also right animation
    public virtual void Die()
    {
        Destroy(gameObject);
    }

    // Subtract damage and knockback to the enemy
    public virtual void TakeDamage(int damageToTake, Vector3 knockback)
    {
        health -= damageToTake;
        transform.position += knockback;
    }

    public Vector3 CalculateKnockback()
    {
        Vector3 knockBack = toPlayer.normalized * knockBackStrength;
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
