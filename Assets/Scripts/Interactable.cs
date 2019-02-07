using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool Occupied = false;
    public string Type;
    public Transform InteractPoint;

    private float count = 5.0f;

    private void Start()
    {
        Type = gameObject.tag;
    }

}
