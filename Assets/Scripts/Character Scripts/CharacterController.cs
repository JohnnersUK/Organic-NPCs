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

    // Neural network
    public NeuralNetwork MasterNetwork;

    public string[] Inputs;
    public int[] HiddenLayers;
    public int Outputs;

    public List<Output> Results;

    enum State
    {
        Idle = 0,
        Combat = 1,
        Dead = 2
    }
    State currentState = State.Idle;

    public void Start()
    {
        MainCam = Camera.main;
        AIAgent = GetComponent<NavMeshAgent>();
        TPController = GetComponent<ThirdPersonCharacter>();
        Anim = GetComponent<Animator>();
        Stats = GetComponent<CharacterStats>();

        CController = GetComponent<CombatController>();
        NController = GetComponent<NeedsController>();

        AIAgent.updateRotation = false;

        // Initilize the neural network
        int length = HiddenLayers.Length + 2;
        int[] layout = new int[length];                 // Length of the NN = num of hidden layers + 2

        layout[0] = Inputs.Length;                      // First layer is the input layer

        // Initilize the hidden layer
        for (int i = 0; i < HiddenLayers.Length; i++)   // For each hidden layer
        {
            layout[i + 1] = HiddenLayers[i];            // Set the number of nodes
        }

        layout[layout.Length - 1] = Outputs;            // Last layer is the output layer

        MasterNetwork = new NeuralNetwork(layout);       // Construct the NN
    }


    void Update()
    {
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
        // Run the NN
        float[] inputs = Stats.GetStats(Inputs);            // Update the inputs
        float[] _outputs = MasterNetwork.Run(inputs);       // Pass them through the NN

        // Evaluate the NN
        Results = new List<Output>();                       // Create the list of results
        for (int i = 0; i < Outputs; i++)
        {
            Output t = new Output();
            t.ID = i;
            t.Value = _outputs[i];
            Results.Add(t);
        }
        Results.Sort();                                     // Sort the list of results

        currentState = (State)Results[Results.Count-1].ID;  // Set the result

        // Run the current state
        switch (currentState)
        {
            case State.Idle: // If idle, run the idle loop
                {
                    if (Anim.GetBool("InCombat"))
                        Anim.SetBool("InCombat", false);
                    NController.Run();
                    break;
                }
            case State.Combat: // If in combat, run the combat loop
                {
                    if (!Anim.GetBool("InCombat"))
                        Anim.SetBool("InCombat", true);
                    if (GetTarget())
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

        NController.NeedsNetwork.AddFitness(Stats.happiness * Time.deltaTime);
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