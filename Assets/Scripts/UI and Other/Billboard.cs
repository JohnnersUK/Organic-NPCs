using UnityEngine;

public class Billboard : MonoBehaviour
{

    [SerializeField] private Transform target;

    private void Start()
    {
        target = Camera.main.transform;
    }
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target.position);
    }
}
