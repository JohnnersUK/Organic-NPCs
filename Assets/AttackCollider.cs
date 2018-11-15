using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour {

    [SerializeField] Animator anim;
    SphereCollider col;

	// Use this for initialization
	void Start ()
    {
        GetComponent<MeshRenderer>().enabled = false;
        col = GetComponent<SphereCollider>();

        col.enabled = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (anim.GetBool("Atacking"))
        {
            col.enabled = true;
        }
        else
        {
            col.enabled = false;
        }
	}
}
