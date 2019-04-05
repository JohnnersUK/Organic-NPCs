using System;
using System.Collections.Generic;

[Serializable]
public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] _layers;
    private float[][] _neurons;
    private float[][][] _weights;

    public float fitness;

    public string name;

    // Initilize a new network
    public NeuralNetwork(int[] layout, string n = "Default")
    {
        List<float[]> neurons;
        List<float[][]> weights;
        List<float[]> layerWeights;

        int NiP;

        float[] neuronWeights;

        _layers = new int[layout.Length];
        for (int i = 0; i < _layers.Length; i++)
        {
            _layers[i] = layout[i];
        }

        // Initilize the neurons
        neurons = new List<float[]>();

        for (int i = 0; i < _layers.Length; i++)
        {
            neurons.Add(new float[_layers[i]]);
        }

        //convert list to array for ease of use
        _neurons = neurons.ToArray();

        // Initilize the Weights
        weights = new List<float[][]>();

        for (int i = 1; i < _layers.Length; i++)
        {
            layerWeights = new List<float[]>();

            // Get the neurons in the previous layer
            NiP = _layers[i - 1];

            for (int j = 0; j < _neurons[i].Length; j++)
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
        _weights = weights.ToArray();

        name = n;
    }


    // Run data through the NN
    public List<Output> Run(float[] inputs)
    {
        float value;
        float[] rawOutputs;

        Output temp;
        List<Output> outputs;

        // pass inputs into the network
        for (int i = 0; i < inputs.Length; i++)
        {
            _neurons[0][i] = inputs[i];
        }

        // run through the network
        for (int i = 1; i < _layers.Length; i++)
        {
            for (int j = 0; j < _neurons[i].Length; j++)
            {
                value = 0f;

                for (int k = 0; k < _neurons[i - 1].Length; k++)
                {
                    // Sum off all weights connections of this neuron weight their values in previous layer
                    value += _weights[i - 1][j][k] * _neurons[i - 1][k];
                }

                // Tanh activation for simplistic classification
                _neurons[i][j] = (float)Math.Tanh(value);
            }
        }
        rawOutputs = _neurons[_neurons.Length - 1];

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


    // Mutate the weights
    public void Mutate()
    {
        int rand;

        for (int i = 0; i < _weights.Length; i++)
        {
            for (int j = 0; j < _weights[i].Length; j++)
            {
                for (int k = 0; k < _weights[i][j].Length; k++)
                {
                    float currentWeight = _weights[i][j][k];

                    rand = UnityEngine.Random.Range(0, 4);
                    switch (rand)
                    {
                        default:
                        case 0:
                        {
                            //Reroll the weight
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

                    // Set the new weight
                    _weights[i][j][k] = currentWeight;
                }
            }
        }
    }

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

    public void SetWeights(float[][][] w)
    {
        _weights = w;
    }


    public float[][][] GetWeights()
    {
        return _weights;
    }


    public void AddFitness(float val)
    {
        fitness += val;
    }


    public float GetFitness()
    {
        return fitness;
    }

    public float[][] GetNeurons()
    {
        return _neurons;
    }
}
