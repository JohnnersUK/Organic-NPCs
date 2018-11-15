using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{

    // Stats
    public float health = 100;
    public float stamina = 100;

    public float damage = 10;

    [SerializeField] int iframes;
    int icount;
    bool invincible = false;

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
    }

    public void GetHit(float dmg)
    {
        if (!invincible)
        {
            health -= dmg;
            stamina -= dmg / 2;

            GetComponent<Animator>().SetInteger("HitType", Random.Range(0, 5));
            invincible = true;
            Debug.Log("Hit:" + this.name);
        }
    }
}
