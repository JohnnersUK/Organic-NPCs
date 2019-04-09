using UnityEngine;

public class CameraScript : MonoBehaviour
{

    [SerializeField] Transform Target;
    [SerializeField] float minFov = 20;
    [SerializeField] float maxFov = 100;

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

            fov = Mathf.Clamp((maxFov - (distance * 3)), minFov, maxFov);

            GetComponent<Camera>().fieldOfView = fov;
            transform.LookAt(Target);
        }

    }
}
