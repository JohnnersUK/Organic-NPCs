﻿using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // Stats
    /// Combat
    [Header("Health Settings:")]
    public float health;
    public float maxHealth;


    [Header("Stamina Settings:")]
    public float stamina;
    public float maxStamina;
    public float staminaGain;
    public float maxRechargeDelay;
    public float rechargeDelay = 0;

    public float damage = 10.0f;
    public float range = 1.5f;

    // Needs
    [Header("Needs:")]
    public float fatigue = 100.0f;
    public float hunger = 100.0f;
    public float boredom = 100.0f;
    public float social = 100.0f;

    // Emotions
    [Header("Emotions:")]
    public float happiness = 0.0f;
    public float anger = 0.0f;
    public List<float> hModifiers;

    [Header("Debug:")]
    public bool randomize = false;
    public bool reset = false;

    [Space(10, order = 0)]
    public float debug = 0.0f;
    

    [SerializeField] private float iframes = 30;
    [SerializeField] private float icount;
    [SerializeField] private bool invincible = false;

    public float rTime;
    public float rCount = 0;

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
        {"Anger", 0.0f},

        // Debug
        {"random", 0.0f},
        {"debug", 0.0f }
    };

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

    public void GetHit(float dmg)
    {
        if (!invincible)
        {
            health -= dmg;
            stamina -= dmg / 2;
            invincible = true;

            hModifiers.Add(-0.5f);
            anger += 10;
        }
    }

    /// <summary>
    /// Returns a list of stats
    /// </summary>
    /// <param name="Strings">Array of names of the stats you want returned</param>
    /// <returns></returns>
    public float[] GetStats(string[] Strings)
    {
        float[] stats = new float[Strings.Length];

        for (int i = 0; i < Strings.Length; i++)
        {
            stats[i] = table[Strings[i]];
        }

        return stats;
    }

    /// <summary>
    /// Returns a single stat
    /// </summary>
    /// <param name="stat">The name of the stat you want to set</param>
    /// <returns></returns>
    public float GetStat(string stat)
    {
        return table[stat];
    }

    /// <summary>
    /// Sets a single stat
    /// </summary>
    /// <param name="stat">The name of the stat you want to set</param>
    /// <param name="value">The value to set it as</param>
    public void SetStat(string stat, float value)
    {
        table[stat] = value;
        return;
    }

    // Updates the stats
    private void UpdateStats()
    {
        // Stats
        // Decrease the stats over time and clap them between 0 and 100
        hunger = Mathf.Clamp(hunger - 1 * Time.deltaTime, 0.0f, 100.0f);
        if (hunger == 0)    // If hunger is equal to zero, lose health
        {
            health -= 1 * Time.deltaTime;
        }

        boredom = Mathf.Clamp(boredom - 0.5f * Time.deltaTime, 0.0f, 100.0f);
        social = Mathf.Clamp(social - 1 * Time.deltaTime, 0.0f, 100.0f);
        fatigue = Mathf.Clamp(fatigue - 0.1f * Time.deltaTime, 0.0f, 100.0f);

        happiness = CalculateHappiness();
        anger = ((anger <= 0) ? 0 : anger - Time.deltaTime * 0.1f);

        // Stamina
        if (rechargeDelay > 0)
        {
            rechargeDelay -= Time.deltaTime;
            if (rechargeDelay <= 0)
            {
                rechargeDelay = 0;

                if (stamina <= 0)
                {
                    stamina = 1;
                    table["stamina"] = 1;
                }

            }
        }

        // If stamina hasn't been modified
        if (stamina == table["stamina"])
        {
            // Recharge stamina
            if (rechargeDelay == 0)
                stamina = ((stamina < maxStamina) ? stamina + staminaGain * Time.deltaTime : maxStamina);
        }
        else // If stamina has been modified
        {
            if (stamina > 0)
                rechargeDelay = maxRechargeDelay / 4;
            else
                rechargeDelay = maxRechargeDelay;
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

        table["happiness"] = happiness;
        table["anger"] = anger;
    }

    // Calculates the bots happiness
    public float CalculateHappiness()
    {
        float hSine;
        float hAsPi;
        float total;

        // Get the stats as a percentage between -1 and 1
        float s = (social / 50) - 1;
        float b = (boredom / 50) - 1;
        float f = (fatigue / 50) - 1;

        // Calculate the hunger multiplier as a Sine wave
        // Where a = 2, h = 0.5, b = 1 and k = -1
        hAsPi = Mathf.PI * (hunger / 100);
        hSine = 2 * Mathf.Sin(hAsPi - 0.5f) - 1;

        total = s + b + f + hSine;

        foreach(float m in hModifiers)
        {
            total -= m;
        }

        return total;
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
