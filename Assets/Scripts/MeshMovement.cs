using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMovement : MonoBehaviour
{
    public Transform CharacterContainer;
    public Transform BodyContainer;
    public Transform ThrusterTransform;

    public float RotationSpeed = 10f;
    [Range(0, 1)]
    public float ThrusterLagPercent = 0.25f;

    [Header("Hovering")]
    public AnimationCurve[] HoverShapes;
    public float HoverFrequency = 1f;
    public float HoverAmplitude = 0.5f;

    [Header("Roll Shit")]
    public float MaxRollAngle = 10f;
    public float RollSpeed = 4f;
    public float ReturnRollSpeed = 4f;

    private Rigidbody Body;
    private Vector3 VelocityDirection;

    private float CurrentRollAngle;

    private float StartingLocalY;
    private AnimationCurve RandomCurve;
    private float HoverTime;

    void Start()
    {
        Body = GetComponent<Rigidbody>();

        VelocityDirection = BodyContainer.forward;

        StartingLocalY = BodyContainer.localPosition.y;
        RandomCurve = HoverShapes[Random.Range(0, HoverShapes.Length)];
    }

    void Update()
    {
        Quaternion MeshRotation = GetYawRotation(false);
        Quaternion ThrusterRotation = GetYawRotation(true);

        Quaternion RollRotation = GetNoCameraRollRotation(false);

        UpdateHover();

        BodyContainer.rotation = RollRotation * MeshRotation;
        ThrusterTransform.rotation = ThrusterRotation;
    }

    private void FixedUpdate()
    {
        //hack to stop weird drop in beginning
        if (Time.time > 1.5f && Body.velocity.sqrMagnitude >= 0.5f)
        {
            VelocityDirection = Body.velocity.normalized;
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
            Vector3 DesiredForward = Vector3.Slerp(BodyContainer.forward, VelocityDirection, RotationSpeed * Time.deltaTime);

            return Quaternion.LookRotation(DesiredForward);
        }     
    }

    private Quaternion GetNoCameraRollRotation(bool isThruster)
    {
        float xInput = (Input.GetAxis("Horizontal"));

        float sign = Mathf.Sign(xInput);

        if (Mathf.Abs(xInput) <= 0.1f)
        {
            sign = 0;
        }

        float acceleration = sign == 0 ? ReturnRollSpeed : RollSpeed;

        CurrentRollAngle = Mathf.MoveTowards(CurrentRollAngle, MaxRollAngle * -sign, Time.deltaTime * acceleration);

        return Quaternion.AngleAxis(CurrentRollAngle, BodyContainer.forward);
    }

    private void UpdateHover()
    {
        HoverTime += Mathf.Min(Time.deltaTime * HoverFrequency, 1f);

        float HoverVal = RandomCurve.Evaluate(HoverTime);
        Vector3 HoverPosition = new Vector3(CharacterContainer.localPosition.x, StartingLocalY + HoverVal, CharacterContainer.localPosition.z);

        CharacterContainer.localPosition = HoverPosition;

        if (HoverTime >= 1)
        {
            //pick new curve
            RandomCurve = HoverShapes[Random.Range(0, HoverShapes.Length)];
            HoverTime = 0;
        }
    }
}
