using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayOverlayController : MonoBehaviour {

    [SerializeField] Text healthText;
    [SerializeField] Text healthJuiceText;

    private GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Sign up for the players delegates
        player.GetComponent<CharController>().OnHealthChanged += UpdateHealthText;
        player.GetComponent<CharController>().OnHealthJuiceChanged += UpdateHealthJuiceText;
    }

    // Updates the health count overlay
    private void UpdateHealthText(int newHealth)
    {
        healthText.text = "Health: " + newHealth;
    }

    // Updates the health juice count overlay
    private void UpdateHealthJuiceText(int newHealthJuice)
    {
        healthJuiceText.text = "Health Juice: " + newHealthJuice;
    }
}
