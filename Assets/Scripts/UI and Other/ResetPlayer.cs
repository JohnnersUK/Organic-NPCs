
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetPlayer : MonoBehaviour
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
        foreach (ResetPlayer im in FindObjectsOfType<ResetPlayer>())
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
        if (resetPlayer)
        {
            Destroy(player);
            player = null;

            player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
            resetPlayer = false;
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Start();
    }
}
