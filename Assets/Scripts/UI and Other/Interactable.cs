using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool Occupied;

    public string Type;
    public List<Factions> _Factions;

    public Transform InteractPoint;

    private void Start()
    {
        Type = gameObject.tag;
        Occupied = false;
    }
}
