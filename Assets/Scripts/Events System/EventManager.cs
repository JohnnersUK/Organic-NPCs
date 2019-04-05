using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

// A class made to limit the ammount of subscriptions required to broadcast an event
// E.g this manager subscribes to all events, all bots subscribe to this manager
public class EventManager : MonoBehaviour
{
    public static EventManager instance = null;
    [SerializeField] private NetworkTrainingScript nts = null;
    [SerializeField] private ResetPlayer rp = null;

    private bool playerLoaded = false;

    // Hit event
    public delegate void HitEventHandler(object source, PublicEventArgs args);
    public event HitEventHandler HitEvent;

    // Death event
    public delegate void DeathEventHandler(object source, PublicEventArgs args);
    public event DeathEventHandler DeathEvent;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
        foreach (EventManager im in FindObjectsOfType<EventManager>())
        {
            if (im != this)
            {
                Destroy(gameObject);
            }
        }
        instance = this;
    }

    // Find components and subscribe to all delegates
    public void Start()
    {
        List<AttackCollider> ac;
        List<PlayerAttackCollider> pac;

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Find the bots and add them to the public event listener
        foreach (GameObject b in nts.Bots)
        {
            // Get the death event
            b.GetComponent<AgentController>().DeathEvent += OnPublicEvent;

            // Get the attack event
            ac = new List<AttackCollider>();
            ac.AddRange(b.GetComponentsInChildren<AttackCollider>());

            foreach (AttackCollider a in ac)
            {
                a.HitEvent += OnPublicEvent;
            }
        }

        // Find the player and add it to the public event listener
        if (rp.player != null)
        {
            rp.player.GetComponent<PlayerController>().DeathEvent += OnPublicEvent;

            pac = new List<PlayerAttackCollider>();
            pac.AddRange(rp.player.GetComponentsInChildren<PlayerAttackCollider>());
            foreach (PlayerAttackCollider p in pac)
            {
                p.HitEvent += OnPublicEvent;
            }
            playerLoaded = true;
        }
    }

    private void Update()
    {
        List<PlayerAttackCollider> pac;

        // Find the player and add it to the public event listener
        if (rp.player != null && !playerLoaded)
        {
            rp.player.GetComponent<PlayerController>().DeathEvent += OnPublicEvent;

            pac = new List<PlayerAttackCollider>();
            pac.AddRange(rp.player.GetComponentsInChildren<PlayerAttackCollider>());
            foreach (PlayerAttackCollider p in pac)
            {
                p.HitEvent += OnPublicEvent;
            }
            playerLoaded = true;
        }
    }

    // Whenever a public event is triggered sort it and send it to all handlers
    public void OnPublicEvent(object source, PublicEventArgs args)
    {
        // If the event is a restart event
        if (args == EventArgs.Empty)
        {
            Start();
            return;
        }

        // Send the event information
        switch (args._eventType)
        {
            default:
            case EventType.Hit:
            {
                Debug.Log("Broadcasting a hit event");
                OnHitEvent(args);
                break;
            }
            case EventType.Death:
            {
                Debug.Log("Broadcasting a death event");
                OnDeathEvent(args);
                break;
            }
        }
    }

    // Receive a scene load event
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Start();
    }

    // Send a hit event to all handlers
    protected virtual void OnHitEvent(PublicEventArgs eventArgs)
    {
        if (HitEvent != null)
        {
            HitEvent(this, eventArgs);
        }
    }

    protected virtual void OnDeathEvent(PublicEventArgs eventArgs)
    {
        if (DeathEvent != null)
        {
            DeathEvent(this, eventArgs);
        }
    }

    public void OnRoundStart()
    {
        DeathEvent = null;
        HitEvent = null;
    }
}
