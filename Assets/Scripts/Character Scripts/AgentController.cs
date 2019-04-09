using System.IO;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class AgentController : AiBehaviour
{
    [SerializeField] private Material DeadMat = null;
    [SerializeField] private AiBehaviour[] Behaviours = null;

    [SerializeField] private float FallSpeed = 0.0f;
    [SerializeField] private float DeathDelay = 10.0f;

    // Interactions
    private Transform InteractPoint;

    // Components
    private Animator Anim;

    private NeedsController NController;

    private GameObject CombatTarget;
    private NavMeshAgent AIAgent;
    private ThirdPersonCharacter TPController;

    private enum State
    {
        Idle = 0,
        Combat = 1,
        Dead = 2
    }
    private State currentState = State.Idle;

    // Death event
    public delegate void DeathEventHandler(object source, PublicEventArgs args);
    public event DeathEventHandler DeathEvent;

    public override void Start()
    {
        // Set the filePath and init the network
        FilePath = Path.Combine(Application.streamingAssetsPath, "MasterNetwork.nn");
        base.Start();

        AIAgent = GetComponent<NavMeshAgent>();
        TPController = GetComponent<ThirdPersonCharacter>();
        Anim = GetComponent<Animator>();
        Stats = GetComponent<CharacterStats>();

        // Add Behaviors
        NController = GetComponent<NeedsController>();

        AIAgent.updateRotation = false;

        // Subscribe to event manager events
        EventManager.instance.HitEvent += OnHitEvent;
        EventManager.instance.DeathEvent += OnDeathEvent;
    }

    public override void Update()
    {
        int i = 1;
        bool invalid = true;
        float[] inputs;

        SkinnedMeshRenderer[] skin;
        AttackCollider[] colliders;

        if (currentState == State.Dead)
        {
            ResolveDeath();
        }

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

        if (Stats.GetStat("health") > 0)
        {
            //Check the state
            //Run the NN
            inputs = Stats.GetStats(Inputs);            // Update the inputs

            Results = Network.Run(inputs);       // Pass them through the NN
            Results.Sort();                      // Sort the list of results

            // Run the best, valid behavior
            // This allows behaviors to be swapped out at will
            do
            {
                if (Behaviours[Results[Results.Count - i].ID] != null)
                {
                    Behaviours[Results[Results.Count - i].ID].Run();
                    invalid = false;
                }
                else
                {
                    i++;
                }

            } while (invalid);

            NController.Network.AddFitness(Stats.GetStat("happiness") * Time.deltaTime);
        }
        else
        {
            if (currentState != State.Dead)
            {
                currentState = State.Dead;

                // Set animator
                if (!Anim.GetBool("Dead"))
                {
                    Anim.SetBool("Dead", true);
                    Anim.Play("Base Layer.Dead");
                    SendDeathEvent();
                }

                // Dissable its colliders
                colliders = GetComponentsInChildren<AttackCollider>();
                foreach (AttackCollider sc in colliders)
                {
                    sc.enabled = false;

                }
                GetComponent<CapsuleCollider>().enabled = false;

                // Set skin
                skin = GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer smr in skin)
                {
                    smr.material = DeadMat;
                }

                // Stop the agent
                AIAgent.isStopped = true;
                AIAgent.enabled = false;
                GetComponentInChildren<Text>().text = "Dead";
            }
        }
    }

    private void ResolveDeath()
    {
        if (transform.position.y <= -3.0f)
        {
            return;
        }

        // fall through the floor after a short delay
        DeathDelay -= Time.deltaTime;
        if (DeathDelay <= 0)
        {
            Vector3 newPos = transform.position;
            newPos.y -= Time.deltaTime * FallSpeed;
            transform.position = newPos;
        }
    }

    // A nearby robot has been hit
    public void OnHitEvent(object source, PublicEventArgs args)
    {
        CharacterStats agentStats;
        CharacterStats subjectStats;

        if (args.Agent == null || args.Subject == null)
        {
            return;
        }

        agentStats = args.Agent.GetComponent<CharacterStats>();
        subjectStats = args.Subject.GetComponent<CharacterStats>();

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
                    // Interuption event and add fear
                    NController.SetEvent(EventType.Hit, args.Agent.transform.position);
                    NController.SetTarget(args.Agent);
                    Stats.ModifyStat("fear", 1.0f);
                    break;
                }
                case Factions.Barbarians:
                {
                    // Join in fight
                    if (subjectStats.faction == Stats.faction)
                    {
                        NController.SetTarget(args.Agent);
                        NController.SetEvent(EventType.Hit, args.Agent.transform.position);
                    }
                    else if (agentStats.faction == Stats.faction)
                    {
                        NController.SetTarget(args.Subject);
                        NController.SetEvent(EventType.Hit, args.Subject.transform.position);
                    }
                    else // Or just watch
                    {
                        NController.SetTarget(args.Subject);
                        NController.SetEvent(EventType.Other, args.Subject.transform.position);
                    }

                    break;
                }
                case Factions.Guards:
                {
                    // Apprehend aggressor
                    NController.SetTarget(args.Subject);
                    NController.SetEvent(EventType.Hit, args.Subject.transform.position);                      
                    break;
                }
            }
        }
    }

    // A nearby robot died
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
                    NController.SetEvent(EventType.Death, args.Agent.transform.position);

                    Stats.ModifyStat("fear", 10.0f);
                    break;
                }
                case Factions.Barbarians:
                {
                    NController.SetEvent(EventType.Hit, args.Agent.transform.position);
                    NController.SetTarget(args.Agent);
                    break;
                }
                case Factions.Guards:
                {
                    NController.SetEvent(EventType.Hit, args.Agent.transform.position);
                    NController.SetTarget(args.Agent);
                    break;
                }
            }
        }
    }

    protected virtual void SendDeathEvent()
    {
        PublicEventArgs args;

        if (DeathEvent != null)
        {
            args = new PublicEventArgs(gameObject, null, EventType.Death, 100);
            DeathEvent(this, args);
        }
    }
}