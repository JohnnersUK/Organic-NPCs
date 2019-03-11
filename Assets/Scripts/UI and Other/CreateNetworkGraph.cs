using UnityEngine;
using UnityEngine.UI;

public class CreateNetworkGraph : MonoBehaviour
{

    public CombatController CC;
    public Sprite NodeSprite;
    public Font font;

    public GameObject[] InputLayer;
    public GameObject[][] HiddenLayer;
    public GameObject[] OutputLayer;

    public Transform UIAnchor;

    public int XOfset;
    public int YOfset;

    private int inputs;
    private int hiddenLayers;
    private int outputs;

    // Use this for initialization
    void Start()
    {
        int length;

        inputs = CC.Inputs.Length;
        hiddenLayers = CC.HiddenLayers.Length;
        outputs = CC.Outputs;

        // Get the total number of nodes
        length = inputs + outputs;
        for (int i = 0; i < hiddenLayers; i++)
        {
            length += CC.HiddenLayers[i];
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
        float[][] NodeValues = CC.Network.GetNeurons();

        for (int i = 0; i < inputs; i++)
        {
            InputLayer[i].GetComponentInChildren<Text>().text = NodeValues[0][i].ToString("F2");
        }

        for (int i = 0; i < CC.HiddenLayers.Length; i++)
        {
            for (int j = 0; j < CC.HiddenLayers[i]; j++)
            {
                HiddenLayer[i][j].GetComponentInChildren<Text>().text = NodeValues[i + 1][j].ToString("F2");
            }
        }

        for (int i = 0; i < outputs; i++)
        {
            OutputLayer[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            OutputLayer[i].GetComponentInChildren<Text>().text = NodeValues[CC.HiddenLayers.Length + 1][i].ToString("F2");

            if (NodeValues[CC.HiddenLayers.Length + 1][i] > bestValue)
            {
                bestValue = NodeValues[CC.HiddenLayers.Length + 1][i];
                bestNode = i;
            }
        }

        OutputLayer[bestNode].GetComponent<Image>().color = new Color(0, 1, 0, 1);


        // Log the actual node values for a 5 layer network
        //Debug.Log("Inputs: " + NodeValues[0][0].ToString("F2") + " " + NodeValues[0][1].ToString("F2") + " " + NodeValues[0][2].ToString("F2"));

        //Debug.Log("Hidden 1: " + NodeValues[1][0].ToString("F2") + " " + NodeValues[1][1].ToString("F2") + " " + NodeValues[1][2].ToString("F2"));

        //Debug.Log("Hidden 2: " + NodeValues[2][0].ToString("F2") + " " + NodeValues[2][1].ToString("F2") + " " + NodeValues[2][2].ToString("F2"));

        //Debug.Log("Hidden 3: " + NodeValues[3][0].ToString("F2") + " " + NodeValues[3][1].ToString("F2") + " " + NodeValues[3][2].ToString("F2"));

        //Debug.Log("Outputs: " + NodeValues[4][0].ToString("F2") + " " + NodeValues[4][1].ToString("F2") + " " + NodeValues[4][2].ToString("F2"));

    }

    void InitInputLayer()
    {
        int count = 1;
        Vector3 Anchor = UIAnchor.position;

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
            g.transform.parent = this.transform;

            Vector3 newPos = new Vector3(Anchor.x, Anchor.y + (YOfset * count), Anchor.z);
            g.transform.position = newPos;
            g.transform.localScale /= 2;

            // Add the circle sprite image
            g.AddComponent(typeof(Image));
            g.GetComponent<Image>().sprite = NodeSprite;

            // Add the text
            GameObject v = new GameObject("Value");
            v.transform.parent = g.transform;
            v.transform.position = g.transform.position;

            // Configure the text
            Text t = v.AddComponent(typeof(Text)) as Text;
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

        // Construct the '2D' (Jagged) array
        HiddenLayer = new GameObject[CC.HiddenLayers.Length][];
        for (int i = 0; i < CC.HiddenLayers.Length; i++)
        {
            HiddenLayer[i] = new GameObject[CC.HiddenLayers[i]];
        }

        for (int y = 0; y < CC.HiddenLayers.Length; y++)
        {
            for (int x = 0; x < CC.HiddenLayers[y]; x++)
            {
                HiddenLayer[y][x] = new GameObject("Hidden" + y + x);
                HiddenLayer[y][x].transform.parent = this.transform;

                Vector3 newPos = new Vector3(Anchor.x + (XOfset * (y + 2)), Anchor.y + (YOfset * (x + 1)), Anchor.z);
                HiddenLayer[y][x].transform.position = newPos;
                HiddenLayer[y][x].transform.localScale /= 2;

                // Add the circle sprite image
                HiddenLayer[y][x].AddComponent(typeof(Image));
                HiddenLayer[y][x].GetComponent<Image>().sprite = NodeSprite;

                // Add the text
                GameObject v = new GameObject("Value");
                v.transform.parent = HiddenLayer[y][x].transform;
                v.transform.position = HiddenLayer[y][x].transform.position;

                // Configure the text
                Text t = v.AddComponent(typeof(Text)) as Text;
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
            g.transform.parent = this.transform;

            Vector3 newPos = new Vector3(Anchor.x + (XOfset * (CC.HiddenLayers.Length + 3)), Anchor.y + (YOfset * count), Anchor.z);
            g.transform.position = newPos;
            g.transform.localScale /= 2;

            // Add the circle sprite image
            g.AddComponent(typeof(Image));
            g.GetComponent<Image>().sprite = NodeSprite;

            // Add the text
            GameObject v = new GameObject("Value");
            v.transform.parent = g.transform;
            v.transform.position = g.transform.position;

            // Configure the text
            Text t = v.AddComponent(typeof(Text)) as Text;
            t.font = font;
            t.text = v.name;
            t.color = new Color(0, 0, 0, 1);
            t.alignment = TextAnchor.MiddleCenter;

            count++;
        }
    }
}
