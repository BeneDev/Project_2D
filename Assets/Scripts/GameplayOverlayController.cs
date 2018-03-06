using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayOverlayController : MonoBehaviour {

    private Text healthText;
    private GameObject player;

    private void Awake()
    {
        healthText = GetComponent<Text>();
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Sign up for changes on the players health
        player.GetComponent<CharController>().OnHealthChanged += UpdateLabel;
    }

    // Updates the health count overlay
    private void UpdateLabel(int newHealth)
    {
        healthText.text = "Health: " + newHealth;
    }
}
