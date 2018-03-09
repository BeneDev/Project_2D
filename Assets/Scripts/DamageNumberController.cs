using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The Script, controlling the canvas which holds the text, showing the damage number over the enemies
/// </summary>
public class DamageNumberController : MonoBehaviour {

    #region Fields
    [SerializeField] Text damageNumberText;

    [SerializeField] float upwardsSpeed = 0.1f; // How fast the text flys up
    [SerializeField] float secondsToLast = 1f; // How long the text lasts on screen

    private GameObject objectToFollow; // The object to follow regarding the x-axis
    #endregion

    /// <summary>
    /// Setting the object to follow on the x-axis and the number to show
    /// </summary>
    /// <param name="number"></param>
    /// <param name="obj"></param>
    public void Init(string number, GameObject obj)
    {
        damageNumberText.text = number;
        objectToFollow = obj;
    }

    private void Update()
    {
        // Start the counter to destroy the object
        StartCoroutine(WaitForEnd());
        // Make the canvas fly upwards
        transform.position += new Vector3(0f, upwardsSpeed);
        // Follow the given object on the x-axis
        if (objectToFollow)
        {
            transform.position = new Vector3(objectToFollow.transform.position.x, transform.position.y);
        }
        // Make the text shrink every frame
        damageNumberText.transform.localScale -= new Vector3(damageNumberText.transform.localScale.x * Time.deltaTime, damageNumberText.transform.localScale.y * Time.deltaTime);
        // TODO Make text fade out
    }

    /// <summary>
    /// Waiting a set amount of time before destroying itself
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForEnd()
    {
        yield return new WaitForSeconds(secondsToLast);
        Destroy(gameObject);
    }
}
