using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkTrainingScript : MonoBehaviour
{
    enum Networks
    {
        NeedsNetwork = 0,
        CombatNetwork = 1,
        MasterNetwork = 2
    }

    // Public
    public static NetworkTrainingScript Instance { get; private set; }

    public List<AiBehaviour> _Behaviours { get; private set; }

    [SerializeField] private bool intensiveTraining = false;
    [SerializeField] private int itCount = 0;

    [Header("Simulation Settings:")]
    [SerializeField] Networks trainingNetwork = 0;

    [SerializeField] private int Simulations = 0;

    [Range(1, 20)]
    [SerializeField] private float simulationSpeed = 1.0f;

    [SerializeField] private float simulationTime = 300;

    [SerializeField] private GameObject botPrefab = null;
    [SerializeField] private GameObject nodePrefab = null;

    [Header("Storage:")]
    [SerializeField] private string fileName = "data.json";
    [SerializeField] private bool saveWeights = false;

    // Private
    private Transform[] spawnPoints = null;
    [SerializeField] public GameObject[] Bots { get; private set; }
    [SerializeField] private GameObject[] nodes;

    private Interactable[] objects;

    private float count;

    private List<float[][][]> tempWeights = new List<float[][][]>();

    // Use this for initialization
    public void Start()
    {
        GameObject interactables;
        GameObject tsp;

        DontDestroyOnLoad(gameObject);
        foreach (NetworkTrainingScript im in FindObjectsOfType<NetworkTrainingScript>())
        {
            if (im != this)
            {
                Destroy(gameObject);
            }
        }

        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;

        int i = 0;

        interactables = GameObject.FindGameObjectWithTag("interactables");
        objects = interactables.GetComponentsInChildren<Interactable>();

        count = simulationTime;

        Time.timeScale = simulationSpeed;

        tsp = GameObject.FindGameObjectWithTag("SpawnPoints");
        spawnPoints = tsp.GetComponentsInChildren<Transform>();

        Bots = new GameObject[spawnPoints.Length];
        _Behaviours = new List<AiBehaviour>();

        foreach (Transform T in spawnPoints)
        {
            // Initilize the bots and assign a faction
            Bots[i] = Instantiate(botPrefab, T.position, T.rotation);
            Bots[i].GetComponent<CharacterStats>().faction = (Factions)Random.Range(0, 3);

            switch (trainingNetwork)
            {
                case Networks.CombatNetwork:
                _Behaviours.Add(Bots[i].GetComponent<CombatController>());
                Bots[i].GetComponent<CombatController>().Start();
                break;
                case Networks.MasterNetwork:
                _Behaviours.Add(Bots[i].GetComponent<AgentController>());
                Bots[i].GetComponent<AgentController>().Start();
                break;
                case Networks.NeedsNetwork:
                _Behaviours.Add(Bots[i].GetComponent<NeedsController>());
                Bots[i].GetComponent<NeedsController>().Start();
                break;
            }

            // name and color the bot to make it identifiable
            Bots[i].name = "bot " + i + " " + Bots[i].GetComponent<CharacterStats>().faction.ToString();
            foreach (SkinnedMeshRenderer smr in Bots[i].GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (smr.material.color == new Color(0.09657001f, 0.4216198f, 0.522f, 1))
                {
                    switch (Bots[i].GetComponent<CharacterStats>().faction)
                    {
                        case Factions.Neutral:
                        {
                            // Random green
                            smr.material.color = Random.ColorHSV(0.22f, 0.38f, 0.2f, 0.8f, 0.2f, 0.8f);
                            break;
                        }
                        case Factions.Barbarians:
                        {
                            // Random red
                            smr.material.color = Random.ColorHSV(0f, 0.08f, 0.2f, 0.8f, 0.2f, 0.8f);
                            break;
                        }
                        case Factions.Guards:
                        {
                            // Random blue
                            smr.material.color = Random.ColorHSV(0.55f, 0.72f, 0.2f, 0.8f, 0.2f, 0.8f);
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
                nodes[i].GetComponent<TrainingController>().Start();

                _Behaviours.Add(nodes[i].GetComponent<TrainingController>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        fileName = trainingNetwork.ToString() + ".nn";
        Time.timeScale = simulationSpeed;

        // If a number of simulations are set
        if (Simulations > 0)
        {
            // Begin simulating
            count -= 1 * Time.deltaTime;

            // If simulation time is up
            if (count <= 0)
            {
                Simulations--;

                // If there are still simulations remaining
                if (Simulations != 0)
                {
                    StartNewRound();

                    if (intensiveTraining)
                    {
                        TrainNodes();
                    }
                }
                else
                {
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    Application.Quit();
                    #endif
                }
            }
        }
    }

    void StartNewRound()
    {
        int i, k;
        List<NeuralNetwork> networks = new List<NeuralNetwork>();
        tempWeights = new List<float[][][]>();

        EventManager.instance.OnRoundStart();

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

        for (i = Bots.Length - 1; i >= 0; i--)
        {
            Destroy(Bots[i]);
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

        EventManager.instance.Start();

        Debug.Log("Begining simulation");
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Start();
    }

    void TrainNodes()
    {
        CharacterStats s;
        AiBehaviour ab;

        List<Output> Results;

        foreach (GameObject n in nodes)
        {
            // Get necesary components
            s = n.GetComponent<CharacterStats>();
            ab = n.GetComponent<AiBehaviour>();

            // Force run the neural network with random stats
            s.Randomize();
            ab.Update();

            // Evaluate the outputs
            Results = ab.Results;

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
                if (s.GetStat("anger") > 0)
                {
                    ab.Network.AddFitness(-1);
                }
                else
                {
                    ab.Network.AddFitness(1);
                }

                break;
            }
            // Combat
            case 1:
            {
                if (s.GetStat("anger") > 0)
                {
                    ab.Network.AddFitness(1);
                }
                else
                {
                    ab.Network.AddFitness(-1);
                }

                break;
            }
        }
    }

    void EvaluateNeeds(CharacterStats s, AiBehaviour ab, List<Output> Results)
    {
        ab.Network.AddFitness(s.GetStat("happiness"));
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
                if (s.GetStat("health") <= s.maxHealth * 0.1)
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
                if (s.GetStat("stamina") < 2)
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