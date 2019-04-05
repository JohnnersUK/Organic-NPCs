using System.Collections.Generic;
using System.IO;
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

    public bool intensiveTraining = false;
    public int itCount = 0;

    [Header("Simulation Settings:")]
    public Networks trainingNetwork;

    public int simultaionNum;

    [Range(1, 20)]
    public float simulationSpeed = 1.0f;

    public float simulationTime = 300;

    public GameObject botPrefab;
    public GameObject nodePrefab;

    [Header("Storage:")]
    public string fileName = "data.json";
    public bool loadWeights = false;
    public bool saveWeights = false;

    // Private
    private Transform[] spawnPoints;
    public GameObject[] bots;
    public GameObject[] nodes;

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
                    bots[i].GetComponent<CombatController>().Start();
                    break;
                case Networks.MasterNetwork:
                    _Behaviours.Add(bots[i].GetComponent<AgentController>());
                    bots[i].GetComponent<AgentController>().Start();
                    break;
                case Networks.NeedsNetwork:
                    _Behaviours.Add(bots[i].GetComponent<NeedsController>());
                    bots[i].GetComponent<NeedsController>().Start();
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

        // Intensive training
        if (intensiveTraining)
        {
            nodes = new GameObject[itCount];
            for (i = 0; i < itCount; i++)
            {
                // Init the training node
                nodes[i] = Instantiate(nodePrefab, new Vector3(-10, -10, -10), new Quaternion());
                nodes[i].name = "Training Node " + i;
                AiBehaviour ab = nodes[i].GetComponent<AiBehaviour>();

                ab.Inputs = _Behaviours[0].Inputs;
                ab.HiddenLayers = _Behaviours[0].HiddenLayers;
                ab.Outputs = _Behaviours[0].Outputs;

                nodes[i].GetComponent<AiBehaviour>().Start();

                _Behaviours.Add(nodes[i].GetComponent<AiBehaviour>());

            }
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

        if (intensiveTraining)
        {
            TrainNodes();
        }
    }

    void StartNewRound()
    {
        EventManager.instance.OnRoundStart();

        int i, k;
        List<NeuralNetwork> networks = new List<NeuralNetwork>();
        tempWeights = new List<float[][][]>();

        foreach (AiBehaviour b in _Behaviours)
        {
            networks.Add(b.Network);
        }

        // Order the networks
        networks.Sort();

        // Save the weights
        if (saveWeights)
        {
            saveWeights = false;
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            NetworkIO.instance.SerializeObject<NeuralNetwork>(filePath, networks[networks.Count - 1]);
            Debug.Log("Top Weights saved");
        }

        Debug.Log("Results:");
        // print and store the fitness, then destroy the bots
        i = 0;
        for (k = networks.Count - 1; k >= 0; k--)
        {
            Debug.Log(networks.Count - k + ") " + networks[k].name + ": " + networks[k].GetFitness());
            tempWeights.Add(networks[k].GetWeights());
        }

        for(i = bots.Length-1; i >= 0; i--)
        {
            Destroy(bots[i]);
        }

        for (i = nodes.Length - 1; i >= 0; i--)
        {
            Destroy(nodes[i]);
        }

        _Behaviours.Clear();

        // Create a new set of mutated bots using the top 5 previous fitnesses
        Start();

        k = 0;
        foreach (AiBehaviour b in _Behaviours)
        {
            b.Start();
            b.Network.SetWeights(tempWeights[k]);
            b.Network.Mutate();
            k++;
            if (k == 5)
            {
                k = 0;
            }
        }

        // Reset the interactables
        for (i = 0; i < objects.Length; i++)
        {
            objects[i].Occupied = false;
        }

        count = simulationTime;
        simultaionNum++;

        EventManager.instance.Start();

        Debug.Log("Begining simulation " + simultaionNum);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Start();
    }

    void TrainNodes()
    {
        CharacterStats s;
        AiBehaviour ab;
        foreach (GameObject n in nodes)
        {
            // Get necesary components
            s = n.GetComponent<CharacterStats>();
            ab = n.GetComponent<AiBehaviour>();

            // Force run the neural network with random stats
            s.Randomize();
            ab.Update();

            // Evaluate the outputs
            List<Output> Results = ab.Results;

            Results.Sort();

            // Award fitness
            switch (trainingNetwork)
            {
                case Networks.CombatNetwork:
                    EvaluateCombat(s, ab, Results);
                    break;
                case Networks.NeedsNetwork:
                    EvaluateNeeds(s, ab, Results);
                    break;
                case Networks.MasterNetwork:
                    EvaluateMaster(s, ab, Results);
                    break;
            }
        }
    }

    void EvaluateMaster(CharacterStats s, AiBehaviour ab, List<Output> Results)
    {
        switch (Results[Results.Count - 1].ID)
        {
            // Idle
            case 0:
                {
                    if (s.anger > 0)
                        ab.Network.AddFitness(-1);
                    else
                        ab.Network.AddFitness(1);

                    break;
                }
            // Combat
            case 1:
                {
                    if (s.anger > 0)
                        ab.Network.AddFitness(1);
                    else
                        ab.Network.AddFitness(-1);

                    break;
                }
        }
    }

    void EvaluateNeeds(CharacterStats s, AiBehaviour ab, List<Output> Results)
    {
        ab.Network.AddFitness(s.happiness);
    }

    void EvaluateCombat(CharacterStats s, AiBehaviour ab, List<Output> Results)
    {
        
        switch (Results[Results.Count - 1].ID)
        {
            // Attack
            case 0:
                {
                    // If the bot is attacking give them a point
                    ab.Network.AddFitness(5);
                    break;
                }
            // Dodge
            case 1:
                {
                    // If the bot is dodging and low on hp give them 5 points
                    if (s.health <= s.maxHealth * 0.1)
                    {
                        ab.Network.AddFitness(5);
                    }
                    else // Otherwise take away a point
                    {
                        ab.Network.AddFitness(-10);
                    }
                    break;
                }
            // Wait
            case 2:
                {
                    // If the bot is low on stamina and waiting gove them 5 points
                    if (s.stamina < 2)
                    {
                        ab.Network.AddFitness(5);
                    }
                    else  // Otherwise take away a point
                    {
                        ab.Network.AddFitness(-10);
                    }
                    break;
                }
        }
    }
}