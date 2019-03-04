using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    public GameObject Player;
    CharacterStats PlayerStats;

    public Image Health;
    public Image Stamina;

	// Use this for initialization
	void Start ()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        if (Player != null)
            PlayerStats = Player.GetComponent<CharacterStats>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            if (Player != null)
                PlayerStats = Player.GetComponent<CharacterStats>();
        }
        else
        {
            Health.fillAmount = PlayerStats.health / PlayerStats.maxHealth;
            Stamina.fillAmount = PlayerStats.stamina / PlayerStats.maxStamina;
        }

    }
}
