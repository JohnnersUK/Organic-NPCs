using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class CharacterController : MonoBehaviour
{
    // Components
    Animator anim;
    Camera mainCam;
    CharacterStats stats;
    NavMeshAgent AIAgent;
    ThirdPersonCharacter controller;



    private void Start()
    {
        mainCam = Camera.main;
        AIAgent = GetComponent<NavMeshAgent>();
        controller = GetComponent<ThirdPersonCharacter>();
        anim = GetComponent<Animator>();
        stats = GetComponent<CharacterStats>();

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
                AIAgent.isStopped = false;
                Debug.Log(hit.point);
            }
        }

        // If the AI Agent isn't at its target position, 
        // Call the move funciton in the TPS
        if (AIAgent.remainingDistance > AIAgent.stoppingDistance)
        {
            controller.Move(AIAgent.desiredVelocity, false, false);
        }
        else
        {
            controller.Move(Vector3.zero, false, false);
            AIAgent.isStopped = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool result = !(anim.GetBool("InCombat"));
            anim.SetBool("InCombat", result);
        }

        if(Input.GetMouseButtonDown(1))
        {
            Attack();
        }
    }

    void Attack()
    {
        anim.SetInteger("AttackType", Random.Range(0, 8));
    }
}