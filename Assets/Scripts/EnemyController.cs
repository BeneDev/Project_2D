using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    // Enemy Attributes
    private int health = 20;
    private int attack = 2;
    private int defense = 2;

    // Variables to find the player
    private GameObject player;
    private Vector3 toPlayer;

    [SerializeField] float hitRange = 2f;
    [SerializeField] float knockBackStrength = 3f;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
        toPlayer = player.transform.position - transform.position;
        if(toPlayer.magnitude <= hitRange)
        {
            player.GetComponent<CharController>().TakeDamage(attack, new Vector3(toPlayer.normalized.x * knockBackStrength, knockBackStrength / 50));
        }
		if(health <= 0)
        {
            Destroy(gameObject);
        }
	}

    public void TakeDamage(int damageToTake)
    {
        health -= damageToTake;
        print("new health = " + health.ToString());
    }
    
}
