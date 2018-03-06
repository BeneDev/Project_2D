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
    }

    private void OnEnable()
    {
        player.GetComponent<CharController>().OnHealthChanged += UpdateLabel;
    }

    private void OnDisable()
    {
        player.GetComponent<CharController>().OnHealthChanged -= UpdateLabel;
    }

    private void UpdateLabel(int newHealth)
    {
        healthText.text = "Health: " + newHealth;
    }
}
