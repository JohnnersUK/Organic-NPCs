using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

using UnityEngine;

public class NetworkTrainingScript : MonoBehaviour
{
    
    // Public
    public enum Networks
    {
        NeedsNetwork = 0,
        CombatNetwork = 1,
        MasterNetwork = 2
    }

    [Header("Simulation Settings:")]
    public Networks trainingNetwork;

    public int simultaionNum;

    [Range(1, 20)]
    public float simulationSpeed = 1.0f;

    public float simulationTime = 300;

    public GameObject botPrefab;

    [Header("Storage:")]
    public string fileName = "data.json";
    public bool loadWeights = false;
    public bool saveWeights = false;

    // Private
    private Transform[] spawnPoints;
    private GameObject[] bots;

    private Interactable[] objects;

    private float count;

    private List<float[][][]> tempWeights = new List<float[][][]>();

    // Use this for initialization
    void Start()
    {
        int i = 0;

        simultaionNum = 1;

        GameObject interactables = GameObject.FindGameObjectWithTag("interactables");
        objects = interactables.GetComponentsInChildren<Interactable>();

        count = simulationTime;

        Time.timeScale = simulationSpeed;

        GameObject tsp = GameObject.FindGameObjectWithTag("SpawnPoints");
        spawnPoints = tsp.GetComponentsInChildren<Transform>();

        bots = new GameObject[spawnPoints.Length];

        foreach (Transform T in spawnPoints)
        {
            // Initilize the bots
            bots[i] = Instantiate(botPrefab, T.position, T.rotation);

            // name and color the bot to make it identifiable
            bots[i].name = "bot " + i;
            foreach (SkinnedMeshRenderer smr in bots[i].GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (smr.material.color == new Color(0.09657001f, 0.4216198f, 0.522f, 1))
                {
                    smr.material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
                }
            }
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = simulationSpeed;

        count -= 1 * Time.deltaTime;
        if (count <= 0)
        {
            StartNewRound();
        }
    }

    void StartNewRound()
    {
        int i, k;
        List<NeuralNetwork> networks = new List<NeuralNetwork>();
        tempWeights = new List<float[][][]>();
        switch (trainingNetwork)
        {
            case Networks.CombatNetwork:
                {
                    foreach (GameObject b in bots)
                    {
                        networks.Add(b.GetComponent<CombatController>().CombatNetwork);
                    }
                    break;
                }
            case Networks.NeedsNetwork:
                {
                    foreach (GameObject b in bots)
                    {
                        networks.Add(b.GetComponent<NeedsController>().NeedsNetwork);
                    }
                    break;
                }
            case Networks.MasterNetwork:
                {
                    foreach (GameObject b in bots)
                    {
                        networks.Add(b.GetComponent<CharacterController>().MasterNetwork);
                    }
                    break;
                }
        }

        // Order the networks
        networks.Sort();

        // Save the weights
        if (saveWeights)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            SerializeObject<NeuralNetwork>(filePath, networks[networks.Count-1]);

            saveWeights = false;

            Debug.Log("Top Weights saved");
        }

        Debug.Log("Results:");
        // print and store the fitness, then destroy the bots
        for (k = 0; k < bots.Length; k++)
        {
            Debug.Log(bots[k].name + ": " + networks[k].GetFitness());

            if (k < 5)
            {
                tempWeights.Add(networks[-k + (networks.Count - 1)].GetWeights());
            }

            Destroy(bots[k]);
        }

        // Create a new set of mutated bots using the top 5 previous fitnesses
        for (k = 0; k < spawnPoints.Length; k++)
        {
            bots[k] = Instantiate(botPrefab, spawnPoints[k].position, spawnPoints[k].rotation);

            // name and color the bot to make it identifiable
            bots[k].name = "bot " + k;
            foreach (SkinnedMeshRenderer smr in bots[k].GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (smr.material.color == new Color(0.09657001f, 0.4216198f, 0.522f, 1))
                {
                    smr.material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
                }
            }
        }

        k = 0;
        switch (trainingNetwork)
        {
            case Networks.CombatNetwork:
                {
                    foreach (GameObject b in bots)
                    {
                        b.GetComponent<CombatController>().Start();
                        b.GetComponent<CombatController>().CombatNetwork.SetWeights(tempWeights[k]);
                        b.GetComponent<CombatController>().CombatNetwork.Mutate();
                        k++;
                        if (k == tempWeights.Count) k = 0;
                    }
                    break;
                }
            case Networks.NeedsNetwork:
                {
                    foreach (GameObject b in bots)
                    {
                        b.GetComponent<NeedsController>().Start();
                        b.GetComponent<NeedsController>().NeedsNetwork.SetWeights(tempWeights[k]);
                        b.GetComponent<NeedsController>().NeedsNetwork.Mutate();
                        k++;
                        if (k == tempWeights.Count) k = 0;
                    }
                    break;
                }
            case Networks.MasterNetwork:
                {
                    foreach (GameObject b in bots)
                    {
                        b.GetComponent<CharacterController>().Start();
                        b.GetComponent<CharacterController>().MasterNetwork.SetWeights(tempWeights[k]);
                        b.GetComponent<CharacterController>().MasterNetwork.Mutate();
                        k++;
                        if (k == tempWeights.Count) k = 0;
                    }
                    break;
                }
        }


        // Reset the interactables
        for (i = 0; i < objects.Length; i++)
        {
            objects[i].Occupied = false;
        }

        count = simulationTime;
        simultaionNum++;

        Debug.Log("Begining simulation " + simultaionNum);
    }

    // Save a network as a binary file
    public void SerializeObject<T>(string filename, T obj)
    {
        Stream stream = File.Open(filename, FileMode.Create);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(stream, obj);
        stream.Close();
    }

    // Load a network from a binary file
    public T DeSerializeObject<T>(string filename)
    {
        T objectToBeDeSerialized;
        Stream stream = File.Open(filename, FileMode.Open);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        objectToBeDeSerialized = (T)binaryFormatter.Deserialize(stream);
        stream.Close();
        return objectToBeDeSerialized;
    }
}