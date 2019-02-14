using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public
    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float gravity = -12;
    public float jumpHeight = 1;
    [Range(0, 1)]
    public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    public float speedSmoothTime = 0.1f;

    public GameObject target;
    public Color playerColour;

    // Private
    private bool inCombat = false;

    private float turnSmoothVelocity;
    private float speedSmoothVelocity;
    private float currentSpeed;
    private float yVel;

    private Animator anim;
    private Transform camT;
    private UnityEngine.CharacterController cc;

    enum State
    {
        Default = -1,
        Idle = 0,
        Combat
    }
    private State currentState = 0;

    void Start()
    {
        anim = GetComponent<Animator>();
        camT = Camera.main.transform;
        cc = GetComponent<UnityEngine.CharacterController>();
        target = null;

        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (smr.material.color == new Color(0.09657001f, 0.4216198f, 0.522f, 1))
            {
                smr.material.color = playerColour;
            }
        }
    }

    void Update()
    {

        // Check if the player is in combat
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (inCombat)
            {
                target = null;
            }
            else
            {
                GetTarget();
            }

            inCombat = !inCombat;
        }
        currentState = ((inCombat) ? (State)1 : (State)0);
        anim.SetBool("inCombat", inCombat);


        // Movment
        if (target == null)
        {
            // inputs
            Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 inputDir = rawInput.normalized;
            bool running = Input.GetKey(KeyCode.LeftShift);

            Move(inputDir, running);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            // animaton
            float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f);
            anim.SetFloat("forwardVelocity", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
            anim.SetFloat("sidewardVelocity", 0.0f);
        }
        else
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            CombatMove(input);

            // animation
            anim.SetFloat("forwardVelocity", input.y);
            anim.SetFloat("sidewardVelocity", input.x);
        }

    }


    void Move(Vector2 inputDir, bool running)
    {
        // Rotation
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + camT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        }

        // Position
        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        yVel += Time.deltaTime * gravity;
        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * yVel;

        cc.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(cc.velocity.x, cc.velocity.z).magnitude;

        if (cc.isGrounded)
        {
            anim.SetBool("Grounded", true);
            yVel = 0;
        }

    }

    void CombatMove(Vector2 input)
    {
        // Rotation
        transform.LookAt(target.transform.position);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // Position
        yVel += Time.deltaTime * gravity;
        input *= walkSpeed;

        Vector3 velocity = transform.forward * input.y + transform.right * input.x + Vector3.up * yVel;

        cc.Move(velocity * Time.deltaTime);

        if (cc.isGrounded)
        {
            anim.SetBool("Grounded", true);
            yVel = 0;
        }
    }

    void Jump()
    {
        if (cc.isGrounded)
        {
            anim.SetBool("Grounded", false);
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            yVel = jumpVelocity;
        }
    }

    float GetModifiedSmoothTime(float smoothTime)
    {
        if (cc.isGrounded)
        {
            return smoothTime;
        }

        if (airControlPercent == 0)
        {
            return float.MaxValue;
        }
        return smoothTime / airControlPercent;
    }

    void GetTarget()
    {

        // Get the bot the player is looking at
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.tag == "character")
            {
                target = hit.transform.gameObject;
            }
        }

        // If the player isn't looking at a bot, get the closest one
        if (target == null)
        {
            float smallestDist = Mathf.Infinity;
            GameObject temp = null;
            GameObject[] bots = GameObject.FindGameObjectsWithTag("character");


            foreach (GameObject g in bots)
            {
                float dist = Vector3.Distance(transform.position, g.transform.position);

                if (dist < smallestDist)
                {
                    smallestDist = dist;
                    temp = g;
                }
            }

            target = temp;
        }

    }

}

