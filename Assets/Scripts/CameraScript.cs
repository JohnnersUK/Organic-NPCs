using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    [SerializeField] Transform Target;
    [SerializeField] float minFov;
    [SerializeField] float maxFov;

    // Update is called once per frame
    void Update ()
    {
        float distance = Vector3.Distance(this.transform.position, Target.transform.position);
        float fov = 100 - (distance * 3);

        GetComponent<Camera>().fieldOfView = fov;
        transform.LookAt(Target);
    }
}
