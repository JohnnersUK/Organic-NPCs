﻿using System.Collections;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NeedsController : MonoBehaviour
{
    // Neural network
    public NeuralNetwork NeedsNetwork;

    public string[] Inputs;

    public int[] HiddenLayers;
    public int Outputs;

    public Output[] Results;

    // Components
    private Animator Anim;
    private CharacterStats Stats;
    private NavMeshAgent AIAgent;

    private GameObject target;
    private Interactable[] objects;

    private int output = 0;
    private float count = 0;
    private string actionTag;

    public bool Using = false;

    private Stack ActionQueue = new Stack();

    public void Start()
    {
        // Get components
        Anim = GetComponent<Animator>();
        Stats = GetComponent<CharacterStats>();
        AIAgent = GetComponent<NavMeshAgent>();

        GameObject interactables = GameObject.FindGameObjectWithTag("interactables");
        objects = interactables.GetComponentsInChildren<Interactable>();

        // Initilize the neural network
        int length = HiddenLayers.Length + 2;
        int[] layout = new int[length]; // Length of the NN = num of hidden layers + 2

        layout[0] = Inputs.Length; // First layer is the input layer

        // Initilize the hidden layer
        for (int i = 0; i < HiddenLayers.Length; i++) // For each hidden layer
        {
            layout[i + 1] = HiddenLayers[i]; // Set the number of nodes
        }

        layout[layout.Length - 1] = Outputs; // Last layer is the output layer

        NeedsNetwork = new NeuralNetwork(layout); // Construct the NN
    }


    public void Run()
    {
        QueueItem currentAction;

        // If theres nothing in the action queue
        if (ActionQueue.Count < 1)
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
            }
        }
    }

    private void GetNewAction()
    {
        int priority;

        float newDistance = 0;
        float targetDistance = 1000;

        bool hasAction = false;

        // Run the NN
        float[] inputs = Stats.GetStats(Inputs);        // Update the inputs
        float[] outputs = NeedsNetwork.Run(inputs);    // Pass them through the NN

        // Evaluate the NN
        Results = new Output[outputs.Length];

        for (int i = 0; i < Results.Length; i++)
        {
            Results[i] = new Output();
            Results[i].ID = i;
            Results[i].Value = outputs[i];
        }

        // Shell sort to determine priority
        int j, inc;
        Output temp;
        inc = 3;
        while (inc > 0)
        {
            for (int i = 0; i < Results.Length; i++)
            {
                j = i;
                temp = Results[i];
                while ((j >= inc) && (Results[j - inc].Value >
                    temp.Value))
                {
                    Results[j] = Results[j - inc];
                    j = j - inc;
                }
                Results[j] = temp;
            }
            if (inc / 2 != 0)
            {
                inc = inc / 2;
            }
            else if (inc == 1)
            {
                inc = 0;
            }
            else
            {
                inc = 1;
            }
        }

        priority = Results.Length - 1; // Set the priority level

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
                if (element.Type == actionTag && !element.Occupied)
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
                    priority = Results.Length - 1;  // Loop back around if no action is still available
            }
        } while (!hasAction);
    }

    private void MoveTo(GameObject t)
    {
        GetComponentInChildren<Text>().text = "Moving to object";

        if (Vector3.Distance(t.GetComponent<Interactable>().InteractPoint.position, this.transform.position) > 0.5)
        {
            AIAgent.SetDestination(t.GetComponent<Interactable>().InteractPoint.position);
            AIAgent.isStopped = false;
        }
        else
        {
            ActionQueue.Pop();
        }
    }

    private void Use(GameObject t)
    {
        GetComponentInChildren<Text>().text = t.tag + "ing";

        // Make the bot look at the object its using
        Vector3 lookPos = t.transform.position - transform.position;
        lookPos.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2);

        if (Using && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + t.tag))
        {
            ActionQueue.Pop();
            Using = false;
            t.GetComponent<Interactable>().Occupied = false;
        }
        else if (!Using && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer." + t.tag))
        {
            Anim.Play("Base Layer." + t.tag);

            switch (t.tag)
            {
                case "Eat":
                    Stats.hunger += 10;
                    break;
                case "Sleep":
                    // Reset fatigue and stamina
                    // Clear the hapiness modifiers
                    Stats.fatigue = 100;
                    Stats.stamina = 100;
                    Stats.hModifiers.Clear();
                    break;
                case "Work":
                    Stats.boredom += 10;
                    break;
                case "Recreational":
                    Stats.social += 10;
                    break;
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

public class Output
{
    public Output()
    {
        ID = 0;
        Value = 0;
    }

    public int ID;
    public float Value;
}

