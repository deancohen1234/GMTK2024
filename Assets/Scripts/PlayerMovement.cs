using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform Camera;

    [Header("Movement Params")]
    public float Speed = 4f;
    public float MaxAcceleration = 6f;
    public float Gravity = 20f;
    public LayerMask GroundCheckMask = ~0;

    private Vector2 DesiredMovement;
    private Rigidbody Body;
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
        Vector3 GroundUp = GetGroundVector();

        Vector3 GroundedForward = Vector3.ProjectOnPlane(Camera.forward, GroundUp);
        Vector3 GroundedRight = Vector3.ProjectOnPlane(Camera.right, GroundUp);

        Vector3 ForwardVel = GroundedForward * DesiredMovement.x;
        Vector3 RightVel = GroundedRight * DesiredMovement.y;

        Vector3 DesiredVel = (ForwardVel + RightVel).normalized * Speed;

        Vector3 Velocity = Vector3.MoveTowards(Body.velocity, DesiredVel, MaxAcceleration * Time.fixedDeltaTime);

        Velocity += -Vector3.up * Gravity * Time.fixedDeltaTime;

        Body.velocity = Velocity;
    }

    //raycast down to get ground
    private Vector3 GetGroundVector()
    {
        RaycastHit hit;

        if (Physics.Raycast(Body.position, Vector3.down, out hit, 1.0f, GroundCheckMask))
        {
            return hit.normal;
        }

        return Vector3.up;
    }
}
