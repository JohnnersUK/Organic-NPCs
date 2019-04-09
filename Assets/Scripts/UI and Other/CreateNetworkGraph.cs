using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class CreateNetworkGraph : MonoBehaviour
{
    public static CreateNetworkGraph instance = null;

    [SerializeField] private AiBehaviour CurrentBehavior = null;
    [SerializeField] private Sprite NodeSprite = null;
    [SerializeField] private Font font = null;
    [SerializeField] private Text NetworkName = null;
    [SerializeField] private Text ToolTip = null;
    [SerializeField] private Text Fitness = null;

    [SerializeField] private GameObject[] InputLayer = null;
    [SerializeField] private GameObject[][] HiddenLayer = null;
    [SerializeField] private GameObject[] OutputLayer = null;

    [SerializeField] private Transform UIAnchor = null;

    [SerializeField] private int XOfset = 0;
    [SerializeField] private int YOfset = 0;

    [SerializeField] private int inputs = 0;
    [SerializeField] private int hiddenLayers = 0;
    [SerializeField] private int outputs = 0;

    private int CurrentBehaviorkID = 0;
    private bool ShowGraph = false;

    private void Start()
    {
        // Make sure this is the only instance, then set the instance
        DontDestroyOnLoad(gameObject);
        foreach (CreateNeedsGraph im in FindObjectsOfType<CreateNeedsGraph>())
        {
            if (im != this)
            {
                Destroy(gameObject);
            }
        }
        instance = this;
    }

    // Initializes the network graph
    void InitializeGraph()
    {
        int length;

        // Get the network layout
        inputs = CurrentBehavior.Inputs.Length;
        hiddenLayers = CurrentBehavior.HiddenLayers.Length;
        outputs = CurrentBehavior.Outputs;

        // Get the total number of nodes
        length = inputs + outputs;
        for (int i = 0; i < hiddenLayers; i++)
        {
            length += CurrentBehavior.HiddenLayers[i];
        }

        // Create the input layer
        InitInputLayer();

        // Create the (Not so) Hidden layer
        InitHiddenLayer();

        // Create the output layer
        InitOutputLayer();

        NetworkName.text = CurrentBehavior.gameObject.name;
    }

    void Update()
    {
        // If the graph is being diaplayed
        if(ShowGraph)
        {

            UpdateGraph();

            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                // Go back a network
                FindNewNetwork(-1);
            }

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                // Go forward a network
                FindNewNetwork(1);
            }

            if(Input.GetKeyUp(KeyCode.F1))
            {
                // Hide the graph
                NetworkName.text = "";
                Fitness.text = "";
                ToolTip.text = "[Press F1 to show Network Graph]";
                Destructor();
                ShowGraph = false;
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.F1))
            {
                // Show the graph
                ToolTip.text = "[Press F1 to hide Network Graph]";
                FindNewNetwork(0);
                NetworkName.text = CurrentBehavior.gameObject.name;
                Fitness.text = CurrentBehavior.Network.GetFitness().ToString();
                ;
                ShowGraph = true;
            }
        }
    }

    // Updates the values of the graph
    void UpdateGraph()
    {
        float bestValue = -2;
        int bestNode = 0;
        float[][] NodeValues = CurrentBehavior.Network.GetNeurons();

        // Get the values of the inputs
        for (int i = 0; i < inputs; i++)
        {
            InputLayer[i].GetComponentInChildren<Text>().text = NodeValues[0][i].ToString("F2");
        }

        // Get the values of the hidden layer
        for (int i = 0; i < CurrentBehavior.HiddenLayers.Length; i++)
        {
            for (int j = 0; j < CurrentBehavior.HiddenLayers[i]; j++)
            {
                HiddenLayer[i][j].GetComponentInChildren<Text>().text = NodeValues[i + 1][j].ToString("F2");
            }
        }

        // Get the outputs and sort them
        for (int i = 0; i < outputs; i++)
        {
            OutputLayer[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            OutputLayer[i].GetComponentInChildren<Text>().text = NodeValues[CurrentBehavior.HiddenLayers.Length + 1][i].ToString("F2");

            if (NodeValues[CurrentBehavior.HiddenLayers.Length + 1][i] > bestValue)
            {
                bestValue = NodeValues[CurrentBehavior.HiddenLayers.Length + 1][i];
                bestNode = i;
            }
        }

        // Highlight the best output green
        OutputLayer[bestNode].GetComponent<Image>().color = new Color(0, 1, 0, 1);

        // get the fitness
        Fitness.text = "Fitness: " + CurrentBehavior.Network.GetFitness().ToString();
    }

    // Finds a new network to display in the current scene
    // dir dictates wether to cycle forwards, backwards or to rebuild the current network graph
    void FindNewNetwork(int dir)
    {
        // Get a list of all current networks
        List<AiBehaviour> tempBehaviors = NetworkTrainingScript.Instance.GetBehaviours();

        // Destruct the current network
        Destructor();

        // Add the direction to the currentNetwork ID
        CurrentBehaviorkID += dir;
        if(CurrentBehaviorkID > tempBehaviors.Count-1)
        {
            CurrentBehaviorkID = 0;
        }
        else if (CurrentBehaviorkID < 0)
        {
            CurrentBehaviorkID = tempBehaviors.Count - 1;
        }

        // Using the new networkId, get the new network
        CurrentBehavior = tempBehaviors[CurrentBehaviorkID];

        // Initialize the graph with the new network
        InitializeGraph();
    }

    // Initializes the inputs
    void InitInputLayer()
    {
        int count = 1;

        Vector3 Anchor = UIAnchor.position;
        Vector3 newPos;
        GameObject v;
        Text t;

        InputLayer = new GameObject[inputs];
        for (int i = 0; i < inputs; i++)
        {
            // Initilize each node
            InputLayer[i] = new GameObject("Input " + i);
        }

        // Construct each node
        foreach (GameObject g in InputLayer)
        {
            // Set the default position
            g.transform.parent = transform;

            newPos = new Vector3(Anchor.x, Anchor.y + (YOfset * count), Anchor.z);
            g.transform.position = newPos;
            g.transform.localScale /= 2;

            // Add the circle sprite image
            g.AddComponent(typeof(Image));
            g.GetComponent<Image>().sprite = NodeSprite;

            // Add the text
            v = new GameObject("Value");
            v.transform.parent = g.transform;
            v.transform.position = g.transform.position;

            // Configure the text
            t = v.AddComponent(typeof(Text)) as Text;
            t.font = font;
            t.text = v.name;
            t.color = new Color(0, 0, 0, 1);
            t.alignment = TextAnchor.MiddleCenter;

            count++;
        }
    }

    // Initializes the hidden layer
    void InitHiddenLayer()
    {
        Vector3 Anchor = UIAnchor.position;
        Vector3 newPos;
        GameObject v;
        Text t;

        // Construct the '2D' (Jagged) array
        HiddenLayer = new GameObject[CurrentBehavior.HiddenLayers.Length][];
        for (int i = 0; i < CurrentBehavior.HiddenLayers.Length; i++)
        {
            HiddenLayer[i] = new GameObject[CurrentBehavior.HiddenLayers[i]];
        }

        for (int y = 0; y < CurrentBehavior.HiddenLayers.Length; y++)
        {
            for (int x = 0; x < CurrentBehavior.HiddenLayers[y]; x++)
            {
                HiddenLayer[y][x] = new GameObject("Hidden" + y + x);
                HiddenLayer[y][x].transform.parent = transform;

                newPos = new Vector3(Anchor.x + (XOfset * (y + 2)), Anchor.y + (YOfset * (x + 1)), Anchor.z);
                HiddenLayer[y][x].transform.position = newPos;
                HiddenLayer[y][x].transform.localScale /= 2;

                // Add the circle sprite image
                HiddenLayer[y][x].AddComponent(typeof(Image));
                HiddenLayer[y][x].GetComponent<Image>().sprite = NodeSprite;

                // Add the text
                v = new GameObject("Value");
                v.transform.parent = HiddenLayer[y][x].transform;
                v.transform.position = HiddenLayer[y][x].transform.position;

                // Configure the text
                t = v.AddComponent(typeof(Text)) as Text;
                t.font = font;
                t.text = v.name;
                t.color = new Color(0, 0, 0, 1);
                t.alignment = TextAnchor.MiddleCenter;
            }
        }
    }

    // Initializes the outputs
    void InitOutputLayer()
    {
        int count = 1;

        Vector3 Anchor = UIAnchor.position;
        Vector3 newPos;
        GameObject v;
        Text t;

        OutputLayer = new GameObject[outputs];
        for (int i = 0; i < outputs; i++)
        {
            // Initilize each node
            OutputLayer[i] = new GameObject("Output " + i);
        }

        // Construct each node
        foreach (GameObject g in OutputLayer)
        {
            // Set the default position
            g.transform.parent = transform;

            newPos = new Vector3(Anchor.x + (XOfset * (CurrentBehavior.HiddenLayers.Length + 3)), Anchor.y + (YOfset * count), Anchor.z);
            g.transform.position = newPos;
            g.transform.localScale /= 2;

            // Add the circle sprite image
            g.AddComponent(typeof(Image));
            g.GetComponent<Image>().sprite = NodeSprite;

            // Add the text
            v = new GameObject("Value");
            v.transform.parent = g.transform;
            v.transform.position = g.transform.position;

            // Configure the text
            t = v.AddComponent(typeof(Text)) as Text;
            t.font = font;
            t.text = v.name;
            t.color = new Color(0, 0, 0, 1);
            t.alignment = TextAnchor.MiddleCenter;

            count++;
        }
    }

    // Garbage disposal
    void Destructor()
    {
        if (InputLayer != null)
        {
            for (int i = InputLayer.Length - 1; i > -1; i--)
            {
                Destroy(InputLayer[i]);
            }
            InputLayer = null;
        }

        if (HiddenLayer != null)
        {
            for (int y = 0; y < HiddenLayer.Length; y++)
            {
                for (int x = 0; x < HiddenLayer[y].Length; x++)
                {
                    Destroy(HiddenLayer[y][x]);
                }
            }
            HiddenLayer = null;
        }

        if (OutputLayer != null)
        {
            for (int i = OutputLayer.Length - 1; i > -1; i--)
            {
                Destroy(OutputLayer[i]);
            }
            InputLayer = null;
        }
    }

    // Restarts the graph
    public void StartNewRound()
    {
        Destructor();
        NetworkName.text = "";
        Fitness.text = "";
        ToolTip.text = "[Press F1 to show Network Graph]";
        ShowGraph = false;
    }
}
