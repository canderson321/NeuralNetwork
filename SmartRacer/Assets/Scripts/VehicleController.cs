using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour {

    public float WheelAngle;
    public Transform FrontWheels;
    public Transform RearWheels;
    private float wheelWidth;

    private Rigidbody rb;

    public float Acceleration = 10;
    public float MaxTurnAngle = 30;

    public float ForwardWheelFriction = 0.2f;
    public float SideWheelFriction = 0.8f;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        wheelWidth = transform.localScale.x;

        Vector3 v = new Vector3(0, 0, 1).normalized;
        Vector3 f = new Vector3(1, 0, 0).normalized;

        Quaternion q = Quaternion.Inverse(Quaternion.LookRotation(f));

        Debug.Log("A: " + q * v);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.A)) WheelAngle = -MaxTurnAngle;
        else if (Input.GetKey(KeyCode.D)) WheelAngle = MaxTurnAngle;
        else WheelAngle = 0;
    }

    private Vector3 GetWheelForce(Vector3 forward, Vector3 currentVelocity, bool isDriveWheel = false)
    {
        Quaternion rotationToForward = Quaternion.LookRotation(forward);
        Vector3 relaviteVelocity = Quaternion.Inverse(rotationToForward) * currentVelocity;
        Vector3 force = Vector3.zero;
        force.x = -relaviteVelocity.x * SideWheelFriction;
        force.z = -relaviteVelocity.z * ForwardWheelFriction;

        if (isDriveWheel && Input.GetKey(KeyCode.W)) force.z += Acceleration;
        if (isDriveWheel && Input.GetKey(KeyCode.S)) force.z -= Acceleration;

        return rotationToForward * force;

        //if(isDriveWheel)Debug.DrawLine(position, position + (rotationToForward * force * 10), Color.green);
        //if(!isDriveWheel)Debug.DrawLine(position, position + (rotationToForward * force * 10), Color.red);
    }

    public void FixedUpdate()
    {
        Vector3 FrontForward = Quaternion.Euler(0, WheelAngle, 0) * FrontWheels.forward;
        Vector3 FrontForce = GetWheelForce(FrontForward, rb.GetPointVelocity(FrontWheels.position), true);
        rb.AddForceAtPosition(FrontForce, FrontWheels.position);

        Vector3 RearForce = GetWheelForce(transform.forward, rb.GetPointVelocity(RearWheels.position), false);
        rb.AddForceAtPosition(RearForce, RearWheels.position);
    }
}
