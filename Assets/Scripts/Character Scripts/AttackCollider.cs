using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    [SerializeField] private int[] attackValues = null;
    [SerializeField] private Animator anim = null;
    [SerializeField] private CharacterStats stats = null;
    [SerializeField] private GameObject player;
    private SphereCollider col;

    // Hit event
    public delegate void HitEventHandler(object source, PublicEventArgs args);
    public event HitEventHandler HitEvent;

    private void Start()
    {
        col = GetComponent<SphereCollider>();

        // Dissable the collider by default
        col.enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void Update()
    {
        bool matchingAV = false;
        int size = attackValues.Length;

        // Check if attack values match
        for (int i = 0; i < size; i++)
        {
            if (anim.GetInteger("AttackType") == attackValues[i])
            {
                matchingAV = true;
            }
        }

        // Enable the attack collider if attacking
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
        // Iff other is a bot or player, damage and send hit event
        if ((other.tag == "character" || other.tag == "Player") && other.name != GetComponent<Collider>().transform.root.name)
        {
            other.GetComponent<CharacterStats>().GetHit(GetComponent<Transform>().root.gameObject, stats.table["damage"]);
            OnHitEvent(other);
        }
    }

    // Dispatch hit event
    protected virtual void OnHitEvent(Collider other)
    {
        PublicEventArgs args;

        if (HitEvent != null)
        {
            args = new PublicEventArgs(GetComponent<Collider>().transform.root.gameObject, other.transform.root.gameObject, EventType.Hit, 50);
            HitEvent(this, args);
        }
    }
}
