using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleDriver : MonoBehaviour {

    private VehicleController vc;
	// Use this for initialization
	void Start () {
        vc = GetComponent<VehicleController>();
	}
	
	// Update is called once per frame
	void Update () {
        vc.SteerValue = 0;
        if (Input.GetKey(KeyCode.A)) vc.SteerValue = -1;
        if (Input.GetKey(KeyCode.D)) vc.SteerValue = 1;

        vc.DriveValue = 0;
        if (Input.GetKey(KeyCode.S)) vc.DriveValue = -1;
        if (Input.GetKey(KeyCode.W)) vc.DriveValue = 1;
    }
}
