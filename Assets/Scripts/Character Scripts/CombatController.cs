using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class CombatController : AiBehaviour
{
    public float AttackCost;
    public float DodgeCost;

    // Components
    private Animator Anim;
    private NavMeshAgent AIAgent;
    private GameObject CombatTarget;

    private int output = 0;
    private float count = 0;

    public override void Start()
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "CombatNetwork.nn");
        base.Start();

        // Get components
        Anim = GetComponent<Animator>();
        AIAgent = GetComponent<NavMeshAgent>();
        Stats = GetComponent<CharacterStats>();

    }

    public override void Run()
    {
        while (CombatTarget == null)
        {
            FindTarget();
        }
        LookAtTarget();

        // Evaluate the NN
        float outputTotal = -2;

        for (int i = 0; i < _outputs.Length; i++)
        {
            if (_outputs[i] > outputTotal)
            {
                output = i;
                outputTotal = _outputs[i];
            }
        }

        switch (output)
        {
            case 0:
                {
                    if (!InRange())
                    {
                        GetComponentInChildren<Text>().text = "Moving to target";

                        if (AIAgent.isOnNavMesh)
                        {
                            AIAgent.SetDestination(this.transform.position + this.transform.forward);
                            AIAgent.isStopped = false;
                        }
                    }
                    else
                    {
                        AIAgent.isStopped = true;
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
                    AIAgent.isStopped = true;
                    if (Stats.GetStat("stamina") > 0)
                    {
                        GetComponentInChildren<Text>().text = "Dodging";
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
                    GetComponentInChildren<Text>().text = "Waiting";
                    break;
                }
        }
    }

    void Attack()
    {
        if (!Anim.GetBool("Attacking"))
        {
            Anim.SetInteger("AttackType", Random.Range(0, 8));
            
        }
    }

    // Check if the combat target is in range
    bool InRange()
    {
        if (Vector3.Distance(CombatTarget.transform.position, this.transform.position) > Stats.table["range"])
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
        Vector3 lookPos = CombatTarget.transform.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);
    }

    public void SetTarget(GameObject t)
    {
        CombatTarget = t;
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
                if (element.name != this.name) // And the enemy isn't itself ヽ( ͡ಠ ʖ̯ ͡ಠ)ﾉ
                {
                    newDistance = Vector3.Distance(element.transform.position, this.transform.position); // Compare distance
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
