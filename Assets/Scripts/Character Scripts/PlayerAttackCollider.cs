using UnityEngine;

public class PlayerAttackCollider : MonoBehaviour
{
    // Components
    private Animator anim;
    private SphereCollider col;
    private GameObject player;
    private CharacterStats stats;

    private bool hit = false;

    // Hit event
    public delegate void HitEventHandler(object source, PublicEventArgs args);
    public event HitEventHandler HitEvent;

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
        if (anim.GetBool("Attacking"))
        {
            if (hit == false)
            {
                col.enabled = true;
            }
        }
        else
        {
            col.enabled = false;
            hit = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "character")
        {
            Debug.Log("Hit " + other.name);

            other.GetComponent<CharacterStats>().GetHit(GetComponent<Transform>().root.gameObject, stats.GetStat("damage"));
            other.GetComponent<Animator>().Play("Base Layer.Combat.Hit.Hit " + UnityEngine.Random.Range(0, 4));

            col.enabled = false;
            hit = true;

            OnHitEvent(other);
        }
    }

    protected virtual void OnHitEvent(Collider other)
    {
        PublicEventArgs args;
        if (HitEvent != null)
        {
            args = new PublicEventArgs(transform.root.gameObject, other.transform.root.gameObject, EventType.Hit, 50);
            HitEvent(this, args);
        }
    }
}
