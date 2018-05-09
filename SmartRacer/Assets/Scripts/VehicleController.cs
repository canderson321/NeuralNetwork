using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour {

    
    public float DriveValue = 0;
    public float SteerValue = 0;

   // private float wheelAngle = 0;


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
	
    private Vector3 GetWheelForce(Vector3 forward, Vector3 currentVelocity, bool isDriveWheel = false)
    {
        Quaternion rotationToForward = Quaternion.LookRotation(forward);
        Vector3 relaviteVelocity = Quaternion.Inverse(rotationToForward) * currentVelocity;
        Vector3 force = Vector3.zero;
        force.x = -relaviteVelocity.x * SideWheelFriction;
        force.z = -relaviteVelocity.z * ForwardWheelFriction;

        if (isDriveWheel) force.z += Acceleration * DriveValue;

        return rotationToForward * force;

        //if(isDriveWheel)Debug.DrawLine(position, position + (rotationToForward * force * 10), Color.green);
        //if(!isDriveWheel)Debug.DrawLine(position, position + (rotationToForward * force * 10), Color.red);
    }

    public void FixedUpdate()
    {
        Vector3 FrontForward = Quaternion.Euler(0, SteerValue * MaxTurnAngle, 0) * FrontWheels.forward;
        Vector3 FrontForce = GetWheelForce(FrontForward, rb.GetPointVelocity(FrontWheels.position), true);
        rb.AddForceAtPosition(FrontForce, FrontWheels.position);

        Vector3 RearForce = GetWheelForce(transform.forward, rb.GetPointVelocity(RearWheels.position), false);
        rb.AddForceAtPosition(RearForce, RearWheels.position);
    }
}
