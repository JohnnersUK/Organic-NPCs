
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerResetScript : MonoBehaviour
{
    [Header("Reset:")]
    [SerializeField] private bool resetPlayer;

    [Header("Player Setup:")]
    public GameObject player;
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private Transform spawn = null;

    // Update is called once per frame
    public void Start()
    {
        DontDestroyOnLoad(gameObject);
        foreach (PlayerResetScript im in FindObjectsOfType<PlayerResetScript>())
        {
            if (im != this)
            {
                Destroy(gameObject);
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.R))
        {
            resetPlayer = true;
        }

        if (resetPlayer)
        {
            ResetPlayer();
        }
    }

    public void ResetPlayer()
    {
        Destroy(player);
        player = null;

        player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
        resetPlayer = false;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Start();
    }
}
