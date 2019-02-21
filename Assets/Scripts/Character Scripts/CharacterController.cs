using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterController : MonoBehaviour
{
    public bool InCombat = false;
    public Material dead;

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
        Combat = 1,
        Dead = 2
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

        // Randomize the needs on startup                                                                                                             
        // Stats.Randomize();
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

        // Check the state
        if (Stats.health <= 0)
        {
            currentState = State.Dead;
        }
        else if (InCombat)
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
            Anim.SetBool("InCombat", false);
            currentState = State.Idle;
        }

        // Run the current state
        switch (currentState)
        {
            case State.Idle: // If idle, run the idle loop
                {
                    NController.Run();
                    NController.NeedsNetwork.AddFitness(Stats.happiness);
                    break;
                }
            case State.Combat: // If in combat, run the combat loop
                {
                    CController.Run(CombatTarget);
                    break;
                }
            case State.Dead:
                {
                    // Do nothing
                    SkinnedMeshRenderer[] skin = GetComponentsInChildren<SkinnedMeshRenderer>();

                    foreach (SkinnedMeshRenderer smr in skin)
                    {
                        smr.material = dead;
                    }

                    AIAgent.isStopped = true;
                    GetComponentInChildren<Text>().text = "Dead";
                    enabled = false;

                    // Bots can no longer gain fitness whilst dead
                    NController.NeedsNetwork.AddFitness(-Stats.happiness);

                    break;
                }
        }

    }

    // Finds nearest combat target
    bool GetTarget()
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