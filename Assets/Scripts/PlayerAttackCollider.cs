using UnityEngine;

public class PlayerAttackCollider : MonoBehaviour
{
    Animator anim;
    SphereCollider col;
    GameObject player;
    CharacterStats stats;

    bool hit = false;

    // Use this for initialization
    void Start()
    {
        col = GetComponent<SphereCollider>();

        player = transform.root.gameObject;
        anim = player.GetComponent<Animator>();
        stats = player.GetComponent<CharacterStats>();

        col.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.GetBool("Attacking") )
        {
            if (hit == false)
                col.enabled = true;
        }
        else
        {
            col.enabled = false;
            hit = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.tag != "Player")
        {
            Debug.Log("Hit " + other.name);
            other.GetComponent<CharacterStats>().GetHit(stats.damage);
            other.GetComponent<Animator>().Play("Base Layer.Combat.Hit.Hit " + Random.Range(0,4));
            col.enabled = false;
            hit = true;
        }
    }
}
