using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumberController : MonoBehaviour {

    [SerializeField] Text damageNumberText;

    [SerializeField] float upwardsSpeed = 0.1f;
    [SerializeField] float secondsToLast = 1f;

    private GameObject objectToFollow;

    private void Awake()
    {
    }

    public void Init(string number, GameObject obj)
    {
        damageNumberText.text = number;
        objectToFollow = obj;
    }

    private void Update()
    {
        StartCoroutine(WaitForEnd());
        transform.position += new Vector3(0f, upwardsSpeed);
        if (objectToFollow)
        {
            transform.position = new Vector3(objectToFollow.transform.position.x, transform.position.y);
        }
        damageNumberText.transform.localScale -= new Vector3(damageNumberText.transform.localScale.x/30, damageNumberText.transform.localScale.y / 30);
        // TODO Make text fade out
    }

    IEnumerator WaitForEnd()
    {
        yield return new WaitForSeconds(secondsToLast);
        Destroy(gameObject);
    }
}
