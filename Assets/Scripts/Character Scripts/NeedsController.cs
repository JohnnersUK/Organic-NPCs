using System.Collections.Generic;
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

    public GameObject target;
    private Interactable[] objects;

    private int output = 0;

    private float count = 0;
    private string actionTag;

    public bool Using = false;

    public bool _event = false;
    public float _eventCount = 0.0f;

    private Stack ActionQueue = new Stack();

    public override void Start()
    {
        // Set the filePath and init the network
        filePath = Path.Combine(Application.streamingAssetsPath, "NeedsNetwork.nn");
        base.Start();

        // Get components
        Anim = GetComponent<Animator>();
        AIAgent = GetComponent<NavMeshAgent>();

        GameObject interactables = GameObject.FindGameObjectWithTag("interactables");
        objects = interactables.GetComponentsInChildren<Interactable>();
    }

    public override void Update()
    {
        // Run the NN
        float[] inputs = Stats.GetStats(Inputs);        // Update the inputs
        _outputs = Network.Run(inputs);             // Pass them through the NN
    }

    public override void Run()
    {
        QueueItem currentAction;

        // If there is an event taking place
        if (_eventCount > 0.0f)
        {
            AIAgent.isStopped = true;
            if (ActionQueue.Count < 1) // If theres nothing in the action queue
            {
                Anim.Play("Base Layer.Grounded");
                ActionQueue.Push(new QueueItem("LookAt", target));
            }
            else // If there is something in the queue
            {
                currentAction = ActionQueue.Peek() as QueueItem;
                if (currentAction.Action != "LookAt")
                {
                    Anim.Play("Base Layer.Grounded");
                    ActionQueue.Push(new QueueItem("LookAt", target));
                }
            }

            _eventCount -= Time.deltaTime;
            if (_eventCount <= 0.0f)
            {
                currentAction = ActionQueue.Peek() as QueueItem;
                if (currentAction.Action == "LookAt")
                {
                    ActionQueue.Pop();
                }
            }
        }

        if (ActionQueue.Count < 1) // If theres nothing in the action queue
        {
            GetComponentInChildren<Text>().text = "Getting Action";

            GetNewAction();
        }
        else // If there is something in the queue
        {
            GetComponentInChildren<Text>().text = "Processing Action";

            currentAction = ActionQueue.Peek() as QueueItem;

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
        Results = new List<Output>();
        for (int i = 0; i < _outputs.Length; i++)
        {
            Output temp = new Output();
            temp.ID = i;
            temp.Value = _outputs[i];
            Results.Add(temp);
        }

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
                    actionTag = "Eat";
                    break;
                // Sleep
                case 1:
                    actionTag = "Sleep";
                    break;
                // Work
                case 2:
                    actionTag = "Work";
                    break;
                // Recreational
                case 3:
                    actionTag = "Recreational";
                    break;
            }

            // Find closest object of that activity type
            target = null;
            foreach (Interactable element in objects)
            {
                // If the object is of the activity type, shares the same faction and isn't occupied
                if (element.Type == actionTag && (element._Factions.Count == 0 || element._Factions.Contains(Stats.faction)) && !element.Occupied)
                {
                    newDistance = Vector3.Distance(element.transform.position, this.transform.position); // Compare distance
                    if (newDistance < targetDistance)
                    {
                        targetDistance = newDistance;
                        target = element.gameObject;
                    }
                }
            }

            // If one exists, go use it
            if (target)
            {
                // Add actions to the queue
                ActionQueue.Push(new QueueItem("Use", target));
                ActionQueue.Push(new QueueItem("MoveTo", target));

                target.GetComponent<Interactable>().Occupied = true;
                hasAction = true;
            }
            else // Decrease the priority level and try again
            {
                priority--;
                if (priority < 0)
                    priority = Results.Count - 1;  // Loop back around if no action is still available
            }
        } while (!hasAction);
    }

    // Move to a location
    private void MoveTo(GameObject t)
    {
        Anim.Play("Base Layer.Grounded");
        GetComponentInChildren<Text>().text = "Moving to object";

        if (Vector3.Distance(t.GetComponent<Interactable>().InteractPoint.position, this.transform.position) > 0.5)
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

        // Make the bot look at the object its using
        Vector3 lookPos = t.transform.position - transform.position;
        lookPos.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2);

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
                    Stats.hunger += 50;
                    break;
                case "Sleep":
                    // Reset fatigue and stamina
                    // Clear the hapiness modifiers
                    Stats.fatigue = 100;
                    Stats.stamina = 100;
                    Stats.hModifiers.Clear();
                    break;
                case "Work":
                    Stats.boredom += 50;
                    break;
                case "Recreational":
                    Stats.social += 50;
                    break;
            }

            Using = true;
        }

    }

    // Look at the target
    void LookAtTarget()
    {
        GetComponentInChildren<Text>().text = "Looking at target";
        Vector3 lookPos = target.transform.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);

        if(transform.rotation == rotation)
        {
            ActionQueue.Pop();
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