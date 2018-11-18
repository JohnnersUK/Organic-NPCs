using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterController : MonoBehaviour
{
    // Components
    Animator anim;
    Camera mainCam;
    CharacterStats stats;
    NavMeshAgent AIAgent;
    ThirdPersonCharacter controller;

    GameObject combatTarget;

    enum State
    {
        Idle = 0,
        Combat = 1
    }
    State currentState = State.Idle;

    private void Start()
    {
        mainCam = Camera.main;
        AIAgent = GetComponent<NavMeshAgent>();
        controller = GetComponent<ThirdPersonCharacter>();
        anim = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();

        AIAgent.updateRotation = false;
    }


    void Update()
    {
        // When lmb is clicked draw a line from the camera 
        // through the view matrix, set the collision point 
        // as target destination for the AI agent
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                AIAgent.SetDestination(hit.point);
                AIAgent.isStopped = false;
                Debug.Log(hit.point);
            }
        }

        // If the AI Agent isn't at its target position, 
        // Call the move funciton in the TPS
        if (AIAgent.remainingDistance > AIAgent.stoppingDistance)
        {
            controller.Move(AIAgent.desiredVelocity, false, false);
        }
        else
        {
            controller.Move(Vector3.zero, false, false);
            AIAgent.isStopped = true;
        }


        // Check for enemy targets
        if (!anim.GetBool("InCombat") && GetTarget()) // If not in combat and enemy detected, get in combat
        {
            anim.SetBool("InCombat", true);
            currentState = State.Combat;
        }
        else if (anim.GetBool("InCombat") && !GetTarget()) // If in combat and there are no enemies, leave combat
        {
            anim.SetBool("InCombat", false);
            currentState = State.Idle;
        }

        switch (currentState)
        {
            case State.Idle:
                {
                    UpdateIdle();
                    break;
                }
            case State.Combat:
                {
                    UpdateCombat();
                    break;
                }
            default:
                {
                    UpdateIdle();
                    break;
                }
        }


    }

    void UpdateIdle()
    {
        // Idle neural network stuff
    }

    void UpdateCombat()
    {
        // Combat neural network stuff
        LookAtTarget();

        //Debug moveTo
        if (!InRange())
        {
            AIAgent.SetDestination(this.transform.position + this.transform.forward);
            AIAgent.isStopped = false;
        }

        // Debug attack
        if (Input.GetMouseButtonDown(1))
        {
            Attack();
        }
    }

    // Check if the combat target is in range
    bool InRange()
    {
        if (Vector3.Distance(combatTarget.transform.position, this.transform.position) > stats.range)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    void Attack()
    {
        anim.SetInteger("AttackType", Random.Range(0, 8));
    }

    // Finds nearest combat target
    bool GetTarget()
    {
        float targetDistance = 1000;
        float newDistance = 0;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("character");

        if (objects.Length > 1) // If there are any enemies (not including itself)
        {
            foreach (GameObject element in objects)
            {
                if (element.name != this.name) // And the enemy isn't itself ヽ( ͡ಠ ʖ̯ ͡ಠ)ﾉ
                {
                    newDistance = Vector3.Distance(element.transform.position, this.transform.position); // Compare distance
                    if (newDistance < targetDistance)
                    {
                        targetDistance = newDistance;
                        combatTarget = element; // Set new target
                    }
                }
            }
            return true; // Target is found and set
        }

        return false; // No valid target was found
    }

    // Look at the combat target
    void LookAtTarget()
    {
        transform.LookAt(combatTarget.transform.position);
    }
}