using UnityEngine;
using UnityEngine.AI;

public class CombatController : MonoBehaviour
{
    // Neural network
    public string[] Inputs;
    public int[] HiddenLayers;
    public int Outputs;

    // Components
    private NeuralNetwork CombatNetwork;
    private Animator Anim;
    private NavMeshAgent AIAgent;
    private GameObject CombatTarget;
    private CharacterStats Stats;

    private void Start()
    {
        // Get components
        Anim = GetComponent<Animator>();
        AIAgent = GetComponent<NavMeshAgent>();
        Stats = GetComponent<CharacterStats>();

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

        CombatNetwork = new NeuralNetwork(layout); // Construct the NN
    }

    // Update is called once per frame
    public void Run (GameObject target)
    {
        CombatTarget = target;
        LookAtTarget();

        // Run the NN
        float[] inputs = Stats.GetStats(Inputs);        // Update the inputs
        float[] results = CombatNetwork.Run(inputs);    // Pass them through the NN

        // Evaluate the NN
        int output = 0;
        float outputTotal = -2;

        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] > outputTotal)
            {
                output = i;
                outputTotal = results[i];
            }
        }

        switch(output)
        {
            case 0:
                {
                    Attack();
                    break;
                }
            default:
            case 1:
            case 2:
                {
                    Debug.Log("nah");
                    break;
                }
        }

        //Debug moveTo, not working
        //if (!InRange())
        //{
        //    AIAgent.SetDestination(this.transform.position + this.transform.forward);
        //    AIAgent.isStopped = false;
        //}

        // Debug attack
        if (Input.GetMouseButtonDown(1))
        {
            Attack();
        }
    }

    void Attack()
    {
        Anim.SetInteger("AttackType", Random.Range(0, 8));
    }

    // Check if the combat target is in range
    bool InRange()
    {
        if (Vector3.Distance(CombatTarget.transform.position, this.transform.position) > Stats.table["range"])
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // Look at the combat target
    void LookAtTarget()
    {
        transform.LookAt(CombatTarget.transform.position);
    }
}
