using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

    public Transform target;

    private void Start()
    {
        target = Camera.main.transform;
    }
    // Update is called once per frame
    void Update ()
    {
        transform.LookAt(target.position);
    }
}
