using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomController : CombatAgent
{
    public enum State
    {
        Idle,
        Walk,
        Jump,
        Grapple
    }

    [SerializeField] private float staminaCurrent, staminaMax = 100, staminaRunCost = 10, staminaRechargeRate = 20, staminaRechargeDelay = 1.5f;

    private float staminaTimeLastUsed;

    [SerializeField] private State currentState;

    //These values are responsible for our movement speeds
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float gravity;

    //how close to the ground we need to be, to be "grounded"
    [SerializeField] private float groundedAllowance = 0.05f;
    //the angle in degrees we're allowed to walk up
    [SerializeField] private float walkAngle = 40f;

    //this will hold a reference to our camera's anchor point
    [SerializeField] private GameObject cameraAnchor;

    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float maxGrappleSpeed = 80;

    private float currentGrappleSpeed;

    private Vector3 grapplePoint = new Vector3();

    private Rigidbody rb;

    private CameraSwapper cameraSwapper;

    private GrappleLine grappleRenderer;

    //hold on to our momentum values
    private float horizontalSpeed, verticalSpeed;

    //this will hold the player's inputs during an Update loop
    public Vector2 inputThisFrame;

    //this will hold our calculated movememnt during an Update loop
    public Vector3 movementThisFrame;

    private PlayerUI playerUI;

    // Start is called before the first frame update
    protected override void Start()
    {
        // Do the Start behaviour from my parent
        base.Start();

        rb = GetComponent<Rigidbody>();
        cameraSwapper = GetComponent<CameraSwapper>();
        grappleRenderer = GetComponentInChildren<GrappleLine>();
        playerUI = FindObjectOfType<PlayerUI>();
        staminaCurrent = staminaMax;

        NextState();
    }

    private void NextState()
    {
        switch (currentState)
        {
            case State.Idle:
                StartCoroutine("IdleState");
                break;
            case State.Walk:
                StartCoroutine("WalkState");
                break;
            case State.Jump:
                StartCoroutine("JumpState");
                break;
            case State.Grapple:
                StartCoroutine("GrappleState");
                break;
        }
    }

    private void ChangeState(State newState)
    {
        currentState = newState;
    }

    private void AscendAt(float speed)
    {
        verticalSpeed = speed;
        ChangeState(State.Jump);
    }

    #region State Coroutines
    IEnumerator IdleState()
    {
        //Enter idle
        Move(Vector3.zero);
        while (currentState == State.Idle)
        {
            //while in Idle...
            if (!IsGrounded())
            {
                ChangeState(State.Jump);
            }
            else
            {
                if (inputThisFrame.magnitude != 0)
                {
                    ChangeState(State.Walk);
                }
                if (Input.GetButton("Jump"))
                {
                    AscendAt(jumpPower);
                }
            }
            yield return null;
        }
        //Exit idle

        NextState();
    }

    IEnumerator WalkState()
    {

        while (currentState == State.Walk)
        {
            //refresh our movement to (0,0,0)
            movementThisFrame = new Vector3();

            //map our horizontal movement based on our inputs.
            movementThisFrame.x = inputThisFrame.x;
            movementThisFrame.z = inputThisFrame.y;

            float speedThisFrame = walkSpeed;


            if (Input.GetButton("Sprint") && TryToUseStamina(staminaRunCost*Time.deltaTime))
            {
                speedThisFrame = runSpeed;
            }
            else
                if (Input.GetButton("Crouch"))
            {
                speedThisFrame = crouchSpeed;
            }

            //transform our inputs to local space
            movementThisFrame = TransformDirection(movementThisFrame);

            if (inputThisFrame.magnitude > 0 && ValidateDirection(movementThisFrame))
            {
                horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, speedThisFrame, runSpeed * Time.deltaTime);
            }
            else
            {
                horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, 0, runSpeed * Time.deltaTime);
            }

            //multiply the direction by our speed
            movementThisFrame *= horizontalSpeed;
            
            //check if we're on the ground
            if (IsGrounded())     //<- remember, IsGrounded() will result in true or false
            {
                //check if the jump button is pressed
                if (Input.GetButtonDown("Jump"))
                {
                    //move upwards based on our jump power
                    AscendAt(jumpPower);
                }
            }
            else
            {
                ChangeState(State.Jump);
            }

            //run our Move instructions, using the direction we worked out
            Move(movementThisFrame);

            yield return null;
        }

        NextState();
    }

    IEnumerator JumpState()
    {

        while (currentState == State.Jump)
        {
            movementThisFrame = new Vector3(inputThisFrame.x, 0, inputThisFrame.y);
            movementThisFrame *= horizontalSpeed;

            movementThisFrame = TransformDirection(movementThisFrame);

            verticalSpeed -= gravity * Time.deltaTime;
            movementThisFrame.y = verticalSpeed;

            Move(movementThisFrame);

            if (IsGrounded() && verticalSpeed < 0)
            {
                ChangeState(State.Walk);
            }

            yield return null;
        }

        NextState();
    }
    
    IEnumerator GrappleState()
    {
        grappleRenderer.StartGrapple(grapplePoint);


        while (currentState == State.Grapple)
        {
            Vector3 grappleDirection = grapplePoint - transform.position;
            grappleDirection.Normalize();
            currentGrappleSpeed = Mathf.Clamp(currentGrappleSpeed + maxGrappleSpeed * Time.deltaTime, 0, maxGrappleSpeed);

            if(Vector3.Distance(grapplePoint, transform.position) < 2f || Input.GetButtonUp("Grapple"))
            {
                EndGrapple();
            }
            else
            {
                Move(grappleDirection * currentGrappleSpeed);
            }
            yield return null;
        }

        grappleRenderer.EndGrapple();
        NextState();
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        //get the player inputs and store in a variable
        inputThisFrame = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        inputThisFrame.Normalize();

        if (Input.GetButtonDown("Grapple"))
        {
            TryToGrapple();
        }

        if(Time.time > staminaTimeLastUsed + staminaRechargeDelay)
        {
            // If the current time has surpassed the last used time and the delay...
            staminaCurrent = Mathf.Clamp(staminaCurrent + staminaRechargeRate * Time.deltaTime, 0, staminaMax);
        }
        externalVelocity /= (Time.deltaTime) + 1;

        // Update HUD with percentages of health and stamina represented as a value between 0-1
        playerUI.UpdateHUD(healthCurrent / healthMax, staminaCurrent / staminaMax);
    }

    protected virtual void Move(Vector3 moveDirection)
    {
        rb.velocity = moveDirection + externalVelocity;
    }

    private bool IsGrounded()
    {
        //cast the bottom of our collider downward
        if (Physics.SphereCast(transform.position, 0.5f, Vector3.down, out RaycastHit hit, (1 / 2f) + groundedAllowance, groundMask))
        {
            //if we find ground...
            //check if the ground is flat
            //if so, we're grounded
            return ValidateGroundAngle(hit.normal);
        }
        //else we're not grounded
        return false;

        //old code
        //return Physics.Raycast(transform.position,Vector3.down,1.001f,groundMask);
    }

    //transform our input direction into our local direction
    private Vector3 TransformDirection(Vector3 direction)
    {
        //get which camera is currently active
        if (cameraSwapper.GetCameraMode() == CameraSwapper.CameraMode.FirstPerson)
        {
            //if it's our first person camera...
            //make sure we're facing the same way as the camera
            FaceDirection(cameraSwapper.GetCurrentCamera().transform.localEulerAngles);
            //translate based on our transform
            return transform.TransformDirection(direction);
        }
        //otherwise, transform based on the CameraAnchor transform
        return cameraSwapper.GetCurrentCamera().transform.root.TransformDirection(direction);
    }


    //make us face the way we're supposed to face
    private void FaceDirection(Vector3 direction)
    {
        //get which camera is currently active
        if (cameraSwapper.GetCameraMode() == CameraSwapper.CameraMode.FirstPerson)
        {
            //if it's our first person camera...
            //snapping our player's y rotation to match the camera
            transform.localEulerAngles = new Vector3(0, direction.y, 0);
        }
        //otherwise, we want to use our movement direction
        else
        {
            //use our movement, but don't rotate upwards/downwards
            transform.forward = new Vector3(direction.x, 0, direction.z);
        }

    }


    //check if the ground we're trying to walk on is valid (i.e. not too steep)
    private bool ValidateDirection(Vector3 direction)
    {
        //we want to check where we're about to be (some kind of cast)
        if (Physics.SphereCast(transform.position + Vector3.down * 0.5f, 0.5f, direction, out RaycastHit hit, 0.5f, groundMask))
        {
            //if we find ground there...
            //check if it's flat
            return ValidateGroundAngle(hit.normal);
        }
        //if we don't find ground, we're allowed to move
        return true;
    }


    //check if certain ground is flat
    private bool ValidateGroundAngle(Vector3 groundNormal)
    {
        //compare the angle of the ground, to our walkable angle
        //if the ground is too steep, the check fails
        if (Vector3.Angle(Vector3.up, groundNormal) < walkAngle)
        {
            return true;
        }
        return false;
    }

    protected override void EndOfLife()
    {
        //put game over behaviour in here
        Debug.Log("Player has died");
    }

    #region Grapple methods
    private void TryToGrapple()
    {
        grapplePoint = GetComponent<Grapple>().TryToGrapple();
        if (grapplePoint != Vector3.zero)
        {
            StartGrapple();
        }
    }

    private void StartGrapple()
    {
        verticalSpeed = 0;
        horizontalSpeed = 0;
        ChangeState(State.Grapple);
    }

    private Vector3 externalVelocity = Vector3.zero;

    private void EndGrapple()
    {
        grapplePoint = Vector3.zero;
        currentGrappleSpeed = 0;
        externalVelocity = rb.velocity;
        //Vector3 localVelocity = transform.TransformDirection(rb.velocity);

        ChangeState(State.Jump);
    }
    #endregion

    public bool TryToUseStamina(float cost)
    {
        if(staminaCurrent == 0)
        {
            return false;
        }

        staminaCurrent = Mathf.Clamp(staminaCurrent - cost, 0, staminaMax);
        staminaTimeLastUsed = Time.time;
        return true;
    }
}
