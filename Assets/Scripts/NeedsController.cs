using System.Collections;

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

    // Components
    private Animator Anim;
    private CharacterStats Stats;
    private NavMeshAgent AIAgent;

    private GameObject target;

    private int output = 0;
    private float count = 0;
    private string actionTag;

    private bool Using = false;

    private Stack ActionQueue = new Stack();

    private void Start()
    {
        // Get components
        Anim = GetComponent<Animator>();
        Stats = GetComponent<CharacterStats>();
        AIAgent = GetComponent<NavMeshAgent>();

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
        float newDistance = 0;
        float targetDistance = 1000;

        GameObject[] objects;

        // Run the NN
        float[] inputs = Stats.GetStats(Inputs);        // Update the inputs
        float[] results = NeedsNetwork.Run(inputs);    // Pass them through the NN

        // Evaluate the NN
        float outputTotal = -2;

        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] > outputTotal)
            {
                output = i;
                outputTotal = results[i];
            }
        }

        // Set the action tag to the activity type
        switch (output)
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
        objects = GameObject.FindGameObjectsWithTag(actionTag);
        target = null;
        foreach (GameObject element in objects)
        {
            if (!element.GetComponent<Interactable>().Occupied)
            {
                newDistance = Vector3.Distance(element.transform.position, this.transform.position); // Compare distance
                if (newDistance < targetDistance)
                {
                    targetDistance = newDistance;
                    target = element;
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
        }

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
            AIAgent.isStopped = true;
            ActionQueue.Pop();
        }
    }

    private void Use(GameObject t)
    {
        GetComponentInChildren<Text>().text = t.tag + "ing";

        // Make the bot look at the object its using
        Vector3 lookPos = t.transform.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);

        if (!Using)
        {
            Anim.Play("Base Layer." + t.tag);
            
            switch (t.tag)
            {
                case "Eat":
                    Stats.hunger += 10;
                    NeedsNetwork.AddFitness(1);
                    break;
                case "Sleep":
                    Stats.fatigue += 10;
                    NeedsNetwork.AddFitness(1);
                    break;
                case "Work":
                    Stats.boredom += 10;
                    NeedsNetwork.AddFitness(10 * Stats.fitnessMultiplier);
                    break;
                case "Recreational":
                    Stats.social += 10;
                    NeedsNetwork.AddFitness(1);
                    break;
            }

            Using = true;
        }

        if (!Anim.GetBool("Using"))
        {
            ActionQueue.Pop();
            Using = false;
            t.GetComponent<Interactable>().Occupied = false;
        }
    }
}

