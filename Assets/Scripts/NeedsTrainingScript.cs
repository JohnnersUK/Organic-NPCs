using System.Collections.Generic;
using UnityEngine;

public class NeedsTrainingScript : MonoBehaviour
{
    public enum Networks
    {
        NeedsNetwork = 0,
        CombatNetwork = 1,
        MasterNetwork = 2
    }
    public Networks trainingNetwork;

    public int simultaionNum;

    [Range(1, 20)]
    public float simulationSpeed = 1.0f;

    public float simulationTime = 300;

    public GameObject botPrefab;

    private Transform[] spawnPoints;
    private GameObject[] bots;

    private Interactable[] objects;

    private float count;

    // Use this for initialization
    void Start()
    {
        int i = 0;

        simultaionNum = 1;

        GameObject interactables = GameObject.FindGameObjectWithTag("interactables");
        objects = interactables.GetComponentsInChildren<Interactable>();

        count = simulationTime;

        Time.timeScale = simulationSpeed;

        spawnPoints = GetComponentsInChildren<Transform>();
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

        int i, j, k, inc;
        GameObject temp;
        List<float[][][]> tempWeights = new List<float[][][]>();
        List<NeuralNetwork> networks = new List<NeuralNetwork>();

        count -= 1 * Time.deltaTime;
        if (count <= 0)
        {
            switch(trainingNetwork)
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
                            networks.Add(b.GetComponent<NeedsController>().CombatNetwork);
                        }
                        break;
                    }
            }
            // Old code reference
            //// Shell sort
            //inc = 3;
            //while (inc > 0)
            //{
            //    for (i = 0; i < bots.Length; i++)
            //    {
            //        j = i;
            //        temp = bots[i];
            //        while ((j >= inc) && (networks[j - inc].GetFitness() >
            //            networks[i].GetFitness()))
            //        {
            //            bots[j] = bots[j - inc];
            //            j = j - inc;
            //        }
            //        bots[j] = temp;
            //    }
            //    if (inc / 2 != 0)
            //    {
            //        inc = inc / 2;
            //    }
            //    else if (inc == 1)
            //    {
            //        inc = 0;
            //    }
            //    else
            //    {
            //        inc = 1;
            //    }
            //}

            //Debug.Log("Results:");
            //// print and store the fitness, then destroy the bots
            //for (k = 0; k < bots.Length; k++)
            //{
            //    Debug.Log(bots[k].name + ": " + networks[k].GetFitness());

            //    if (k < 5)
            //    {
            //        tempWeights.Add((networks[-k + (networks.Length - 1)].GetWeights()));
            //    }

            //    Destroy(bots[k]);
            //}

            //// Create a new set of mutated bots using the top 5 previous fitnesses
            //for (k = 0; k < spawnPoints.Length; k++)
            //{
            //    bots[k] = Instantiate(botPrefab, spawnPoints[k].position, spawnPoints[k].rotation);

            //    // name and color the bot to make it identifiable
            //    bots[k].name = "bot " + k;
            //    foreach (SkinnedMeshRenderer smr in bots[k].GetComponentsInChildren<SkinnedMeshRenderer>())
            //    {
            //        if (smr.material.color == new Color(0.09657001f, 0.4216198f, 0.522f, 1))
            //        {
            //            smr.material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            //        }
            //    }
            //}

            // Reset the interactables
            for (i = 0; i < objects.Length; i++)
            {
                objects[i].Occupied = false;
            }

            count = simulationTime;
            simultaionNum++;

            Debug.Log("Begining simulation " + simultaionNum);
        }
    }
}
