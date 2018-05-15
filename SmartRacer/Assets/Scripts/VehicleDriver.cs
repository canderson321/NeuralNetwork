using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;

[RequireComponent(typeof(VehicleController))]
public class VehicleDriver : MonoBehaviour {

    public bool IsPlayerControlled = false;
    private bool _isPlayerControlled = false;

    public bool FollowedCar = false;
    public bool DebugCar = false;

    private Action DoUpdate;
    public Individual network;
    private TrackGenerator track;

    private int node = 0;
    private int lastNode = 0;

    private float relativeVelocity;

    private VehicleController vc;
	// Use this for initialization
	void Start () {
        vc = GetComponent<VehicleController>();
        track = FindObjectOfType<TrackGenerator>();
        DoUpdate = AIUpdate;
	}

	// Update is called once per frame
	void Update () {
        

    }

    public void FixedUpdate()
    {
        if (FollowedCar)
        {
            Transform cam = Camera.main.transform;
            Vector3 target = transform.position + transform.TransformDirection(new Vector3(0, 5, -5));
            cam.position = Vector3.Lerp(cam.position, target, 0.1f);
            cam.rotation = transform.rotation;
            cam.Rotate(25, 0, 0, Space.Self);
        }

        if (_isPlayerControlled != IsPlayerControlled)
        {
            _isPlayerControlled = IsPlayerControlled;
            DoUpdate = IsPlayerControlled ? (Action)PlayerUpdate : (Action)AIUpdate;
        }

        if (DoUpdate != null) DoUpdate();
    }

    private void PlayerUpdate()
    {
        vc.SteerValue = 0;
        if (Input.GetKey(KeyCode.A)) vc.SteerValue = -1;
        if (Input.GetKey(KeyCode.D)) vc.SteerValue = 1;

        vc.DriveValue = 0;
        if (Input.GetKey(KeyCode.S)) vc.DriveValue = -1;
        if (Input.GetKey(KeyCode.W)) vc.DriveValue = 1;
    }

    private void AIUpdate()
    {
        if (network == null) return;
        ApplyNetworkOutputs(UpdateNetwork());
        GetFitness();
        lastNode = node;
    }

    /*  INPUTS
     *  0       current speed
        1-4     wall sensor rays
        5-15    track node angle deltas
        16-17   alignment with track direction
    */
    private float[] UpdateNetwork()
    {
        

        float[] inputs = new float[network.NumInputs];

        // CURRENT FORWARD VELOCITY
        inputs[0] = relativeVelocity = transform.InverseTransformVector(GetComponent<Rigidbody>().velocity).z;

        // WALL SENSORS
        int SensorLength = 20;
        RaycastHit hit;
        Ray ray;
        int mask = 1 << LayerMask.NameToLayer("Default");
        Vector3 startPos = transform.position + new Vector3(0, -1.5f, 0);
        ray = new Ray(startPos, transform.TransformVector(new Vector3(1, 0, 1).normalized * SensorLength));
        if (Physics.Raycast(ray, out hit, SensorLength, mask))
        {
            inputs[1] = (SensorLength - hit.distance) / SensorLength;
            if (DebugCar) Debug.DrawRay(ray.origin, ray.direction * 15, new Color(1 - inputs[1], 0, inputs[1], 1));
        }
        else inputs[1] = 0;
        ray = new Ray(startPos, transform.TransformVector(new Vector3(-1, 0, 1).normalized * SensorLength));
        if (Physics.Raycast(ray, out hit, SensorLength, mask))
        {
            inputs[2] = (SensorLength - hit.distance) / SensorLength;
            if (DebugCar) Debug.DrawRay(ray.origin, ray.direction * 15, new Color(1 - inputs[2], 0, inputs[2], 1));
        }
        else inputs[2] = 0;
        ray = new Ray(startPos, transform.TransformVector(new Vector3(1, 0, -1).normalized * SensorLength));
        if (Physics.Raycast(ray, out hit, SensorLength, mask))
        {
            inputs[3] = (SensorLength - hit.distance) / SensorLength;
            if (DebugCar) Debug.DrawRay(ray.origin, ray.direction * 15, new Color(1 - inputs[3], 0, inputs[3], 1));
        }
        else inputs[3] = 0;
        ray = new Ray(startPos, transform.TransformVector(new Vector3(-1, 0, -1).normalized * SensorLength));
        if (Physics.Raycast(ray, out hit, SensorLength, mask))
        {
            inputs[4] = (SensorLength - hit.distance) / SensorLength;
            if (DebugCar) Debug.DrawRay(ray.origin, ray.direction * 15, new Color(1 - inputs[4], 0, inputs[4], 1));
        }
        else inputs[4] = 0;

        // TRACK LOOK-AHEAD
        int NumLookAheadNodes = 10;
        int LookAheadNodeDist = 10;
        float angle = Vector3.Angle(Vector3.right, transform.position);
        if (transform.position.z < 0) angle = 360 - angle;
        node = Mathf.FloorToInt(angle / 360 * track.NumNodes);

        int prev = node - LookAheadNodeDist; if (prev < 0) prev += track.NumNodes;
        int curr = node;
        int next = (node + LookAheadNodeDist) % track.NumNodes;

        for (int i = 0; i < NumLookAheadNodes; i++)
        {
            Vector3 fromLast = track.Nodes[curr] - track.Nodes[prev];
            Vector3 toNext = track.Nodes[next] - track.Nodes[curr];
            float deltaAngle = Vector3.Angle(fromLast, toNext);
            if (Vector3.Cross(fromLast, toNext).y >= 0) deltaAngle = -deltaAngle;
            inputs[4 + i] = deltaAngle / 60f;

            if (DebugCar) Debug.DrawRay(track.Nodes[curr], track.Nodes[next] - track.Nodes[curr], new Color(0, i / (float)NumLookAheadNodes, 1 - i / (float)NumLookAheadNodes, 1));

            prev = curr; curr = next; next = (next + LookAheadNodeDist) % track.NumNodes;
        }

        // VEHICLE ORIENTATION
        inputs[15] = Vector3.Dot(track.Nodes[node] - track.Nodes[lastNode], transform.forward);
        inputs[16] = Vector3.Dot(track.Nodes[node] - track.Nodes[lastNode], transform.right);

        return network.FeedForward(inputs);
    }

    /*  OUTPUTS
     *  0       vehicle drive
     *  1       vehicle steering
    */
    private void ApplyNetworkOutputs(float[] outputs)
    {
        vc.DriveValue = outputs[0] * 2;
        vc.SteerValue = outputs[1] * 2;
    }

    private void GetFitness()
    {
        if (node > lastNode && node - lastNode < 100) network.fitness += (node - lastNode) * 50;
        if (node < lastNode || node - lastNode > 100) network.fitness -= 25;

        network.fitness += relativeVelocity;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!IsPlayerControlled) network.fitness -= collision.impulse.sqrMagnitude * 160;
    }

    public void OnCollisionStay(Collision collision)
    {
        if (!IsPlayerControlled) network.fitness -= 40;
    }
}
