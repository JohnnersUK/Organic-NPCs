using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlayer : MonoBehaviour
{
    public GameObject player;
    public bool reset;

    public GameObject playerPrefab;
    public Transform spawn;

    // Update is called once per frame
    private void Start()
    {
        player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
    }

    void Update ()
    {
		if (reset)
        {
            Destroy(player);
            player = null;

            player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
            reset = false;
        }

	}
}
