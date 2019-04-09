using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CombatController : AiBehaviour
{
    // Stamina costs
    [SerializeField] private float AttackCost = 1;
    [SerializeField] private float DodgeCost = 1;

    // Components
    private Animator Anim;
    private NavMeshAgent AIAgent;
    private GameObject CombatTarget;
    private NeedsController NController;

    public override void Start()
    {
        // Set the filePath and init the network
        FilePath = Path.Combine(Application.streamingAssetsPath, "CombatNetwork.nn");
        base.Start();

        // Get components
        Anim = GetComponent<Animator>();
        AIAgent = GetComponent<NavMeshAgent>();
        Stats = GetComponent<CharacterStats>();

        NController = GetComponent<NeedsController>();
    }

    public override void Run()
    {
        if (!Anim.GetBool("InCombat"))
        {
            Anim.SetBool("InCombat", true);
            Anim.Play("Base Layer.Grounded");

            // If the bot was using something before combat
            NController.ClearQueue();
        }
            
        // If there isn't a target find one
        while (CombatTarget == null)
        {
            // If you cant' find one, return
            if(!FindTarget())
            {
                GetComponentInChildren<Text>().text = "No Target";
                return;
            }
        }
        LookAtTarget();

        // If the target is dead, forget about it or move on
        if (CombatTarget.GetComponent<CharacterStats>().GetStat("health") <= 0)
        {
            Stats.ModifyStat("anger", -Stats.GetStat("anger"));
            CombatTarget = null;
        }

        // Evaluate the NN
        Update();
        Results.Sort();

        switch (Results[Results.Count - 1].ID)
        {
            case 0:
            {
                if (!InRange())
                {
                    GetComponentInChildren<Text>().text = "Moving to target";

                    if (AIAgent.isOnNavMesh)
                    {
                        AIAgent.SetDestination(transform.position + transform.forward);
                        AIAgent.isStopped = false;
                    }
                }
                else
                {
                    if (Stats.GetStat("stamina") > 0)
                    {
                        GetComponentInChildren<Text>().text = "Attacking";

                        if (AIAgent.isOnNavMesh)
                        {
                            AIAgent.isStopped = true;
                        }

                        Attack();
                    }
                    else
                    {
                        GetComponentInChildren<Text>().text = "Waiting for stamina";
                    }
                }
                break;
            }
            case 1:
            {

                if (Stats.GetStat("stamina") > 0)
                {
                    GetComponentInChildren<Text>().text = "Dodging";

                    Stats.ModifyStat("stamina", -DodgeCost);
                    Stats.ModifyStat("rcount", -Stats.GetStat("rcount"));

                    Anim.SetInteger("Dodge", Random.Range(-5, 5));
                }
                else
                {
                    GetComponentInChildren<Text>().text = "Waiting for stamina";
                }
                break;
            }
            case 2:
            {
                AIAgent.isStopped = true;
                GetComponentInChildren<Text>().text = "Waiting for stamina";
                break;
            }
        }
    }

    void Attack()
    {
        if (!Anim.GetBool("Attacking"))
        {
            Stats.ModifyStat("stamina", -AttackCost);
            Anim.SetInteger("AttackType", Random.Range(0, 8));
        }
    }

    // Check if the combat target is in range
    bool InRange()
    {
        if (CombatTarget == null)
        {
            return false;
        }

        if (Vector3.Distance(CombatTarget.transform.position, transform.position) > Stats.table["range"])
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // Look at the combat target
    void LookAtTarget()
    {
        Vector3 lookPos;
        Quaternion rotation;

        // Get the look position
        lookPos = CombatTarget.transform.position - transform.position;
        lookPos.y = 0;

        // Rotate towards the look position
        rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);
    }

    public void SetTarget(GameObject t)
    {
        // Guards need to remember their first target
        if(Stats.faction == Factions.Guards)
        {
            if(CombatTarget == null)
            {
                CombatTarget = t;
            }
        }
        else
        {
            CombatTarget = t;
        }
        
    }

    // Finds nearest combat target
    bool FindTarget()
    {
        float targetDistance = 1000;
        float newDistance = 0;
        List<GameObject> objects = new List<GameObject>();

        objects.AddRange(GameObject.FindGameObjectsWithTag("character"));
        objects.AddRange(GameObject.FindGameObjectsWithTag("Player"));

        if (objects.Count > 0) // If there are any enemies (not including itself)
        {
            foreach (GameObject element in objects)
            {
                if (element.name != name && element.GetComponent<CharacterStats>().GetStat("health") > 0) // And the enemy isn't itself or deadヽ( ͡ಠ ʖ̯ ͡ಠ)ﾉ
                {
                    newDistance = Vector3.Distance(element.transform.position, transform.position); // Compare distance
                    if (newDistance < targetDistance)
                    {
                        targetDistance = newDistance;
                        CombatTarget = element; // Set new target
                    }
                }
            }
            return true; // Target is found and set
        }

        return false; // No valid target was found
    }
}
