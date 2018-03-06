using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    private int health = 20;
    public int attack = 2;
    private int defense = 2;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
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
