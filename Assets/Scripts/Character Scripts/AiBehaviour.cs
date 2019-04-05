using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class AiBehaviour : MonoBehaviour
{
    // Neural network
    public NeuralNetwork Network { get; private set; }

    // Public for use in inspector
    public string[] Inputs;
    public int[] HiddenLayers;
    public int Outputs;
    //

    public List<Output> Results { get; protected set; }

    protected string FilePath;

    // Components
    protected CharacterStats Stats;

    public virtual void Start()
    {
        int length;
        int[] layout;

        Stats = GetComponent<CharacterStats>();

        if (FilePath != null && File.Exists(FilePath))
        {
            // If a binary network file is found, load it
            Network = NetworkIO.instance.DeSerializeObject<NeuralNetwork>(FilePath);
            Network.name = gameObject.name;
        }
        else
        {
            // In not, Initilize a new neural network
            length = HiddenLayers.Length + 2;
            layout = new int[length];                 // Length of the NN = num of hidden layers + 2

            layout[0] = Inputs.Length;                      // First layer is the input layer

            // Initilize the hidden layer
            for (int i = 0; i < HiddenLayers.Length; i++)   // For each hidden layer
            {
                layout[i + 1] = HiddenLayers[i];            // Set the number of nodes
            }

            layout[layout.Length - 1] = Outputs;            // Last layer is the output layer

            Network = new NeuralNetwork(layout, gameObject.name);       // Construct the NN
        }
    }

    public virtual void Update()
    {
        float[] inputs;

        // Run the NN
        inputs = Stats.GetStats(Inputs);        // Update the inputs
        Results = Network.Run(inputs);             // Pass them through the NN
    }
}