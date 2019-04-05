using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public
    [Header("Movment settings:")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float gravity = -12f;
    [SerializeField] private float jumpHeight = 1f;
    [Range(0, 1)]
    [SerializeField] private float airControlPercent = 0.5f;

    [SerializeField] private float turnSmoothTime = 0.2f;
    [SerializeField] private float speedSmoothTime = 0.1f;
    [Space(10)]
    [SerializeField] public float jumpCost = 1f;

    [Header("Combat Settings:")]
    [SerializeField] private GameObject target;
    [SerializeField] public bool inCombat { private get; set; }
    [SerializeField] private float attackCost = 1f;

    // Private
    private float turnSmoothVelocity;
    private float speedSmoothVelocity;
    private float currentSpeed;
    private float yVel;

    private Animator anim;
    private Transform camT;
    private UnityEngine.CharacterController cc;
    private CharacterStats stats;

    private bool dead = false;

    // Death event
    public delegate void DeathEventHandler(object source, PublicEventArgs args);
    public event DeathEventHandler DeathEvent;

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
        Vector2 rawInput;
        Vector2 inputDir;
        Vector2 input;

        if (stats.GetStat("health") <= 0)
        {
            if (dead)
            {
                return;
            }
            else
            {
                anim.Play("Base Layer.Dead");
                dead = true;
                return;
            }
        }

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
        anim.SetBool("inCombat", inCombat);

        if (Input.GetKeyDown(KeyCode.E))
        {
            Attack();
        }

        // Movment
        if (!anim.GetBool("Attacking"))
        {
            if (target == null)
            {
                // inputs
                rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                inputDir = rawInput.normalized;

                bool running;
                if (stats.GetStat("stamina") > 0)
                {
                    running = Input.GetKey(KeyCode.LeftShift);
                }
                else
                {
                    running = false;
                }

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
                input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
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
        float targetRotation;
        float targetSpeed;
        Vector3 velocity;

        // Rotation
        if (inputDir != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + camT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        }

        // Position
        targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        yVel += Time.deltaTime * gravity;
        velocity = transform.forward * currentSpeed + Vector3.up * yVel;

        cc.Move(velocity * Time.deltaTime);

        currentSpeed = new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
        if (running && currentSpeed > 0)
        {
            stats.ModifyStat("stamina", -Time.deltaTime);
        }

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
        Vector3 velocity;

        // Rotation
        transform.LookAt(target.transform.position);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // Position
        yVel += Time.deltaTime * gravity;
        input *= walkSpeed;

        velocity = transform.forward * input.y + transform.right * input.x + Vector3.up * yVel;

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
        if (!anim.GetBool("Attacking") && stats.GetStat("stamina") > 0)
        {
            anim.SetBool("Attacking", true);
            anim.SetInteger("attackValue", Random.Range(0, 6));
            stats.ModifyStat("stamina", -attackCost);
        }
    }


    void Jump()
    {
        float jumpVelocity;

        if (cc.isGrounded && stats.GetStat("stamina") > 0)
        {
            anim.SetBool("Grounded", false);
            jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            yVel = jumpVelocity;

            stats.ModifyStat("stamina", -jumpCost);
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
        RaycastHit hit;

        // Get the bot the player is looking at
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

    protected virtual void SendDeathEvent()
    {
        PublicEventArgs args;

        if (DeathEvent != null)
        {
            args = new PublicEventArgs(gameObject, null, EventType.Death, 100);
            DeathEvent(this, args);
        }
    }

}

