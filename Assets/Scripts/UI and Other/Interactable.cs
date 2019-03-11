using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    public bool Occupied = false;

    public string Type;
    public List<Factions> _Factions;

    public Transform InteractPoint;

    private float count = 5.0f;

    private void Start()
    {
        Type = gameObject.tag;
    }

}
