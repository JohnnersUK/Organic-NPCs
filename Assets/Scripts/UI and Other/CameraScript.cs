using UnityEngine;

public class CameraScript : MonoBehaviour
{

    [SerializeField] Transform Target;
    [SerializeField] float minFov;
    [SerializeField] float maxFov;

    // Update is called once per frame
    void Update()
    {
        float distance;
        float fov;

        if (Target == null)
        {
            Target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        }
        else
        {
            distance = Vector3.Distance(transform.position, Target.transform.position);
            fov = 100 - (distance * 3);

            GetComponent<Camera>().fieldOfView = fov;
            transform.LookAt(Target);
        }

    }
}
