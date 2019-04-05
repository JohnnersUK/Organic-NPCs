using UnityEngine;
using UnityEngine.UI;

public class CreateNeedsGraph : MonoBehaviour
{
    [SerializeField] private NeedsController NC = null;
    [SerializeField] private Sprite NodeSprite = null;
    [SerializeField] private Font font = null;

    [SerializeField] private GameObject[] InputLayer = null;
    [SerializeField] private GameObject[][] HiddenLayer = null;
    [SerializeField] private GameObject[] OutputLayer = null;

    [SerializeField] private Transform UIAnchor = null;

    [SerializeField] private int XOfset = 0;
    [SerializeField] private int YOfset = 0;

    private int inputs;
    private int hiddenLayers;
    private int outputs;

    // Use this for initialization
    void Start()
    {
        int length;

        inputs = NC.Inputs.Length;
        hiddenLayers = NC.HiddenLayers.Length;
        outputs = NC.Outputs;

        // Get the total number of nodes
        length = inputs + outputs;
        for (int i = 0; i < hiddenLayers; i++)
        {
            length += NC.HiddenLayers[i];
        }

        // Create the input layer
        InitInputLayer();

        // Create the (Not so) Hidden layer
        InitHiddenLayer();

        // Create the output layer
        InitOutputLayer();
    }

    // Update is called once per frame
    void Update()
    {
        float bestValue = -2;
        int bestNode = 0;
        float[][] NodeValues = NC.Network.GetNeurons();

        for (int i = 0; i < inputs; i++)
        {
            InputLayer[i].GetComponentInChildren<Text>().text = NodeValues[0][i].ToString("F2");
        }

        for (int i = 0; i < NC.HiddenLayers.Length; i++)
        {
            for (int j = 0; j < NC.HiddenLayers[i]; j++)
            {
                HiddenLayer[i][j].GetComponentInChildren<Text>().text = NodeValues[i + 1][j].ToString("F2");
            }
        }

        for (int i = 0; i < outputs; i++)
        {
            OutputLayer[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            OutputLayer[i].GetComponentInChildren<Text>().text = NodeValues[NC.HiddenLayers.Length + 1][i].ToString("F2");

            if (NodeValues[NC.HiddenLayers.Length + 1][i] > bestValue)
            {
                bestValue = NodeValues[NC.HiddenLayers.Length + 1][i];
                bestNode = i;
            }
        }

        OutputLayer[bestNode].GetComponent<Image>().color = new Color(0, 1, 0, 1);
    }

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

    void InitHiddenLayer()
    {
        Vector3 Anchor = UIAnchor.position;
        Vector3 newPos;
        GameObject v;
        Text t;

        // Construct the '2D' (Jagged) array
        HiddenLayer = new GameObject[NC.HiddenLayers.Length][];
        for (int i = 0; i < NC.HiddenLayers.Length; i++)
        {
            HiddenLayer[i] = new GameObject[NC.HiddenLayers[i]];
        }

        for (int y = 0; y < NC.HiddenLayers.Length; y++)
        {
            for (int x = 0; x < NC.HiddenLayers[y]; x++)
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

            newPos = new Vector3(Anchor.x + (XOfset * (NC.HiddenLayers.Length + 3)), Anchor.y + (YOfset * count), Anchor.z);
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
}

