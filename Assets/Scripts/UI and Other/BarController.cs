
using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    private CharacterStats PlayerStats;

    [SerializeField] private Image Health = null;
    [SerializeField] private Image Stamina = null;

    // Use this for initialization
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        if (Player != null)
        {
            PlayerStats = Player.GetComponent<CharacterStats>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            if (Player != null)
            {
                PlayerStats = Player.GetComponent<CharacterStats>();
            }
        }
        else
        {
            Health.fillAmount = PlayerStats.GetStat("health") / PlayerStats.maxHealth;
            Stamina.fillAmount = PlayerStats.GetStat("stamina") / PlayerStats.maxStamina;
        }
    }
}
