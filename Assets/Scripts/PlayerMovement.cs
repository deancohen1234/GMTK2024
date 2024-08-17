using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform Camera;

    [Header("Movement Parans")]
    public float Speed = 4f;

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
        Vector3 MoveDirection = Camera.forward;

        Vector3 ForwardVel = Camera.forward * DesiredMovement.x;
        Vector3 RightVel = Camera.right * DesiredMovement.y;

        Vector3 Vel = (ForwardVel + RightVel).normalized * Speed;
        Body.AddForce(Vel);
    }
}
