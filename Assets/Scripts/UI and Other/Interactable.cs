using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool Occupied;

    public string Type;
    public List<Factions> _Factions;

    public Transform InteractPoint;

    private GameObject lastUsed;

    private void Start()
    {
        Type = gameObject.tag;
        Occupied = false;
    }

    public void SetLastUsed(GameObject obj)
    {
        lastUsed = obj;
    }

    public GameObject GetLastUsed()
    {
        return lastUsed;
    }
}
