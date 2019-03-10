using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public
    [Header("Movment settings:")]
    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float gravity = -12;
    public float jumpHeight = 1;
    [Range(0, 1)]
    public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    public float speedSmoothTime = 0.1f;
    [Space(10)]
    public float jumpCost;

    [Header("Combat Settings:")]
    public GameObject target;
    public bool inCombat = false;
    public float attackCost;

    // Private
    private float turnSmoothVelocity;
    private float speedSmoothVelocity;
    private float currentSpeed;
    private float yVel;

    private Animator anim;
    private Transform camT;
    private UnityEngine.CharacterController cc;
    private CharacterStats stats;

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
        stats = GetComponent<CharacterStats>();

        target = null;
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

        if(Input.GetKeyDown(KeyCode.E))
        {
            Attack();
        }

        // Movment
        if (!anim.GetBool("Attacking"))
        {
            if (target == null)
            {
                // inputs
                Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                Vector2 inputDir = rawInput.normalized;

                bool running;
                if (stats.stamina > 0)
                    running = Input.GetKey(KeyCode.LeftShift);
                else
                    running = false;

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
    }

    // Default movment mode
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
        if (running && currentSpeed > 0)
            stats.stamina -= Time.deltaTime;

        if (cc.isGrounded)
        {
            anim.SetBool("Grounded", true);
            yVel = 0;
        }
        else
        {
            anim.SetBool("Grounded", false);
        }

    }

    // Combat movment mode
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
        else
        {
            anim.SetBool("Grounded", false);
        }
    }

    void Attack()
    {
        inCombat = true;
        if (!anim.GetBool("Attacking") && stats.stamina > 0)
        {
            anim.SetBool("Attacking", true);
            anim.SetInteger("attackValue", Random.Range(0, 6));
            stats.stamina -= attackCost;
        }
    }

    
    void Jump()
    {
        if (cc.isGrounded && stats.stamina > 0)
        {
            anim.SetBool("Grounded", false);
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            yVel = jumpVelocity;

            stats.stamina -= jumpCost;
        }
    }

    // Returns movment smoothtime adjusted for air travel
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

    // Gets the closest target
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

