using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterController : MonoBehaviour
{

    Camera mainCam;
    NavMeshAgent AIAgent;
    ThirdPersonCharacter controller;
    Animator anim;

    private void Start()
    {
        mainCam = Camera.main;
        AIAgent = GetComponent<NavMeshAgent>();
        controller = GetComponent<ThirdPersonCharacter>();
        anim = GetComponent<Animator>();

        AIAgent.updateRotation = false;
    }
    // Update is called once per frame
    void Update()
    {
        // When lmb is clicked draw a line from the camera 
        // through the view matrix, set the collision point 
        // as target destination for the AI agent
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                AIAgent.SetDestination(hit.point);
            }
        }

        // If the AI Agent isn't at its target position, 
        // Call the move funciton in the TPS
        if (AIAgent.remainingDistance > AIAgent.stoppingDistance)
        {
            controller.Move(AIAgent.desiredVelocity, false, false);
        }
        //else
        //{
        //    controller.Move(Vector3.zero, false, false);
        //}

        // Dance time
        if(Input.GetKeyDown(KeyCode.Space))
        {
            bool result = !(anim.GetBool("Dancing"));
            anim.SetBool("Dancing", result);
        }
    }
}