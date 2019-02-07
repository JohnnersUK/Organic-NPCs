using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // Stats
    /// Combat
    public float health = 100.0f;
    public float stamina = 100.0f;
    public float damage = 10.0f;
    public float range = 1.5f;

    // Needs
    public float fatigue = 100.0f;
    public float hunger = 100.0f;
    public float boredom = 100.0f;
    public float social = 100.0f;
    public float fitnessMultiplier = 1.0f;

    public float debug = 0.0f;

    public IDictionary<string, float> table = new Dictionary<string, float>()
    {
        // Combat
        {"health", 100.0f},
        {"stamina", 100.0f},
        {"damage", 10.0f },
        {"range", 1.5f },

        // Needs
        {"fatigue", 100.0f},
        {"hunger", 100.0f},
        {"boredom", 100.0f },
        {"social", 100.0f },
        {"fitnessMultiplier", 1.0f },

        // Debug
        {"random", 0.0f},
        {"debug", 0.0f }
    };

    public bool randomize = false;
    public bool reset = false;

    public float StaminaGain;
    public float RechargeDelay;

    [SerializeField] private int iframes = 30;
    private int icount;
    private bool invincible = false;

    public float rTime;
    public float rCount = 0;


    // Use this for initialization
    void Start()
    {
        icount = iframes;
        rTime = Random.Range(10, 30);
    }

    // Update is called once per frame
    void Update()
    {
        if (invincible)
        {
            icount -= 1;
            if (icount < 1)
            {
                invincible = false;
                icount = iframes;
            }
        }

        if (stamina < 100)
        {
            stamina += Time.deltaTime * StaminaGain;
        }

        // Update stats table;
        UpdateStats();
    }

    public void GetHit(float dmg)
    {
        if (!invincible)
        {
            table["health"] -= dmg;
            //Debug.Log(table["health"]);
            table["stamina"] -= dmg / 2;

            GetComponent<Animator>().SetInteger("HitType", Random.Range(0, 5));
            invincible = true;
        }
    }

    // Gets a list of stats
    public float[] GetStats(string[] Strings)
    {
        float[] stats = new float[Strings.Length];

        for (int i = 0; i < Strings.Length; i++)
        {
            stats[i] = table[Strings[i]];
        }

        return stats;
    }

    // Gets a single stat
    public float GetStat(string stat)
    {
        return table[stat];
    }

    public void SetStat(string stat, float value)
    {
        table[stat] = value;

        // If the stat being changed is stamina, 
        // and the value is less than 0
        if (stat == "stamina" && value < 0)
        {
            table["stamina"] = 0 - RechargeDelay; // Apply a stamina delay
        }
        return;
    }

    // Updates the table of stats
    private void UpdateStats()
    {
        hunger -= 1 * Time.deltaTime;
        if (hunger <= 0)
        {
            hunger = 0;
            health -= 1 * Time.deltaTime;
        }

        boredom -= 1 * Time.deltaTime;
        if (boredom <= 0)
        {
            boredom = 0;
        }

        social -= 1 * Time.deltaTime;
        if (social <= 0)
        {
            social = 0;
        }

        fatigue -= 0.1f * Time.deltaTime;
        if (fatigue <= 0)
        {
            fatigue = 0;
        }


        table["health"] = health;
        table["stamina"] = stamina;
        table["damage"] = damage;
        table["range"] = range;
        table["random"] = Random.Range(-100f, 100f);
        table["debug"] = debug;

        table["fatigue"] = fatigue;
        table["hunger"] = hunger;
        table["social"] = social;
        table["boredom"] = boredom;

        // Get fitness multiplier
        fitnessMultiplier =
            (table["hunger"] +
            table["social"] +
            table["boredom"] +
            table["hunger"]) / 100;
        table["fitnessMultiplier"] = fitnessMultiplier;
    }

    // Randomizes the stats
    public void Randomize()
    {
        health = Random.Range(0.0f, 100f);
        stamina = Random.Range(0.0f, 100f);
        damage = Random.Range(0.0f, 100f);

        debug = Random.Range(0.0f, 100f);

        fatigue = Random.Range(0.0f, 100f);
        hunger = Random.Range(0.0f, 100f);
        boredom = Random.Range(0.0f, 100f);
        social = Random.Range(0.0f, 100f);

        randomize = false;
    }
}
