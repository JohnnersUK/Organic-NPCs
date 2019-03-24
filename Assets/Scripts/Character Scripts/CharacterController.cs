﻿using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterController : AiBehaviour
{
    public bool InCombat = false;
    public Material dead;

    // Interactions
    private float interactionCount = 0.0f;
    private Transform interactPoint;

    // Components
    private Animator Anim;
    private Camera MainCam;

    private CombatController CController;
    private NeedsController NController;

    private GameObject CombatTarget;
    private NavMeshAgent AIAgent;
    private ThirdPersonCharacter TPController;

    private EventManager em;

    enum State
    {
        Idle = 0,
        Combat = 1,
        Dead = 2
    }
    State currentState = State.Idle;

    // Death event
    public delegate void DeathEventHandler(object source, PublicEventArgs args);
    public event DeathEventHandler DeathEvent;

    public override void Start()
    {
        // Set the filePath and init the network
        filePath = Path.Combine(Application.streamingAssetsPath, "MasterNetwork.nn");
        base.Start();

        MainCam = Camera.main;
        AIAgent = GetComponent<NavMeshAgent>();
        TPController = GetComponent<ThirdPersonCharacter>();
        Anim = GetComponent<Animator>();
        Stats = GetComponent<CharacterStats>();

        CController = GetComponent<CombatController>();
        NController = GetComponent<NeedsController>();

        AIAgent.updateRotation = false;

        // Subscribe to event manager events
        EventManager.instance.HitEvent += OnHitEvent;
        EventManager.instance.DeathEvent += OnDeathEvent;
    }

    public override void Update()
    {
        if (currentState == State.Dead)
            return;

        // If the AI Agent isn't at its target position, 
        // Call the move funciton in the TPS
        if (AIAgent.isOnNavMesh)
        {
            if (AIAgent.remainingDistance > AIAgent.stoppingDistance)
            {
                TPController.Move(AIAgent.desiredVelocity, false, false);
            }
            else
            {
                TPController.Move(Vector3.zero, false, false);
                AIAgent.isStopped = true;
            }
        }

        if (Stats.health > 0)
        {
            //Check the state
            //Run the NN
            float[] inputs = Stats.GetStats(Inputs);            // Update the inputs
            float[] _outputs = Network.Run(inputs);       // Pass them through the NN

            // Evaluate the NN
            Results = new List<Output>();                 // Create the list of results
            for (int i = 0; i < Outputs; i++)
            {
                Output t = new Output();
                t.ID = i;
                t.Value = _outputs[i];
                Results.Add(t);
            }
            Results.Sort();                                     // Sort the list of results
            currentState = (State)Results[Results.Count - 1].ID;  // Set the result
        }
        else
        {
            currentState = State.Dead;
        }

        // Run the current state
        switch (currentState)
        {
            case State.Idle: // If idle, run the idle loop
                {
                    if (Anim.GetBool("InCombat"))
                    {
                        Anim.SetBool("InCombat", false);
                    }

                    NController.Run();
                    break;
                }
            case State.Combat: // If in combat, run the combat loop
                {
                    if (!Anim.GetBool("InCombat"))
                    {
                        Anim.SetBool("InCombat", true);
                        Anim.Play("Base Layer.Combat.Locomotion");
                    }

                    CController.Run();
                    break;
                }
            case State.Dead:
                {
                    // Do nothing
                    if(!Anim.GetBool("Dead"))
                    {
                        Anim.SetBool("Dead", true);
                        Anim.Play("Base Layer.Dead");
                        SendDeathEvent();
                    }

                    SkinnedMeshRenderer[] skin = GetComponentsInChildren<SkinnedMeshRenderer>();

                    foreach (SkinnedMeshRenderer smr in skin)
                    {
                        smr.material = dead;
                    }

                    AIAgent.isStopped = true;
                    GetComponentInChildren<Text>().text = "Dead";

                    // Bots can no longer gain fitness whilst dead
                    NController.Network.AddFitness(-Stats.happiness);

                    break;
                }
        }

        NController.Network.AddFitness(Stats.happiness * Time.deltaTime);
    }

    // A nearby robot has been hit
    public void OnHitEvent(object source, PublicEventArgs args)
    {
        Debug.Log("Here");
        if(args.Agent == null || args.Subject == null)
        {
            return;
        }

        CharacterStats agentStats = args.Agent.GetComponent<CharacterStats>();
        CharacterStats subjectStats = args.Subject.GetComponent<CharacterStats>();

        if (agentStats == null || subjectStats == null)
        {
            return;
        }

        // If the bot is in range of the event
        if (Vector3.Distance(args.Agent.transform.position, gameObject.transform.position) < args.Range
            && currentState == State.Idle)
        {
            switch (Stats.faction)
            {
                default:
                case Factions.Neutral:
                    {
                        NController._eventCount = 1.0f;
                        NController.target = args.Agent;
                        NController._eventType = EventType.Hit;
                        Stats.fear += 1.0f;
                        break;
                    }
                case Factions.Barbarians:
                    {
                        if(subjectStats.faction == Stats.faction)
                        {
                            Stats.anger += 10.0f;
                            CController.SetTarget(args.Agent);
                        }
                        else
                        {
                            NController._eventCount = 1.0f;
                            NController.target = args.Agent;
                        }
                        break;
                    }
                case Factions.Guards:
                    {
                        if(agentStats.faction != Factions.Guards)
                        {
                            Stats.anger += 10.0f;
                            CController.SetTarget(args.Agent);
                        }                       
                        break;
                    }
            }
        }
    }

    public void OnDeathEvent(object source, PublicEventArgs args)
    {
        // If the bot is in range of the event && they didn't trigger it
        if (Vector3.Distance(args.Agent.transform.position, gameObject.transform.position) < args.Range
            && args.Agent != gameObject)
        {
            switch (Stats.faction)
            {
                default:
                case Factions.Neutral:
                    {
                        // Flee
                        NController._eventCount = 1.0f;
                        NController._eventType = EventType.Death;
                        NController.target = GameObject.FindGameObjectWithTag("Exit");
                        Stats.fear += 10.0f;
                        break;
                    }
                case Factions.Barbarians:
                    {
                        // Barbarians do not care for mortal death
                        break;
                    }
                case Factions.Guards:
                    {
                        // Call for backup
                        break;
                    }
            }
        }
    }

    protected virtual void SendDeathEvent()
    {
        if (DeathEvent != null)
        {
            PublicEventArgs args = new PublicEventArgs(gameObject, null, EventType.Death, 100);
            DeathEvent(this, args);
        }
    }
}