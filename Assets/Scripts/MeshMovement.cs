using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMovement : MonoBehaviour
{
    public Transform MeshTransform;
    public Transform ThrusterTransform;

    public float RotationSpeed = 10f;
    [Range(0, 1)]
    public float ThrusterLagPercent = 0.25f;
    public float ForwardLogRate = 0.5f;

    [Header("Roll Shit")]
    public float MaxRollAngle = 10f;
    public float RollSpeed = 4f;
    public float ReturnRollSpeed = 4f;

    private Rigidbody Body;
    private Vector3 VelocityDirection;

    private Vector3 LastForwardDirection;
    private float NextForwardLogTime = 0;
    private float CurrentRoll;
    private float CurrentDotDiff;

    private const float MAX_ROLL_DOT = 0.5f;

    void Start()
    {
        Body = GetComponent<Rigidbody>();

        VelocityDirection = MeshTransform.forward;
    }

    void Update()
    {
        Quaternion MeshRotation = GetYawRotation(false);
        Quaternion ThrusterRotation = GetYawRotation(true);

        Quaternion RollRotation = GetRollRotation(false);

        MeshTransform.rotation = RollRotation * MeshRotation;
        ThrusterTransform.rotation = ThrusterRotation;


        LastForwardDirection = MeshTransform.forward;
    }

    private void FixedUpdate()
    {
        //hack to stop weird drop in beginning
        if (Time.time > 1.5f && Body.velocity.sqrMagnitude >= 0.5f)
        {
            VelocityDirection = Body.velocity.normalized;
        }

        //if ready cache forward direction
        if (Time.time > NextForwardLogTime)
        {
            NextForwardLogTime = Time.time + ForwardLogRate;
            //LastForwardDirection = VelocityDirection;
        }
    }

    private Quaternion GetYawRotation(bool isThruster)
    {
        if (isThruster)
        {
            //lag the thrusterBehind
            Vector3 DesiredThrusterForward = Vector3.Slerp(ThrusterTransform.forward, VelocityDirection, RotationSpeed * ThrusterLagPercent * Time.deltaTime);

            return Quaternion.LookRotation(DesiredThrusterForward);            
        }
        
        else
        {
            //point mesh front to velocity
            Vector3 DesiredForward = Vector3.Slerp(MeshTransform.forward, VelocityDirection, RotationSpeed * Time.deltaTime);

            return Quaternion.LookRotation(DesiredForward);
        }

        
    }

    private Quaternion GetRollRotation(bool isThruster)
    {
        //get diff between current forward and this old one
        float dotDiff = 1f - Vector3.Dot(VelocityDirection, LastForwardDirection);

        float xInput = (Input.GetAxis("Mouse X"));

        if (dotDiff <= 0.001f || Mathf.Abs(xInput) <= 0.005f)
        {
            CurrentRoll = Mathf.MoveTowards(CurrentRoll, 0, Time.deltaTime * ReturnRollSpeed);

            return Quaternion.AngleAxis(CurrentRoll, MeshTransform.forward);
        }

        CurrentDotDiff = Mathf.Clamp(dotDiff + CurrentDotDiff, 0f, MAX_ROLL_DOT);

        //derive roll from this
        float rollDirection = -Mathf.Sign(xInput);

        float desiredRollAmount = Mathf.Clamp(MathStatics.Map(CurrentDotDiff, 0, MAX_ROLL_DOT, 0, MaxRollAngle), 0, MaxRollAngle) * rollDirection;

        Debug.Log("Desired: " + desiredRollAmount);

        CurrentRoll = Mathf.MoveTowards(CurrentRoll, desiredRollAmount, Time.deltaTime * RollSpeed);

        return Quaternion.AngleAxis(CurrentRoll, MeshTransform.forward);
    }
}
