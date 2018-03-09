using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This Script controls the general Panel for showing the gameplay UI
/// </summary>
public class GameplayOverlayController : MonoBehaviour {

    #region Fields
    [SerializeField] Text healthText;
    [SerializeField] Text healthJuiceText;

    private GameObject player;
    #endregion

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Sign up for the players delegates
        player.GetComponent<CharController>().OnHealthChanged += UpdateHealthText;
        player.GetComponent<CharController>().OnHealthJuiceChanged += UpdateHealthJuiceText;
    }

    /// <summary>
    /// Updates the health count overlay
    /// </summary>
    /// <param name="newHealth"></param>
    private void UpdateHealthText(int newHealth)
    {
        healthText.text = "Health: " + newHealth;
    }

    /// <summary>
    /// Updates the health juice count overlay
    /// </summary>
    /// <param name="newHealthJuice"></param>
    private void UpdateHealthJuiceText(int newHealthJuice)
    {
        healthJuiceText.text = "Health Juice: " + newHealthJuice;
    }
}
