using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class NeedsController : AiBehaviour
{
    [SerializeField] private GameObject TempInteractable;
    private GameObject tmp;

    // Components
    private Animator Anim;
    private NavMeshAgent AIAgent;
    private GameObject Target;
    private ThirdPersonCharacter TPController;

    private Interactable[] Objects;

    private string ActionTag;

    private bool Doing = false;

    private bool UnresolvedEvent;
    private float EventCounter;
    private EventType CurrentEventType;

    private Stack ActionQueue = new Stack();

    public override void Start()
    {
        GameObject interactables;

        EventCounter = 0;
        CurrentEventType = 0;

        // Set the filePath and init the network
        FilePath = Path.Combine(Application.streamingAssetsPath, "NeedsNetwork.nn");
        base.Start();

        // Get components
        Anim = GetComponent<Animator>();
        AIAgent = GetComponent<NavMeshAgent>();

        interactables = GameObject.FindGameObjectWithTag("interactables");
        Objects = interactables.GetComponentsInChildren<Interactable>();
        TPController = GetComponent<ThirdPersonCharacter>();
    }

    public override void Update()
    {
        float[] inputs;

        // Run the NN
        inputs = Stats.GetStats(Inputs);        // Update the inputs
        Results = Network.Run(inputs);             // Pass them through the NN
    }

    public override void Run()
    {
        QueueItem currentAction;

        if (Anim.GetBool("InCombat"))
        {
            Anim.SetBool("InCombat", false);
        }

        // If there is an unresolved event
        if (UnresolvedEvent)
        {
            ResolveEvent();
        }

        if (ActionQueue.Count < 1) // If theres nothing in the action queue
        {
            GetComponentInChildren<Text>().text = "Getting Action";

            GetNewAction();
        }
        else // If there is something in the queue
        {
            GetComponentInChildren<Text>().text = "Processing Action";

            if (ActionQueue.Count > 0)
            {
                currentAction = ActionQueue.Peek() as QueueItem;

                if (currentAction == null)
                {
                    return;
                }

                switch (currentAction.Action)
                {
                    case "MoveTo":
                    MoveTo(currentAction.Target);
                    break;
                    case "Use":
                    Use(currentAction.Target);
                    break;
                    case "LookAt":
                    LookAtTarget();
                    break;
                    case "Flee":
                    Flee(currentAction.Target);
                    break;
                    case "Hide":
                    Hide();
                    break;
                    case "Idle":
                    Idle();
                    break;
                    case "Aggravate":
                    Aggravate(Target);
                    break;
                }
            }
        }
    }

    private void GetNewAction()
    {
        int priority;
        float newDistance = 0;
        float targetDistance = 1000;
        float rnd = 0;
        bool hasAction = false;

        Interactable lastChecked = null;
        NavMeshHit hit;
        Vector3 rndDir;
        Vector3 finalPosition;

        // Evaluate the NN
        Update();
        Results.Sort();

        priority = Results.Count - 1; // Set the priority level

        SetTarget(null); // Reset the target
        Doing = false;

        // Find an activity
        do
        {
            // Set the action tag to the activity type
            switch (Results[priority].ID)
            {
                // Eat
                case 0:
                ActionTag = "Eat";
                break;
                // Sleep
                case 1:
                ActionTag = "Sleep";
                break;
                // Work
                case 2:
                ActionTag = "Work";
                break;
                // Recreational
                case 3:
                ActionTag = "Recreational";
                break;
            }

            // Find closest object of that activity type
            foreach (Interactable element in Objects)
            {
                // If the object is of the activity type and shares the same faction
                if (element.Type == ActionTag && (element._Factions.Count == 0 || element._Factions.Contains(Stats.faction)))
                {
                    // If the object isnt occupied, use it
                    if (!element.Occupied)
                    {
                        newDistance = Vector3.Distance(element.transform.position, transform.position); // Compare distance
                        if (newDistance < targetDistance)
                        {
                            targetDistance = newDistance;
                            SetTarget(element.gameObject);
                        }
                    }
                    else
                    {
                        lastChecked = element;
                    }
                }
            }

            // If one exists, go use it
            if (Target)
            {
                // Add actions to the queue
                ActionQueue.Push(new QueueItem("Use", Target));
                ActionQueue.Push(new QueueItem("LookAt", Target));
                ActionQueue.Push(new QueueItem("MoveTo", Target));

                Target.GetComponent<Interactable>().Occupied = true;
                Target.GetComponent<Interactable>().SetLastUsed(gameObject);
                hasAction = true;
            }
            else
            {
                // Choose wether to decrease the priority and try again or get angry
                switch (Stats.faction)
                {
                    case Factions.Barbarians:
                    {
                        // 1 in 5 to get angry
                        rnd = Random.Range(0, 10);
                        break;
                    }
                    case Factions.Guards:
                    {
                        // No chance to get angry
                        rnd = 5;
                        break;
                    }
                    case Factions.Neutral:
                    {
                        // 1 in 20 to get angry
                        rnd = Random.Range(0, 20);
                        break;
                    }
                }

                if (rnd <= 1)
                {
                    SetTarget(lastChecked.GetLastUsed());
                    // Get angry
                    ActionQueue.Push(new QueueItem("Aggravate", Target));
                    ActionQueue.Push(new QueueItem("LookAt", Target));
                    ActionQueue.Push(new QueueItem("MoveTo", Target));
                    hasAction = true;
                }
                else
                {
                    // Decrease priority and try again
                    priority--;
                    if (priority < 0)
                    {
                        // Nothing left to try so idle for a turn
                        // Random a location on the navmesh
                        rndDir = Random.insideUnitSphere * 10;
                        rndDir += transform.position;
                        NavMesh.SamplePosition(rndDir, out hit, 10, 1);
                        finalPosition = hit.position;

                        // Create a temp gameobject
                        tmp = Instantiate(TempInteractable);
                        tmp.name = "tmp";
                        tmp.transform.position = hit.position;

                        // Move to the gameobject
                        ActionQueue.Push(new QueueItem("Idle", Target));
                        ActionQueue.Push(new QueueItem("MoveTo", tmp));

                        hasAction = true;
                    }
                }

            }
        } while (!hasAction);
    }

    // Evaluates what to do in an event
    private void ResolveEvent()
    {
        // Random a float between the fear floor and 20
        float rnd = Random.Range(Stats.GetStat("fear"), 20);

        List<GameObject> exits = new List<GameObject>();
        QueueItem currentAction;

        switch (CurrentEventType)
        {
            default:
            case EventType.Death:
            {
                if (Stats.faction == Factions.Neutral)
                {
                    // Reset animations and clear the queue
                    AIAgent.isStopped = true;
                    Anim.Play("Base Layer.Grounded");
                    ActionQueue.Clear();

                    // Make the bot run
                    TPController.running = true;

                    // Add actions to the queue
                    ActionQueue.Push(new QueueItem("Hide", tmp));
                    ActionQueue.Push(new QueueItem("Flee", tmp));

                    // Set event time
                    EventCounter = 30.0f;
                    break;
                }
                break;
            }
            case EventType.Hit:
            {
                AIAgent.isStopped = true;
                if (ActionQueue.Count == 0) // If theres nothing in the action queue
                {
                    Anim.Play("Base Layer.Grounded");

                    // Random between running and watching
                    // Bot will always run if fear is greater than 
                    if (rnd > 19f)
                    {
                        // Flee
                        ActionQueue.Push(new QueueItem("Hide", tmp));
                        ActionQueue.Push(new QueueItem("Flee", tmp));

                        EventCounter = 15.0f;
                    }
                    else
                    {
                        ActionQueue.Push(new QueueItem("LookAt", Target));
                    }
                }
                else // If there is something in the queue
                {
                    currentAction = ActionQueue.Peek() as QueueItem;
                    if (currentAction.Action != "LookAt" && currentAction.Action != "Flee" && currentAction.Action != "Hide")
                    {
                        // Random between running and watching
                        // Bot will always run if fear is greater than
                        if (rnd > 19f)
                        {
                            // Flee
                            ActionQueue.Push(new QueueItem("Hide", Target));
                            ActionQueue.Push(new QueueItem("Flee", Target));

                            EventCounter = 15.0f;
                        }
                        else
                        {
                            Anim.Play("Base Layer.Grounded");
                            ActionQueue.Push(new QueueItem("LookAt", Target));
                        }
                    }
                }
                break;
            }
        }

        UnresolvedEvent = false;
    }

    // Move to a location
    private void MoveTo(GameObject t)
    {
        Anim.Play("Base Layer.Grounded");
        GetComponentInChildren<Text>().text = "Moving to object";

        // If the object has an interaction point
        if (t.GetComponent<Interactable>() != null)
        {
            // Move to it
            if (Vector3.Distance(t.GetComponent<Interactable>().InteractPoint.position, transform.position) > 0.5)
            {
                if (AIAgent.isOnNavMesh)
                {
                    AIAgent.SetDestination(t.GetComponent<Interactable>().InteractPoint.position);
                    AIAgent.isStopped = false;
                }
            }
            else
            {
                // If moving to a temporary location
                if (tmp != null)
                {
                    Destroy(tmp);
                    tmp = null;
                }

                // When close enough, stop
                ActionQueue.Pop();
            }
        }
        else
        {
            // Else move directly to the object
            if (Vector3.Distance(t.transform.position, transform.position) > 2.5)
            {
                if (AIAgent.isOnNavMesh)
                {
                    AIAgent.SetDestination(t.transform.position);
                    AIAgent.isStopped = false;
                }
            }
            else
            {
                // When close enough, stop
                ActionQueue.Pop();
            }

        }

    }

    // Use an interactable object
    private void Use(GameObject t)
    {
        GetComponentInChildren<Text>().text = t.tag + "ing";


        if (Doing && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + Stats.faction.ToString() + "." + t.tag))
        {
            // If the item has finished being used
            ActionQueue.Pop();

            Doing = false;

            SetTarget(null);
        }
        else if (!Doing && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + Stats.faction.ToString() + "." + t.tag))
        {
            // If the item hasn't started being used

            // Play the coresponding animation, per faction
            Anim.Play("Base Layer." + Stats.faction.ToString() + "." + t.tag);

            switch (t.tag)
            {
                case "Eat":
                Stats.ModifyStat("hunger", 50);
                break;
                case "Sleep":
                // Reset fatigue and stamina
                // Clear the hapiness modifiers
                Stats.ModifyStat("fatigue", 100);
                Stats.ModifyStat("stamina", 100);
                break;
                case "Work":
                Stats.ModifyStat("boredom", 50);
                break;
                case "Recreational":
                Stats.ModifyStat("social", 50);
                break;
            }

            Doing = true;
        }

    }

    // Look at the target
    private void LookAtTarget()
    {
        if (Target == null)
            return;

        Vector3 lookPos;
        Quaternion rotation;

        float cy;
        float ty;
        float diff;

        float turnRate = Anim.GetFloat("Turn");

        GetComponentInChildren<Text>().text = "Looking at target";
        lookPos = Target.transform.position - transform.position;
        lookPos.y = 0;

        rotation = Quaternion.LookRotation(lookPos);

        // Calculate turn direction and set animation
        cy = transform.rotation.eulerAngles.y;
        ty = rotation.eulerAngles.y;
        diff = ty - cy;

        if (diff < 0)
        {
            diff += 360;
        }

        if (diff > 180)
        {
            Anim.SetFloat("Turn", turnRate - 0.1f);
        }
        else
        {
            Anim.SetFloat("Turn", turnRate + 0.1f);
        }

        // Turn the character
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);

        // If the current rotation is within 1 degree
        if (Quaternion.Angle(transform.rotation, rotation) <= 1)
        {
            Anim.SetFloat("Turn", 0);
            ActionQueue.Pop();
        }
    }

    // Run to an exit
    private void Flee(GameObject t)
    {
        NavMeshHit hit;
        Vector3 rndDir;
        Vector3 finalPosition;

        if (!Doing || tmp == null)
        {
            TPController.running = true;
            // Pick a random hiding spot
            rndDir = Random.insideUnitSphere * 10;
            rndDir += transform.position;
            NavMesh.SamplePosition(rndDir, out hit, 10, 1);
            finalPosition = hit.position;

            // Create a temp gameobject
            tmp = Instantiate(TempInteractable);
            tmp.name = "tmp";
            tmp.transform.position = hit.position;

            Anim.Play("Base Layer.Grounded");
            GetComponentInChildren<Text>().text = "Fleeing";

            Doing = true;
        }
        else
        {
            if (Vector3.Distance(tmp.GetComponent<Interactable>().InteractPoint.position, transform.position) > 0.5)
            {
                if (AIAgent.isOnNavMesh)
                {
                    AIAgent.SetDestination(tmp.GetComponent<Interactable>().InteractPoint.position);
                    AIAgent.isStopped = false;
                }
            }
            else
            {
                TPController.running = false;

                // If moving to a temporary location
                if (tmp != null)
                {
                    Destroy(tmp);
                    tmp = null;
                }

                ActionQueue.Pop();
                Doing = false;
            }
        }

    }

    // Play the hiding animation until the evnt is over
    private void Hide()
    {
        GetComponentInChildren<Text>().text = "Hiding";

        if (!Anim.GetBool("Hiding"))
        {
            Anim.SetBool("Hiding", true);
            Anim.Play("Base Layer.Hiding.Crouching");
        }

        // When the event is over, stop hiding
        EventCounter -= Time.deltaTime;
        if (EventCounter <= 0.0f)
        {
            ActionQueue.Pop();
            Anim.SetBool("Hiding", false);
        }
    }

    // Play the idle animation
    private void Idle()
    {
        GetComponentInChildren<Text>().text = "Idle";
        if (Doing && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle"))
        {
            // If the bot has finished being idle
            ActionQueue.Pop();
            Doing = false;
        }
        else if (!Doing && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle"))
        {
            // If the bot hasn't started being idle
            Anim.Play("Base Layer.Idle");
            Doing = true;
        }
    }

    // Aggravates the target
    private void Aggravate(GameObject t)
    {
        GetComponentInChildren<Text>().text = "Angry";
        if (Doing && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + Stats.faction.ToString() + "." + "Aggravate"))
        {
            // If the bot has finished aggravating
            ActionQueue.Pop();

            // 'Hit' for 0 damage
            t.GetComponent<CharacterStats>().GetHit(gameObject, 0);
            Stats.ModifyStat("anger", 1f);
            GetComponent<CombatController>().SetTarget(t);

            Doing = false;
        }
        else if (!Doing && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + Stats.faction.ToString() + "." + "Aggravate"))
        {
            // If the bot hasn't started aggravating
            Anim.Play("Base Layer." + Stats.faction.ToString() + "." + "Aggravate");
            t.GetComponent<CharacterStats>().GetHit(gameObject, 0);
            Doing = true;
        }

    }

    public void SetUsing(int u)
    {
        if (u == 1)
        {
            Doing = true;
        }
    }

    public void SetEvent(EventType et)
    {
        UnresolvedEvent = true;
        CurrentEventType = et;
    }

    public void SetTarget(GameObject t)
    {
        if (Target != null)
        {
            // Remove occupation of old target
            if (Target.GetComponent<Interactable>() != null)
            {
                Target.GetComponent<Interactable>().Occupied = false;
            }
        }


        // Set new target and occupation
        Target = t;
        if (t != null && t.GetComponent<Interactable>() != null)
        {
            t.GetComponent<Interactable>().Occupied = true;
        }

    }
}