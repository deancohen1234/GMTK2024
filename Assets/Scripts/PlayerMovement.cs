using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public struct GroundParams
{
    public bool IsGrounded;
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


    [Header("Turn Params")]
    public float TurnAngle = 30f;
    public float TurnAcceleration = 10f;

    [Header("Hover Params")]
    public float HoverHeight = 3f;
    public float SpringConstant = 3f;
    public float Dampening = 0.99f;
    public float CheckAheadDistance = 20f;
    public float CheckAheadHeight = 4f;

    private Vector2 DesiredMovement;
    private Rigidbody Body;

    private Vector3 GroundedForward;
    private Vector3 GroundedRight;

    //turns with A and D keys
    private Vector3 ForwardDirection;

    private GroundParams GroundParams;
    // Start is called before the first frame update
    void Start()
    {
        Body = GetComponent<Rigidbody>();

        ForwardDirection = Body.transform.forward;
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

        Velocity = GetNoCameraMoveVelocity(Velocity);
        Velocity += GetHoverVelocity(Velocity);

        //apply damper
        Velocity = GetHoverDamper(Velocity);

        Body.velocity = Velocity;
    }

    public void SetMaxSpeed(float NewMaxSpeed)
    {
        Speed = NewMaxSpeed;
    }

    private Vector3 GetMoveVelocity(Vector3 CurrentVelocity)
    {
        SetGroundParams();

        //get local forward and right
        Vector3 forward = (Body.position - Camera.position).normalized;
        Vector3 right = Vector3.Cross(GroundParams.Normal, forward).normalized;
        GroundedForward = Vector3.ProjectOnPlane(forward, GroundParams.Normal);
        GroundedRight = Vector3.ProjectOnPlane(right, GroundParams.Normal);

        //get local forward and right vel
        Vector3 forwardVel = GroundedForward * DesiredMovement.x;
        Vector3 rightVel = GroundedRight * DesiredMovement.y;
        Vector3 desiredVel = (forwardVel + rightVel).normalized * Speed;

        //move current Velocity to desired, based on acceleration
        Vector3 velocity = Vector3.MoveTowards(CurrentVelocity, desiredVel, MaxAcceleration * Time.fixedDeltaTime);

        //add gravity
        velocity += -Vector3.up * Gravity * Time.fixedDeltaTime;

        return velocity;
    }

    private Vector3 GetNoCameraMoveVelocity(Vector3 CurrentVelocity)
    {
        SetGroundParams();

        //get local forward and right
        Vector3 forward = Body.transform.forward;
        Vector3 right = Body.transform.right;
        GroundedForward = Vector3.ProjectOnPlane(forward, GroundParams.Normal);
        GroundedRight = Vector3.ProjectOnPlane(right, GroundParams.Normal);

        //do turning
        Quaternion TurnQuat = Quaternion.AngleAxis(TurnAngle * DesiredMovement.y, GroundParams.Normal);
        ForwardDirection = Vector3.Slerp(ForwardDirection, TurnQuat * ForwardDirection, Time.deltaTime * TurnAcceleration);

        Vector3 desiredVel = (ForwardDirection.normalized * DesiredMovement.x) * Speed;


        //move current Velocity to desired, based on acceleration
        Vector3 velocity = Vector3.MoveTowards(CurrentVelocity, desiredVel, MaxAcceleration * Time.fixedDeltaTime);

        //add gravity
        velocity += -Vector3.up * Gravity * Time.fixedDeltaTime;

        return velocity;
    }

    private Vector3 GetHoverVelocity(Vector3 CurrentVelocity)
    {
        //try and spring the riding height at HoverHeight

        //get projected height ahead of player 
        RaycastHit hit;
        float projectedGroundHeight = GroundParams.Position.y;

        //get opposite and adjacent lengths
        float height = (Body.position.y + CheckAheadHeight) - GroundParams.Position.y;
        float lateralDistance = Mathf.Sqrt((height * height) + (CheckAheadDistance * CheckAheadDistance));

        //caluclate angle of ray
        float angle = Mathf.Atan2(height, CheckAheadDistance);
        Quaternion tiltDownRotation = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, GroundedRight);

        //Debug.DrawLine(Body.position + Vector3.up * CheckAheadHeight, Body.position + Vector3.up * CheckAheadHeight + (tiltDownRotation * GroundedForward) * lateralDistance, Color.red, 1f);

        if (Physics.Raycast(Body.position + Vector3.up * CheckAheadHeight, (tiltDownRotation * GroundedForward), out hit, lateralDistance, GroundCheckMask))
        {
            if (hit.point.y > projectedGroundHeight && Vector3.Dot(hit.normal, Vector3.up) > 0.25f)
            {
                projectedGroundHeight = hit.point.y;
            }
        }

        float yDiff = (Body.position.y - (projectedGroundHeight + HoverHeight));

        float restoringVelocity = -SpringConstant * yDiff * Time.fixedDeltaTime;

        return GroundParams.Normal * restoringVelocity;
    }

    private Vector3 GetHoverDamper(Vector3 CurrentVelocity)
    {
        //only do hover damping if "grounded"
        if (!GroundParams.IsGrounded)
        {
            return CurrentVelocity;
        }

        float currentVel = 0;

        float dampenedY = Mathf.SmoothDamp(CurrentVelocity.y, 0, ref currentVel, Dampening);

        CurrentVelocity.y = dampenedY;

        return CurrentVelocity;
    }

    //raycast down to get ground
    private void SetGroundParams()
    {
        RaycastHit hit;

        if (Physics.Raycast(Body.position, Vector3.down, out hit, HoverHeight + 1.0f, GroundCheckMask))
        {
            GroundParams.IsGrounded = true;
            GroundParams.Normal = hit.normal;
            GroundParams.Position = hit.point;
        }
        else
        {
            GroundParams.IsGrounded = false;
        }
    }
}
