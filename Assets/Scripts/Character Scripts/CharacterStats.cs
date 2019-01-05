using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // Stats
    public float health = 100.0f;
    public float stamina = 100.0f;
    public float damage = 10.0f;
    public float range = 1.5f;
    public float debug = 0.0f;

    public bool randomize = false;
    public bool reset = false;

    public IDictionary<string, float> table = new Dictionary<string, float>()
    {
        {"health", 100.0f},
        {"stamina", 100.0f},
        {"damage", 10.0f },
        {"range", 1.5f },
        {"random", 0.0f},
        {"debug", 0.0f }
    };

    [SerializeField] private int iframes = 30;
    private int icount;
    private bool invincible = false;

    // Use this for initialization
    void Start()
    {
        icount = iframes;
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

        // Update stats table;
        table["health"] = health;
        table["stamina"] = stamina;
        table["damage"] = damage;
        table["range"] = range;
        table["random"] = Random.Range(-100f, 100f);
        table["debug"] = debug;

        // Debug button to randomize stats
        if (randomize == true)
        {
            health = Random.Range(-100f, 100f);
            stamina = Random.Range(-100f, 100f);
            damage = Random.Range(-100f, 100f);
            debug = Random.Range(-100f, 100f);
            randomize = false;
        }

        // Debug button to reset stats
        if (reset == true)
        {
            health = 100.0f;
            stamina = 100.0f;
            damage = 10.0f;
            range = 1.5f;
            debug = 0.0f;
            reset = false;

            GetComponent<Animator>().SetInteger("Dodge", 0);
        }
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

    public float[] GetStats(string[] Strings)
    {
        float[] stats = new float[Strings.Length];

        for (int i = 0; i < Strings.Length; i++)
        {
            stats[i] = table[Strings[i]];
        }

        return stats;
    }
}
