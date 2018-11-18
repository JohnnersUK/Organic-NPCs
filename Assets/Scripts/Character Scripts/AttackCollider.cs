using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour {

    [SerializeField] int[] attackValues;
    [SerializeField] Animator anim;
    [SerializeField] CharacterStats stats;
    [SerializeField] GameObject player;
    SphereCollider col;

	// Use this for initialization
	void Start ()
    {
        col = GetComponent<SphereCollider>();

        col.enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        bool matchingAV = false;
        int size = attackValues.Length;

        for (int i = 0; i < size; i++)
        {
            if (anim.GetInteger("AttackType") == attackValues[i])
            {
                matchingAV = true;
            }
        }

		if (anim.GetBool("Attacking") && matchingAV)
        {
            col.enabled = true;
        }
        else
        {
            col.enabled = false;
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "character" && (other.name != player.name))
        {
            other.GetComponent<CharacterStats>().GetHit(stats.table["damage"]);
            col.enabled = false;
        }
    }
}
