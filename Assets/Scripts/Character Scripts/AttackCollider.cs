using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AttackCollider : MonoBehaviour {

    [SerializeField] int[] attackValues;
    [SerializeField] Animator anim;
    [SerializeField] CharacterStats stats;
    [SerializeField] GameObject player;
    SphereCollider col;

    public delegate void HitEventHandler(object source, PublicEventArgs args);
    public event HitEventHandler HitEvent;

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
        if ((other.tag == "character" || other.tag == "Player") && other.name != GetComponent<Collider>().transform.root.name)
        {
            other.GetComponent<CharacterStats>().GetHit(GetComponent<Transform>().root.gameObject, stats.table["damage"]);
            OnHitEvent(other);
        }
    }

    protected virtual void OnHitEvent(Collider other)
    {
        if (HitEvent != null)
        {
            PublicEventArgs args = new PublicEventArgs(GetComponent<Collider>().transform.root.gameObject, other.transform.root.gameObject, EventType.Hit, 50);
            HitEvent(this, args);
        }
    }
}
