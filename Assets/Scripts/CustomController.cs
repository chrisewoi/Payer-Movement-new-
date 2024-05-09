using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomController : CombatAgent
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float gravity;
    [SerializeField] private LayerMask groundedMask;
    // How close to the ground we need to be, to be "grounded"
    [SerializeField] private float groundedAllowance = 0.05f;
    // The angle in degrees we're allowed to walk up
    [SerializeField] private float walkAngle = 40f;
    //[SerializeField] private Transform cameraTransform;
    private Rigidbody rb;

    private CameraSwapper cameraSwapper;

    // Hold on to our momentum values
    private float horizontalSpeed, verticalSpeed;

    private Vector2 inputThisFrame = new Vector2();
    private Vector3 movementThisFrame = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraSwapper = GetComponent<CameraSwapper>();
    }

    // Update is called once per frame
    void Update()
    {
        inputThisFrame = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        inputThisFrame.Normalize(); //changes inputThisFrame into a normalized vector
        //inputThisFrame.normalized - gives us the normalized vector, without changing the vector itself

        movementThisFrame = new();

        movementThisFrame.x = inputThisFrame.x;
        movementThisFrame.z = inputThisFrame.y;

        float speedThisFrame = walkSpeed;

        if (Input.GetButton("Sprint"))
        {
            speedThisFrame = runSpeed;
        }
        if (Input.GetButton("Crouch"))
        {
            speedThisFrame = crouchSpeed;
        }

        movementThisFrame = TransformDirection(movementThisFrame);

        if (inputThisFrame.magnitude > 0 && ValidateDirection(movementThisFrame))
        {
            horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, speedThisFrame, runSpeed * Time.deltaTime);
        } else
        {
            horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, 0, runSpeed * Time.deltaTime);
        }

        // Multiply the direction by our speed
        movementThisFrame *= horizontalSpeed;

        // Maintain current vertical speed, and apply gravity
        verticalSpeed -= gravity * Time.deltaTime;

        // Check if we're on the ground
        if (IsGrounded())   //<---- remember, IsGrounded() will result  in true or false
        {
            verticalSpeed = Mathf.Clamp(verticalSpeed, 0, float.PositiveInfinity);
            // Check if the jump button is pressed
            if (Input.GetButton("Jump"))
            {
                verticalSpeed = jumpPower;
            }
        }

        movementThisFrame.y = verticalSpeed;

        Move(movementThisFrame);        
    }

    protected virtual void Move(Vector3 direction)
    {
        rb.velocity = direction;
    }

    private bool IsGrounded()
    {
        // Cast the bottom of our collider downward
        if (Physics.SphereCast(transform.position, 0.5f, Vector3.down, out RaycastHit hit, (1/2f) + groundedAllowance, groundedMask))
        {
        // If we find ground...
            // Check if the ground is flat
            // If so, we're grounded
            return ValidateGroundAngle(hit.normal);

        }
        // Else we're not grounded
        return false;


        // Old code
        //return Physics.Raycast(transform.position, Vector3.down, 1.001f, groundedMask);
    }

    // Transform our input direction into our local direction
    private Vector3 TransformDirection(Vector3 direction)
    {
        // Get which camera is currently active
        if (cameraSwapper.GetCameraMode() == CameraSwapper.CameraMode.FirstPerson)
        {
            // If it's our first person camera...
            FaceDirection(cameraSwapper.GetCurrentCamera().transform.localEulerAngles);
            // Translate based on our transform
            return transform.TransformDirection(direction);
        }
        // Otherwise, transform based on the CameraAnchor transform
        return cameraSwapper.GetCurrentCamera().transform.root.TransformDirection(direction);
    }





    // Make us face the way we're supposed to face
    private void FaceDirection(Vector3 direction)
    {
        //  Get which camera is currently active
        if (cameraSwapper.GetCameraMode() == CameraSwapper.CameraMode.FirstPerson)
        {
            // If it's our first person camera...
            // Snapping our player's y rotation to match the camera
            transform.localEulerAngles = new Vector3(0, direction.y, 0);
        }
        // Otherwise, we want to use our movement direction
        else
        {
            // Use our movement, but don't rotate upwards/downwards
            transform.forward = new Vector3(direction.x, 0, direction.z);
        }
    }




    // Check if the ground we're trying to walk on is valid (i.e. not too steep)
    private bool ValidateDirection(Vector3 direction)
    {
        // We want to check where we're about to be (some kind of cast)
        if (Physics.SphereCast(transform.position + Vector3.down * 0.5f, 0.5f, direction, out RaycastHit hit, 0.5f, groundedMask))
        {
            // If we find ground there...
            // Check if it's flat
            return ValidateGroundAngle(hit.normal);
        }

        // If we don't find ground, we're allowed to move
        return true;
    }


    // Check if certain ground is flat

    private bool ValidateGroundAngle(Vector3 groundNormal)
    {
        // Compare the angle of the ground, to our walkable angle
        // If the ground is too steep, the check fails
        if (Vector3.Angle(Vector3.up, groundNormal) < walkAngle)
        {
            return true;
        }
        else return false;
    }

    protected override void EndOfLife()
    {
        // Put game over behaviour in here
        Debug.Log("Player has died");
    }
}
