using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterController : MonoBehaviour
{
    public bool InCombat = false;

    // Components
    private Animator Anim;
    private Camera MainCam;
    private CharacterStats Stats;

    private CombatController CController;
    private NeedsController NController;

    private GameObject CombatTarget;
    private NavMeshAgent AIAgent;
    private ThirdPersonCharacter TPController;


    enum State
    {
        Idle = 0,
        Combat = 1
    }
    State currentState = State.Idle;

    private void Start()
    {
        MainCam = Camera.main;
        AIAgent = GetComponent<NavMeshAgent>();
        TPController = GetComponent<ThirdPersonCharacter>();
        Anim = GetComponent<Animator>();
        Stats = GetComponent<CharacterStats>();

        CController = GetComponent<CombatController>();
        NController = GetComponent<NeedsController>();

        AIAgent.updateRotation = false;
    }


    void Update()
    {
        // When lmb is clicked draw a line from the camera 
        // through the view matrix, set the collision point 
        // as target destination for the AI agent
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);
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
            TPController.Move(AIAgent.desiredVelocity, false, false);
        }
        else
        {
            TPController.Move(Vector3.zero, false, false);
            AIAgent.isStopped = true;
        }


        if (InCombat)
        {
            // Check for enemy targets
            if (!Anim.GetBool("InCombat") && GetTarget()) // If not in combat and enemy detected, get in combat
            {
                Anim.SetBool("InCombat", true);
                currentState = State.Combat;
            }
            else if (Anim.GetBool("InCombat") && !GetTarget()) // If in combat and there are no enemies, leave combat
            {
                Anim.SetBool("InCombat", false);
                currentState = State.Idle;
            }
        }
        else
        {
            currentState = State.Idle;
        }



        // Check character state
        switch (currentState)
        {
            case State.Idle: // If idle, run the idle loop
                {
                    Debug.Log("Runing Idle");
                    NController.Run();
                    break;
                }
            case State.Combat: // If in combat, run the combat loop
                {
                    Debug.Log("Runing Combat");
                    CController.Run(CombatTarget);
                    break;
                }
            default:
                {
                    Debug.Log("Runing Default");
                    NController.Run();
                    break;
                }
        }

    }

    // Finds nearest combat target
    bool GetTarget()
    {
        float targetDistance = 1000;
        float newDistance = 0;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Finish");

        if (objects.Length > 0) // If there are any enemies (not including itself)
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