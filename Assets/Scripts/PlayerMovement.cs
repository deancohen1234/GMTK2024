using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public struct GroundParams
{
    public Vector3 Normal;
    public Vector3 Position;
}

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform Camera;

    [Header("Movement Params")]
    public float Speed = 4f;
    public float MaxAcceleration = 6f;
    public float Gravity = 20f;
    public LayerMask GroundCheckMask = ~0;

    [Header("Hover Params")]
    public float HoverHeight = 3f;
    public float SpringConstant = 3f;
    public float Dampening = 0.99f;


    private Vector2 DesiredMovement;
    private Rigidbody Body;

    private GroundParams GroundParams;
    // Start is called before the first frame update
    void Start()
    {
        Body = GetComponent<Rigidbody>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //get input
        DesiredMovement.x = Input.GetAxis("Vertical");
        DesiredMovement.y = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        Vector3 Velocity = Body.velocity;

        Velocity = GetMoveVelocity(Velocity);
        Velocity += GetHoverVelocity(Velocity);

        Body.velocity = Velocity;
    }

    private Vector3 GetMoveVelocity(Vector3 CurrentVelocity)
    {
        SetGroundParams();

        //get local forward and right
        Vector3 forward = (Body.position - Camera.position).normalized;
        Vector3 right = Vector3.Cross(GroundParams.Normal, forward).normalized;
        Vector3 groundedForward = Vector3.ProjectOnPlane(forward, GroundParams.Normal);
        Vector3 groundedRight = Vector3.ProjectOnPlane(right, GroundParams.Normal);

        //get local forward and right vel
        Vector3 forwardVel = groundedForward * DesiredMovement.x;
        Vector3 rightVel = groundedRight * DesiredMovement.y;
        Vector3 desiredVel = (forwardVel + rightVel).normalized * Speed;

        //move current Velocity to desired, based on acceleration
        Vector3 velocity = Vector3.MoveTowards(CurrentVelocity, desiredVel, MaxAcceleration * Time.fixedDeltaTime);

        //add gravity
        velocity += -Vector3.up * Gravity * Time.fixedDeltaTime;

        return velocity;
    }

    private Vector3 GetHoverVelocity(Vector3 CurrentVelocity)
    {
        //try and spring the riding height at HoverHeight

        float yDiff = (Body.position.y - (GroundParams.Position.y + HoverHeight));

        float restoringVelocity = -SpringConstant * yDiff * Time.fixedDeltaTime;
        restoringVelocity *= Dampening;

        return GroundParams.Normal * restoringVelocity;
    }

    //raycast down to get ground
    private void SetGroundParams()
    {
        RaycastHit hit;

        if (Physics.Raycast(Body.position, Vector3.down, out hit, Mathf.Infinity, GroundCheckMask))
        {

            GroundParams.Normal = hit.normal;
            GroundParams.Position = hit.point;
        }
    }
}
