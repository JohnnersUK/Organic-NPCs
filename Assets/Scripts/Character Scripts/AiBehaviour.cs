﻿using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class AiBehaviour : MonoBehaviour
{
    // Neural network
    public NeuralNetwork Network;

    public string[] Inputs;

    public int[] HiddenLayers;
    public int Outputs;

    public List<Output> Results;
    protected string filePath;
    // Components
    protected CharacterStats Stats;
    public float[] _outputs;

    public virtual void Start()
    {
        th
        Stats = GetComponent<CharacterStats>();
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

        if (filePath != null && File.Exists(filePath))
        {
            Debug.Log("Loading in network from file: " + filePath);
            Network = NetworkIO.instance.DeSerializeObject<NeuralNetwork>(filePath);
            Debug.Log("Loading complete");
            Network.name = gameObject.name;
        }
        else
        {
            Debug.Log("Generating new network");
            Network = new NeuralNetwork(layout, gameObject.name);       // Construct the NN
        }
        if (true == true)
        {

        }
    }

    public virtual void Update()
    {
        // Run the NN
        float[] inputs = Stats.GetStats(Inputs);        // Update the inputs
        _outputs = Network.Run(inputs);             // Pass them through the NN
    }


    public virtual void Run()
    {

    }
}