using System;
using System.Collections.Generic;

[Serializable]
public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] Layers;

    private float[][] Neurons;
    private float[][][] Weights;
    public float Fitness;

    public string Name;

    // Initilize a new network
    public NeuralNetwork(int[] layout, string n = "Default")
    {
        List<float[]> neurons;
        List<float[][]> weights;
        List<float[]> layerWeights;

        int NiP;

        float[] neuronWeights;

        // Initialize the layout
        Layers = new int[layout.Length];
        for (int i = 0; i < Layers.Length; i++)
        {
            Layers[i] = layout[i];
        }

        // Initilize the neurons
        neurons = new List<float[]>();

        for (int i = 0; i < Layers.Length; i++)
        {
            neurons.Add(new float[Layers[i]]);
        }

        //convert list to array for ease of use
        Neurons = neurons.ToArray();

        // Initilize the Weights
        weights = new List<float[][]>();

        for (int i = 1; i < Layers.Length; i++)
        {
            layerWeights = new List<float[]>();

            // Get the neurons in the previous layer
            NiP = Layers[i - 1];

            for (int j = 0; j < Neurons[i].Length; j++)
            {
                // Randomize their weights
                neuronWeights = new float[NiP];

                for (int k = 0; k < NiP; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-1f, 1f);
                }

                layerWeights.Add(neuronWeights);
            }

            weights.Add(layerWeights.ToArray());
        }

        //convert list to array for ease of use
        Weights = weights.ToArray();

        Name = n;
    }


    // Run data through the NN
    public List<Output> Run(float[] inputs)
    {
        float value;
        float[] rawOutputs;

        Output temp;
        List<Output> outputs;

        // Pass inputs into first layer of the network
        for (int i = 0; i < inputs.Length; i++)
        {
            Neurons[0][i] = inputs[i];
        }

        // Run through the network
        for (int i = 1; i < Layers.Length; i++)
        {
            for (int j = 0; j < Neurons[i].Length; j++)
            {
                value = 0f;

                for (int k = 0; k < Neurons[i - 1].Length; k++)
                {
                    // Sum off all connections to this neuron by weight
                    value += Weights[i - 1][j][k] * Neurons[i - 1][k];
                }

                // Tanh activation for simplistic classification
                Neurons[i][j] = (float)Math.Tanh(value);
            }
        }
        // Store the last layer as raw outputs
        rawOutputs = Neurons[Neurons.Length - 1];

        // Process the outputs for ease of use
        outputs = new List<Output>();
        for (int i = 0; i < rawOutputs.Length; i++)
        {
            temp = new Output
            {
                ID = i,
                Value = rawOutputs[i]
            };
            outputs.Add(temp);
        }

        // Return the outputs
        return outputs;
    }


    // Mutates the networks weights
    public void Mutate()
    {
        float currentWeight;

        // Run through each weight
        for (int i = 0; i < Weights.Length; i++)
        {
            for (int j = 0; j < Weights[i].Length; j++)
            {
                for (int k = 0; k < Weights[i][j].Length; k++)
                {
                    // Get the current weight value
                    currentWeight = Weights[i][j][k];

                    // Pick a random number
                    switch (UnityEngine.Random.Range(0, 4))
                    {
                        default:
                        case 0:
                        {
                            // Randomize a new weight
                            currentWeight = UnityEngine.Random.Range(-1f, 1f);
                            break;
                        }
                        case 1:
                        {
                            // Change the sign of the weight
                            currentWeight *= -1f;
                            break;
                        }
                        case 3:
                        {
                            // Add or take away 0-50% of the weight
                            float change = UnityEngine.Random.Range(0.5f, 1.5f);
                            currentWeight *= change;
                            break;
                        }
                    }

                    // Set the new weight value
                    Weights[i][j][k] = currentWeight;
                }
            }
        }
    }

    // Compares the fitness of this network to the next
    public int CompareTo(NeuralNetwork other)
    {
        if (other == null)
        {
            return 1;
        }
        else
        {
            return GetFitness().CompareTo(other.GetFitness());
        }
    }

    // Gets the fitness
    public float GetFitness()
    {
        return Fitness;
    }

    // Adds fitness
    public void AddFitness(float val)
    {
        Fitness += val;
    }

    // Sets the weights
    public void SetWeights(float[][][] w)
    {
        Weights = w;
    }

    // Gets the weights
    public float[][][] GetWeights()
    {
        return Weights;
    }


    // Gets the neurons
    public float[][] GetNeurons()
    {
        return Neurons;
    }
}
