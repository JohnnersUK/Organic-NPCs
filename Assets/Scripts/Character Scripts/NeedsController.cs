using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NeedsController : AiBehaviour
{
    // Components
    private Animator Anim;
    private NavMeshAgent AIAgent;
    public GameObject Target { private get; set; }

    private Interactable[] Objects;

    private string ActionTag;

    private bool Using = false;

    public float EventCounter { private get; set; }
    public EventType EventType { private get; set; }

    private Stack ActionQueue = new Stack();

    public override void Start()
    {
        GameObject interactables;

        EventCounter = 0;
        EventType = 0;

        // Set the filePath and init the network
        FilePath = Path.Combine(Application.streamingAssetsPath, "NeedsNetwork.nn");
        base.Start();

        // Get components
        Anim = GetComponent<Animator>();
        AIAgent = GetComponent<NavMeshAgent>();

        interactables = GameObject.FindGameObjectWithTag("interactables");
        Objects = interactables.GetComponentsInChildren<Interactable>();
    }

    public override void Update()
    {
        float[] inputs;

        // Run the NN
        inputs = Stats.GetStats(Inputs);        // Update the inputs
        Results = Network.Run(inputs);             // Pass them through the NN
    }

    public void Run()
    {
        QueueItem currentAction;

        // If there is an event taking place
        if (EventCounter > 0.0f)
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
                }
            }
        }
    }

    private void GetNewAction()
    {
        int priority;

        float newDistance = 0;
        float targetDistance = 1000;

        bool hasAction = false;

        // Evaluate the NN
        Update();
        Results.Sort();

        priority = Results.Count - 1; // Set the priority level

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
            Target = null;
            foreach (Interactable element in Objects)
            {
                // If the object is of the activity type, shares the same faction and isn't occupied
                if (element.Type == ActionTag && (element._Factions.Count == 0 || element._Factions.Contains(Stats.faction)) && !element.Occupied)
                {
                    newDistance = Vector3.Distance(element.transform.position, transform.position); // Compare distance
                    if (newDistance < targetDistance)
                    {
                        targetDistance = newDistance;
                        Target = element.gameObject;
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
                hasAction = true;
            }
            else // Decrease the priority level and try again
            {
                priority--;
                if (priority < 0)
                {
                    priority = Results.Count - 1;  // Loop back around if no action is still available
                }
            }
        } while (!hasAction);
    }

    private void ResolveEvent()
    {
        QueueItem currentAction;

        switch (EventType)
        {
            default:
            case EventType.Hit:
            {
                AIAgent.isStopped = true;
                if (ActionQueue.Count == 0) // If theres nothing in the action queue
                {
                    Anim.Play("Base Layer.Grounded");
                    ActionQueue.Push(new QueueItem("LookAt", Target));
                }
                else // If there is something in the queue
                {
                    currentAction = ActionQueue.Peek() as QueueItem;
                    if (currentAction.Action != "LookAt" || currentAction.Action != "Flee")
                    {
                        Anim.Play("Base Layer.Grounded");
                        ActionQueue.Push(new QueueItem("LookAt", Target));
                    }
                }

                EventCounter -= Time.deltaTime;
                if (EventCounter <= 0.0f)
                {
                    currentAction = ActionQueue.Peek() as QueueItem;
                    if (currentAction.Action == "LookAt")
                    {
                        ActionQueue.Pop();
                    }
                }
                break;
            }
            case EventType.Death:
            {
                if (Stats.faction == Factions.Neutral)
                {
                    AIAgent.isStopped = true;
                    Anim.Play("Base Layer.Grounded");
                    ActionQueue.Clear();

                    ActionQueue.Push(new QueueItem("Flee", Target));
                }
                break;
            }
        }
    }

    // Move to a location
    private void MoveTo(GameObject t)
    {
        Anim.Play("Base Layer.Grounded");
        GetComponentInChildren<Text>().text = "Moving to object";

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
            ActionQueue.Pop();
        }
    }

    // Use an interactable object
    private void Use(GameObject t)
    {
        GetComponentInChildren<Text>().text = t.tag + "ing";

        if (Using && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + Stats.faction.ToString() + "." + t.tag))
        {
            ActionQueue.Pop();
            Using = false;
            t.GetComponent<Interactable>().Occupied = false;
        }
        else if (!Using && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + Stats.faction.ToString() + "." + t.tag))
        {
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

            Using = true;
        }

    }

    // Look at the target
    private void LookAtTarget()
    {
        Vector3 lookPos;
        Quaternion rotation;

        float cy;
        float ty;
        float diff;

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
            Anim.SetFloat("Turn", -0.5f);
        }
        else
        {
            Anim.SetFloat("Turn", 0.5f);
        }

        // Turn the character
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);

        // If the current rotation is within 1 degree
        if (Quaternion.Angle(transform.rotation, rotation) <= 1)
        {
            ActionQueue.Pop();
        }
    }

    private void Flee(GameObject t)
    {
        Anim.Play("Base Layer.Grounded");
        GetComponentInChildren<Text>().text = "Moving to object";

        if (Vector3.Distance(t.GetComponent<Interactable>().InteractPoint.position, transform.position) > 0.5)
        {
            if (AIAgent.isOnNavMesh)
            {
                AIAgent.SetDestination(t.GetComponent<Interactable>().InteractPoint.position);
                AIAgent.isStopped = false;
            }
        }
    }

    private void SetUsing(int u)
    {
        if (u == 1)
        {
            Using = true;
        }
    }
}