using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuiceParticleController : MonoBehaviour {

    [SerializeField] LayerMask layersToCollide;
    [SerializeField] float upwardsVelocity = 0.1f;
    [SerializeField] float maxHorizontalVelocity = 0.03f;
    [SerializeField] float maxSecondsFlyingUp = 2f;

    private float actualHorizontalVelocity = 0f;
    private float actualSecondsFlyingUp = 0f;

	// Use this for initialization
	void Awake () {
        actualHorizontalVelocity = Random.Range(-maxHorizontalVelocity, maxHorizontalVelocity);
        actualSecondsFlyingUp = Random.Range(0.5f, maxSecondsFlyingUp);
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 direction = new Vector2(actualHorizontalVelocity, upwardsVelocity);
        if (actualSecondsFlyingUp > 0f && !Physics2D.Raycast(transform.position, direction, direction.magnitude, layersToCollide))
        {
            transform.position += (Vector3)direction;
            actualSecondsFlyingUp -= Time.deltaTime;
        }
	}
}
