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
            player.GetComponent<CharController>().TakeDamage(attack, new Vector3(toPlayer.normalized.x * knockBackStrength, toPlayer.normalized.y * knockBackStrength));
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

    // Subtract damage and knockback to the enemy
    public void TakeDamage(int damageToTake, Vector3 knockback)
    {
        health -= damageToTake;
        transform.position += knockback;
    }

    #endregion

}
