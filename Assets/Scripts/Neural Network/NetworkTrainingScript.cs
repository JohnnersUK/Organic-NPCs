using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkTrainingScript : MonoBehaviour
{
    // Public
    public enum Networks
    {
        NeedsNetwork = 0,
        CombatNetwork = 1,
        MasterNetwork = 2
    }
    private List<AiBehaviour> _Behaviours;

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
    public GameObject[] bots;

    private Interactable[] objects;

    private float count;

    private List<float[][][]> tempWeights = new List<float[][][]>();

    // Use this for initialization
    public void Start()
    {
        DontDestroyOnLoad(gameObject);
        foreach (NetworkTrainingScript im in FindObjectsOfType<NetworkTrainingScript>())
        {
            if (im != this)
            {
                Destroy(gameObject);
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        int i = 0;

        simultaionNum = 1;

        GameObject interactables = GameObject.FindGameObjectWithTag("interactables");
        objects = interactables.GetComponentsInChildren<Interactable>();

        count = simulationTime;

        Time.timeScale = simulationSpeed;

        GameObject tsp = GameObject.FindGameObjectWithTag("SpawnPoints");
        spawnPoints = tsp.GetComponentsInChildren<Transform>();

        bots = new GameObject[spawnPoints.Length];
        _Behaviours = new List<AiBehaviour>();

        foreach (Transform T in spawnPoints)
        {
            // Initilize the bots and assign a faction
            bots[i] = Instantiate(botPrefab, T.position, T.rotation);
            bots[i].GetComponent<CharacterStats>().faction = (Factions)Random.Range(0, 3);

            switch (trainingNetwork)
            {
                case Networks.CombatNetwork:
                    _Behaviours.Add(bots[i].GetComponent<CombatController>());
                    break;
                case Networks.MasterNetwork:
                    _Behaviours.Add(bots[i].GetComponent<CharacterController>());
                    break;
                case Networks.NeedsNetwork:
                    _Behaviours.Add(bots[i].GetComponent<NeedsController>());
                    break;
            }

            // name and color the bot to make it identifiable
            bots[i].name = "bot " + i + " " + bots[i].GetComponent<CharacterStats>().faction.ToString();
            foreach (SkinnedMeshRenderer smr in bots[i].GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (smr.material.color == new Color(0.09657001f, 0.4216198f, 0.522f, 1))
                {
                    switch (bots[i].GetComponent<CharacterStats>().faction)
                    {
                        case Factions.Neutral:
                            {
                                smr.material.color = Color.white;
                                break;
                            }
                        case Factions.Barbarians:
                            {
                                smr.material.color = Color.red;
                                break;
                            }
                        case Factions.Guards:
                            {
                                smr.material.color = Color.blue;
                                break;
                            }
                    }   
                }
            }
            i++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        fileName = trainingNetwork.ToString() + ".nn";
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

        foreach(AiBehaviour b in _Behaviours)
        {
            networks.Add(b.Network);
        }

        // Order the networks
        networks.Sort();

        // Save the weights
        if (saveWeights)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            NetworkIO.instance.SerializeObject<NeuralNetwork>(filePath, networks[networks.Count-1]);

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
        _Behaviours.Clear();

        // Create a new set of mutated bots using the top 5 previous fitnesses
        Start();

        k = 0;
        foreach(AiBehaviour b in _Behaviours)
        {
            b.Start();
            b.Network.SetWeights(tempWeights[k]);
            b.Network.Mutate();
            k++;
            if (k == tempWeights.Count) k = 0;
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

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Start();
    }
}