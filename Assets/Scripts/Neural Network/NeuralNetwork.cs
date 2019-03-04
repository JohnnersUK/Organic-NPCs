using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

using UnityEngine;

[Serializable]
public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;

    public float fitness; 


    // Initilize a new network
    public NeuralNetwork(int[] layout)
    {
        layers = new int[layout.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = layout[i];
        }

        // Initilize the neurons
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++) //run through the layers
        {
            neuronsList.Add(new float[layers[i]]); //add layer to neuron list
        }
        neurons = neuronsList.ToArray(); //convert list to array

        // Initilize the Weights
        List<float[][]> weightsList = new List<float[][]>();

        for (int i = 1; i < layers.Length; i++) // Run through the layers
        {
            List<float[]> layerWeightsList = new List<float[]>();

            int neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < neurons[i].Length; j++) // Run through neurons in current layer
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];

                for (int k = 0; k < neuronsInPreviousLayer; k++) // Run through neurons in previous layer
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f); // Give random weights
                }

                layerWeightsList.Add(neuronWeights); // Add neuron weights to layer weights
            }

            weightsList.Add(layerWeightsList.ToArray()); // Add layers weights into weights list
        }
        weights = weightsList.ToArray(); // Convert to array
    }


    // Run data through the NN
    public float[] Run(float[] inputs)
    {
        // Feed inputs into the NN
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        // Itterate over the network
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;

                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k]; // Sum off all weights connections of this neuron weight their values in previous layer
                }

                neurons[i][j] = (float)Math.Tanh(value); // Tanh activation
            }
        }

        return neurons[neurons.Length - 1]; // Return output layer
    }


    // Mutate weights
    public void Mutate()
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    int rnd = Mathf.RoundToInt(UnityEngine.Random.Range(0f, 4f));
                    switch (rnd)
                    {
                        default:    //Completely reroll the weight
                        case 0:
                            {
                                weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                                break;
                            }

                        case 1:     // Change the sign of the weight
                            {
                                weight *= -1f;
                                break;
                            }
                        case 3:     // Add or take away 0-50% of the weight
                            {
                                float change = UnityEngine.Random.Range(0.5f, 1.5f);
                                weight *= change;
                                break;
                            }
                    }

                    weights[i][j][k] = weight;
                }
            }
        }
    }

    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;
        else
        {
            return this.GetFitness().CompareTo(other.GetFitness());
        }
    }

    public void SetWeights(float[][][] w)
    {
        weights = w;
    }


    public float[][][] GetWeights()
    {
        return weights;
    }


    public void AddFitness(float val)
    {
        fitness += val;
    }


    public float GetFitness()
    {
        return fitness;
    }


    // Compare NNs
    public int CompareFitness(NeuralNetwork other)
    {
        // Error handling
        if (other == null)
        {
            Debug.Log("No NN to compare");
            return 1;
        }


        if (fitness > other.fitness) // If current NN is better fitted
        {
            return 1;
        }
        else if (fitness < other.fitness) // If other NN is better fitted
        {
            return -1;
        }
        else // If they are equal                    
        {
            return 0;
        }

    }

    public float[][] GetNeurons()
    {
        return neurons;
    }
}
