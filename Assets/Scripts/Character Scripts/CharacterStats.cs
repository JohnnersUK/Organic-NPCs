using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{

    // Stats
    public IDictionary<string, float> table = new Dictionary<string, float>()
    {
        {"health", 100.0f},
        {"stamina", 100.0f},
        {"damage", 10.0f },
        {"range", 0.5f },
        {"random", 0.0f}
    };

    [SerializeField] private int iframes = 30;
    private int icount;
    private bool invincible = false;

    // Use this for initialization
    void Start ()
    {
        icount = iframes;
    }
	
	// Update is called once per frame
	void Update ()
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

        table["random"] = Random.Range(-100f, 100f);
    }

    public void GetHit(float dmg)
    {
        if (!invincible)
        {
            table["health"] -= dmg;
            Debug.Log(table["health"]);
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
