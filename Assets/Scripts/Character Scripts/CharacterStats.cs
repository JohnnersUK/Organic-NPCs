// Dissable assigned but not used warning
// Stats are there for debug viewing in the inspector
// as unity dosen't show dictionaries in the inspector
#pragma warning disable 0414

using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // Stats for inspector
    /// Combat
    [Header("Health Settings:")]
    [SerializeField] private float health;
    public float maxHealth = 100;

    [Header("Stamina Settings:")]
    [SerializeField] private float stamina;
    public float maxStamina = 10;
    public float staminaGain = 10;
    public float maxRechargeDelay = 1;
    [SerializeField] private float rechargeDelay = 0;

    [SerializeField] private float damage = 10.0f;
    [SerializeField] private float range = 1.5f;

    // Needs
    [Header("Needs:")]
    [SerializeField] private float fatigue = 100.0f;
    [SerializeField] private float hunger = 100.0f;
    [SerializeField] private float boredom = 100.0f;
    [SerializeField] private float social = 100.0f;

    // Emotions
    [Header("Emotions:")]
    [SerializeField] private float happiness = 0.0f;
    [SerializeField] private float anger = 0.0f;
    [SerializeField] private float fear = 0.0f;
    [SerializeField] private List<float> hModifiers;

    // Faction
    [Header("Faction:")]
    public Factions faction;

    // Invincibility settings
    [Header("invincibility:")]
    [SerializeField] private float iframes = 0.2f;
    [SerializeField] private float icount;
    [SerializeField] private bool invincible = false;

    // Debug
    [Header("Debug:")]

    [Space(10, order = 0)]
    [SerializeField] private float debug = 0.0f;
    [SerializeField] private float rTime;

    // A dictionary of the character stats, for easy lookup and access
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

        // Emotions
        {"happiness", 0.0f },
        {"anger", 0.0f},
        {"fear", 0.0f},

        // Debug
        {"random", 0.0f},
        {"debug", 0.0f }
    };

    // A dictionary of all entities this bot has ever interacted with
    // Float is a rating of their relationship between 0 and 100
    // Not in use, see faction system
    public IDictionary<GameObject, float> relationships = new Dictionary<GameObject, float>();

    // A dictionary of all factions and the robots relationship with them
    // Float is a rating of their relationship between 0 and 100
    public IDictionary<Factions, float> factionRelationships = new Dictionary<Factions, float>()
    {
        {Factions.Barbarians, 50.0f},
        {Factions.Guards, 50.0f},
        {Factions.Neutral, 50.0f}
    };

    [SerializeField] private ParticleSystem happy = null;
    [SerializeField] private ParticleSystem sad = null;

    // Use this for initialization
    void Start()
    {
        hModifiers = new List<float>();

        icount = iframes;
        rTime = Random.Range(10, 30);

        health = maxHealth;
        stamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (invincible)
        {
            icount -= 1 * Time.deltaTime;
            if (icount <= 0)
            {
                invincible = false;
                icount = iframes;
            }
        }

        // Update stats table;
        UpdateStats();
    }

    public void GetHit(GameObject agent, float dmg)
    {
        if (!invincible)
        {
            table["health"] -= dmg;
            invincible = true;
            icount = iframes;
        }

        DecreaseRelationship(agent, 10.0f);
        hModifiers.Add(-0.5f);

        if (GetComponent<CombatController>() != null)
        {
            if (faction == Factions.Guards && agent.GetComponent<CharacterStats>().faction == Factions.Guards)
            {
                table["anger"] += 10;
            }
            else
            {
                GetComponent<CombatController>().SetTarget(agent);
                table["anger"] += 10;
            }
        }
    }

    // Returns a list of stats
    public float[] GetStats(string[] Strings)
    {
        float[] stats = new float[Strings.Length];

        for (int i = 0; i < Strings.Length; i++)
        {
            stats[i] = table[Strings[i]];
        }

        return stats;
    }

    // Returns a single stat
    public float GetStat(string stat)
    {
        return table[stat];
    }

    // Sets a single stat
    public void ModifyStat(string stat, float value)
    {
        table[stat] += value;
    }

    // Updates the stats
    private void UpdateStats()
    {
        // Stats
        // Decrease the stats over time and clap them between 0 and 100
        table["hunger"] = Mathf.Clamp(table["hunger"] - 1 * Time.deltaTime, 0.0f, 100.0f);
        if (table["hunger"] == 0)    // If hunger is equal to zero, lose health
        {
            //health -= 1 * Time.deltaTime;
            // Dissabled from gameplay testing feedback
        }

        table["boredom"] = Mathf.Clamp(table["boredom"] - 0.5f * Time.deltaTime, 0.0f, 100.0f);
        table["social"] = Mathf.Clamp(table["social"] - 1 * Time.deltaTime, 0.0f, 100.0f);
        table["fatigue"] = Mathf.Clamp(table["fatigue"] - 0.1f * Time.deltaTime, 0.0f, 100.0f);

        table["happiness"] = CalculateHappiness();
        table["anger"] = ((table["anger"] <= 0) ? 0 : table["anger"] - Time.deltaTime * 0.1f);
        table["fear"] = ((table["fear"] <= 0) ? 0 : table["fear"] - Time.deltaTime * 0.1f);

        // Stamina
        if (rechargeDelay > 0)
        {
            rechargeDelay -= Time.deltaTime;
            if (rechargeDelay <= 0)
            {
                rechargeDelay = 0;

                if (table["stamina"] <= 0)
                {
                    table["stamina"] = 1;
                }

            }
        }

        // If stamina hasn't been modified
        if (stamina == table["stamina"])
        {
            // Recharge stamina
            if (rechargeDelay == 0)
            {
                table["stamina"] = ((table["stamina"] < maxStamina) ? table["stamina"] + staminaGain * Time.deltaTime : maxStamina);
            }
        }
        else // If stamina has been modified
        {
            if (table["stamina"] > 0)
            {
                rechargeDelay = maxRechargeDelay / 4;
            }
            else
            {
                rechargeDelay = maxRechargeDelay;
            }
        }

        health = table["health"];
        stamina = table["stamina"];
        damage = table["damage"];
        range = table["range"];
        table["random"] = Random.Range(-100f, 100f);
        debug = table["debug"];

        fatigue = table["fatigue"];
        hunger = table["hunger"];
        social = table["social"];
        boredom = table["boredom"];

        happiness = table["happiness"];
        anger = table["anger"];
        fear = table["fear"];
    }

    // Calculates the bots happiness
    public float CalculateHappiness()
    {
        float hSine;
        float hAsPi;
        float total;

        // Get the stats as a percentage between -1 and 1
        float s = (table["social"] / 50) - 1;
        float b = (table["boredom"] / 50) - 1;
        float f = (table["fatigue"] / 50) - 1;

        // Calculate the hunger multiplier as a Sine wave
        // Where a = 2, h = 0.5, b = 1 and k = -1
        hAsPi = Mathf.PI * (table["hunger"] / 100);
        hSine = 2 * Mathf.Sin(hAsPi - 0.5f) - 1;

        total = s + b + f + hSine;

        foreach (float m in hModifiers)
        {
            total -= m;
        }

        return total;
    }

    // Randomizes the stats
    public void Randomize()
    {
        table["health"] = Random.Range(0.0f, 100f);
        table["stamina"] = Random.Range(0.0f, 100f);
        table["damage"] = Random.Range(0.0f, 100f);

        table["debug"] = Random.Range(0.0f, 100f);

        table["fatigue"] = Random.Range(0.0f, 100f);
        table["hunger"] = Random.Range(0.0f, 100f);
        table["boredom"] = Random.Range(0.0f, 100f);
        table["social"] = Random.Range(0.0f, 100f);

        table["anger"] = Random.Range(-10.0f, 10.0f);
        table["fear"] = Random.Range(0.0f, 0.0f);

        UpdateStats();
    }

    public void IncreaseRelationship(GameObject agent, float value)
    {
        if (relationships.ContainsKey(agent))
        {
            relationships[agent] = Mathf.Clamp(relationships[agent] + value, 0, 100);
        }
        else
        {
            relationships.Add(agent, 50 + value);
        }
        happy.Play();
    }

    public void DecreaseRelationship(GameObject agent, float value)
    {
        if (relationships.ContainsKey(agent))
        {
            relationships[agent] = Mathf.Clamp(relationships[agent] - value, 0, 100);
        }
        else
        {
            relationships.Add(agent, 50 - value);
        }
        sad.Play();
    }
}
