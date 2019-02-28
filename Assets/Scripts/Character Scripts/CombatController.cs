using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CombatController : MonoBehaviour
{
    // Neural network
    public NeuralNetwork CombatNetwork;

    public string[] Inputs;

    public int[] HiddenLayers;
    public int Outputs;

    public float AttackCost;
    public float DodgeCost;

    // Components
    private Animator Anim;
    private NavMeshAgent AIAgent;
    private GameObject CombatTarget;
    private CharacterStats Stats;

    private int output = 0;
    private float count = 0;

    public void Start()
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


    public void Run(GameObject target)
    {
        CombatTarget = target;
        LookAtTarget();

        count += Time.deltaTime * 1;

        if (count > 1)
        {
            // Run the NN
            float[] inputs = Stats.GetStats(Inputs);        // Update the inputs
            float[] results = CombatNetwork.Run(inputs);    // Pass them through the NN

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

            switch (output)
            {
                case 0:
                    {
                        if (!InRange())
                        {
                            GetComponentInChildren<Text>().text = "Moving to target";
                            AIAgent.SetDestination(this.transform.position + this.transform.forward);
                            AIAgent.isStopped = false;
                        }
                        else
                        {
                            if (Stats.GetStat("stamina") > 0)
                            {
                                GetComponentInChildren<Text>().text = "Attacking";
                                AIAgent.isStopped = true;
                                Attack();
                                Stats.stamina -= AttackCost;
                            }
                            else
                            {
                                GetComponentInChildren<Text>().text = "Waiting for stamina";
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        if (Stats.GetStat("stamina") > 0)
                        {
                            GetComponentInChildren<Text>().text = "Dodging";
                            Anim.SetInteger("Dodge", Random.Range(-5, 5));
                            Stats.stamina -= DodgeCost;
                        }
                        else
                        {
                            GetComponentInChildren<Text>().text = "Waiting for stamina";
                        }
                        break;
                    }
                case 2:
                    {
                        GetComponentInChildren<Text>().text = "Waiting";
                        break;
                    }
            }

            count = 0;
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
        Vector3 lookPos = CombatTarget.transform.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);
    }
}
