using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class CharController : MonoBehaviour {

    PlayerInput input;
    Rigidbody2D rb;
    bool bGrounded = false;
    
    [SerializeField] float speed = 1;
    [SerializeField] [Range(1f, 20f)] float jumpPower;

    // Use this for initialization
    void Start () {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += new Vector3(input.Horizontal * speed * Time.deltaTime, 0f);
        if(Physics2D.Raycast(transform.position, Vector2.down, 0.08f))
        {
            bGrounded = true;
        }
        else
        {
            bGrounded = false;
        }
        if (Input.GetKeyDown(KeyCode.Space) && bGrounded)
        {
            rb.velocity += new Vector2(0f, input.Jump * jumpPower);
        }
	}

    //private void OnDrawGízmos()
    //{
    //    Gizmos.DrawLine(transform.position, (transform.position + Vector3.down) * 0.08f);
    //}
}
