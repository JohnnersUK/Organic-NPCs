using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private int output = 0;
    private float count = 0;

    private void Start()
    {
        // Get components
        Anim = GetComponent<Animator>();
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

        NeedsNetwork = new NeuralNetwork(layout); // Construct the NN
    }


    public void Run()
    {
        count += Time.deltaTime * 1;

        if (count > 1)
        {
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

            switch (output)
            {
                // Eat
                case 0:
                    break;
                // Sleep
                case 1:
                    break;
                // Work
                case 2:
                    break;
                // Recreational
                case 3:
                    break;
            }

            count = 0;
        }


    }
}

