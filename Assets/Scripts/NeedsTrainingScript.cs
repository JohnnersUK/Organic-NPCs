﻿using System.Collections.Generic;
using UnityEngine;

public class NeedsTrainingScript : MonoBehaviour
{
    public int simultaionNum;

    //public GameObject hsTable;

    [Range(1, 20)]
    public float simulationSpeed = 1.0f;

    public float simulationTime = 300;

    public GameObject botPrefab;

    private Transform[] spawnPoints;
    private GameObject[] bots;

    public List<GameObject> highScores;

    private Interactable[] objects;

    private float count;

    // Use this for initialization
    void Start()
    {
        int i = 0;

        simultaionNum = 1;

        GameObject interactables = GameObject.FindGameObjectWithTag("interactables");
        objects = interactables.GetComponentsInChildren<Interactable>();
        highScores = new List<GameObject>();

        count = simulationTime;

        Time.timeScale = simulationSpeed;

        spawnPoints = GetComponentsInChildren<Transform>();
        bots = new GameObject[spawnPoints.Length];

        foreach (Transform T in spawnPoints)
        {
            bots[i] = Instantiate(botPrefab, T.position, T.rotation);
            highScores.Add(bots[i]);

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

        count -= 1 * Time.deltaTime;
        if (count <= 0)
        {
            // Shell sort
            inc = 3;
            while (inc > 0)
            {
                for (i = 0; i < bots.Length; i++)
                {
                    j = i;
                    temp = bots[i];
                    while ((j >= inc) && (bots[j - inc].GetComponent<NeedsController>().NeedsNetwork.GetFitness() >
                        temp.GetComponent<NeedsController>().NeedsNetwork.GetFitness()))
                    {
                        bots[j] = bots[j - inc];
                        j = j - inc;
                    }
                    bots[j] = temp;
                }
                if (inc / 2 != 0)
                {
                    inc = inc / 2;
                }
                else if (inc == 1)
                {
                    inc = 0;
                }
                else
                {
                    inc = 1;
                }
            }

            UpdateHighScores();

            Debug.Log("Results:");
            // print and store the fitness, then destroy the bots
            for (k = 0; k < bots.Length; k++)
            {
                Debug.Log(bots[k].name + ": " + bots[k].GetComponent<NeedsController>().NeedsNetwork.GetFitness());

                if (k < 5)
                {
                    tempWeights.Add((bots[-k + (bots.Length - 1)].GetComponent<NeedsController>().NeedsNetwork.GetWeights()));
                }

                Destroy(bots[k]);
            }



            // Create a new set of mutated bots using the top 5 previous fitnesses
            for (k = 0; k < spawnPoints.Length; k++)
            {
                bots[k] = Instantiate(botPrefab, spawnPoints[k].position, spawnPoints[k].rotation);
                bots[k].GetComponent<NeedsController>().Start();


                // name and color the bot to make it identifiable
                bots[k].name = "bot " + k;
                foreach (SkinnedMeshRenderer smr in bots[k].GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if (smr.material.color == new Color(0.09657001f, 0.4216198f, 0.522f, 1))
                    {
                        smr.material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
                    }
                }
                bots[k].GetComponent<NeedsController>().NeedsNetwork.SetWeights(tempWeights[k % 4]);
                bots[k].GetComponent<NeedsController>().NeedsNetwork.Mutate();
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
    }

    void UpdateHighScores()
    {
        List<GameObject> tempTable = new List<GameObject>();
        tempTable.AddRange(highScores);
        tempTable.AddRange(bots);

        int i, j, k, inc;
        GameObject temp;

        count -= 1 * Time.deltaTime;
        if (count <= 0)
        {
            // Shell sort
            inc = 3;
            while (inc > 0)
            {
                for (i = 0; i < tempTable.Count; i++)
                {
                    j = i;
                    temp = tempTable[i];
                    while ((j >= inc) && (tempTable[j - inc].GetComponent<NeedsController>().NeedsNetwork.GetFitness() >
                        temp.GetComponent<NeedsController>().NeedsNetwork.GetFitness()))
                    {
                        tempTable[j] = tempTable[j - inc];
                        j = j - inc;
                    }
                    tempTable[j] = temp;
                }
                if (inc / 2 != 0)
                {
                    inc = inc / 2;
                }
                else if (inc == 1)
                {
                    inc = 0;
                }
                else
                {
                    inc = 1;
                }
            }
        }

        for (i = 0; i < 10; i++)
        {
            highScores[i] = tempTable[tempTable.Count - (i+1)];
        }

    }

}
