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
            i++;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        count -= 1 * Time.deltaTime;
        if (count <= 0)
        {
            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;
        }
	}
}
