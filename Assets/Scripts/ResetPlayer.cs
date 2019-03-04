using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlayer : MonoBehaviour
{
    [Header("Reset:")]
    public bool resetPlayer;

    [Header("Player Setup:")]
    public GameObject player;
    public GameObject playerPrefab;
    public Transform spawn;

    // Update is called once per frame
    private void Start()
    {
        player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
    }

    void Update ()
    {
		if (resetPlayer)
        {
            Destroy(player);
            player = null;

            player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
            resetPlayer = false;
        }

	}
}
