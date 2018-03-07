using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

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

    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	void Update () {
        // Store the vector towards the player
        toPlayer = player.transform.position - transform.position;

        // Attacks the player if he within reach
        if(toPlayer.magnitude <= hitRange)
        {
            player.GetComponent<CharController>().TakeDamage(attack, CalculateKnockback());
        }
        // Die if health is gone
		if(health <= 0)
        {
            Die();
        }
	}

    #region Helper Functions

    // TODO Play also right animation
    private void Die()
    {
        Destroy(gameObject);
    }

    private Vector3 CalculateKnockback()
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

    private float GetOnlyValue(float number)
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

    // Subtract damage and knockback to the enemy
    public void TakeDamage(int damageToTake, Vector3 knockback)
    {
        health -= damageToTake;
        transform.position += knockback;
    }

    #endregion

}
