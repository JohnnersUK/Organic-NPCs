using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedsTrainingScript : MonoBehaviour
{

    public float simulationSpeed = 1.0f;
    public float simulationTime = 300;

    public GameObject botPrefab;

    private Transform[] spawnPoints;
    private GameObject[] bots;
    private float count;

	// Use this for initialization
	void Start ()
    {
        int i = 0;

        count = simulationTime;

        Time.timeScale = simulationSpeed;
        Time.fixedDeltaTime = simulationSpeed;

        spawnPoints = GetComponentsInChildren<Transform>();
        bots = new GameObject[spawnPoints.Length];

        foreach (Transform T in spawnPoints)
        {
            bots[i] = Instantiate(botPrefab, T.position, T.rotation);
            bots[i].name = "bot " + i;
            i++;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        int i, j, k, inc;
        GameObject temp;

        count -= 1 * Time.deltaTime;
        if (count <= 0)
        {
            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;

            // bubble sort
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
                    inc = inc / 2;
                else if (inc == 1)
                    inc = 0;
                else
                    inc = 1;
            }

            // print the fitness
            for(k = 0; k < bots.Length; k++)
            {
                Debug.Log(bots[k].GetComponent<NeedsController>().NeedsNetwork.GetFitness());
            }

            count = simulationTime;
            Time.timeScale = simulationSpeed;
            Time.fixedDeltaTime = simulationSpeed;

        }
	}
}
