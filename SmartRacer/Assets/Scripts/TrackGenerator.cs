using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class TrackGenerator : MonoBehaviour
{

    [Header("Track Size")]
    public int NumNodes = 64;
    public int TrackRadius = 50;
    public int TrackWidth = 10;

    [Header("Track Generation Variables")]
    public float NoiseScale = 0.5f;
    public float NoiseFactor = 0.5f;

    [Header("Debugging Lines")]
    public bool ShowNodes = true;
    public bool ShowSpokes = false;
    public bool ShowOuterWall = false;
    public bool ShowInnerWall = false;
    public bool ShowCrossRoadLines = false;
    public bool ShowTenMeterMarks = false;


    public List<Vector3> Nodes;
    private List<Vector3> InnerWall;
    private List<Vector3> OuterWall;

    [HideInInspector]
    public DateTime updateTime;


    public void GenerateTrack()
    {
        //CarManager manager = GetComponent<CarManager>();
        //if (manager != null) manager.NextGen();
        updateTime = DateTime.Now;
        float xOffset = UnityEngine.Random.Range(-1000f, 1000f);
        float yOffset = UnityEngine.Random.Range(-1000f, 1000f);

        Nodes = new List<Vector3>();
        OuterWall = new List<Vector3>();
        InnerWall = new List<Vector3>();


        for (int i = 0; i < NumNodes; i++)
        {
            float x = Mathf.Cos(Mathf.PI * 2 * i / NumNodes);
            float y = Mathf.Sin(Mathf.PI * 2 * i / NumNodes);
            float noise = Mathf.PerlinNoise(x * NoiseScale + xOffset, y * NoiseScale + yOffset) * 2 * NoiseFactor * TrackRadius + TrackRadius;
            Nodes.Add(new Vector3(x * noise, 0, y * noise));
        }

        for (int i = 0; i < Nodes.Count; i++)
        {
            Vector3 prev = Nodes[i > 0 ? i - 1 : NumNodes - 1];
            Vector3 next = Nodes[(i + 1) % NumNodes];
            Vector3 tangent = (next - prev).normalized;
            Vector3 toWall = Vector3.Cross(Vector3.up, tangent) * TrackWidth / 2;
            OuterWall.Add(Nodes[i] + toWall);
            InnerWall.Add(Nodes[i] - toWall);
        }
        BuildMesh();

        Vector3 toGround = new Vector3(0, 1.6f, 0);
        foreach(VehicleController vc in FindObjectsOfType<VehicleController>())
        {
            vc.transform.position = Nodes[0] + toGround;
            vc.transform.forward = Nodes[1] - Nodes[0];
            Rigidbody rb = vc.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.isKinematic = false;
        }
    }

    private void BuildMesh()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        List<Vector3> wallVerts = new List<Vector3>();
        List<int> wallTris = new List<int>();

        for (int i = 0; i < Nodes.Count; i++)
        {
            verts.Add(InnerWall[i]);
            wallVerts.Add(InnerWall[i]);
            wallVerts.Add(new Vector3(InnerWall[i].x, 1, InnerWall[i].z));

            verts.Add(OuterWall[i]);
            wallVerts.Add(OuterWall[i]);
            wallVerts.Add(new Vector3(OuterWall[i].x, 1, OuterWall[i].z));

            int start = i * 4;
            int numVerts = NumNodes * 4;

            // Lower: 0 2 4 6

            wallTris.Add((start + 0) % numVerts);
            wallTris.Add((start + 1) % numVerts);
            wallTris.Add((start + 5) % numVerts);
            wallTris.Add((start + 0) % numVerts);
            wallTris.Add((start + 5) % numVerts);
            wallTris.Add((start + 4) % numVerts);

            wallTris.Add((start + 2) % numVerts);
            wallTris.Add((start + 6) % numVerts);
            wallTris.Add((start + 3) % numVerts);
            wallTris.Add((start + 3) % numVerts);
            wallTris.Add((start + 6) % numVerts);
            wallTris.Add((start + 7) % numVerts);

            start = i * 2;
            numVerts = NumNodes * 2;

            tris.Add((start + 0) % numVerts);
            tris.Add((start + 2) % numVerts);
            tris.Add((start + 1) % numVerts);
            tris.Add((start + 1) % numVerts);
            tris.Add((start + 2) % numVerts);
            tris.Add((start + 3) % numVerts);
        }

        for (int i = 0; i < Nodes.Count; i++)
        {
            wallVerts.Add(InnerWall[i]);
            wallVerts.Add(new Vector3(InnerWall[i].x, 1, InnerWall[i].z));

            wallVerts.Add(OuterWall[i]);
            wallVerts.Add(new Vector3(OuterWall[i].x, 1, OuterWall[i].z));

            int start = i * 4;
            int numVerts = NumNodes * 4;

            wallTris.Add(numVerts + (start + 4) % numVerts);
            wallTris.Add(numVerts + (start + 5) % numVerts);
            wallTris.Add(numVerts + (start + 0) % numVerts);
            wallTris.Add(numVerts + (start + 5) % numVerts);
            wallTris.Add(numVerts + (start + 1) % numVerts);
            wallTris.Add(numVerts + (start + 0) % numVerts);

            wallTris.Add(numVerts + (start + 7) % numVerts);
            wallTris.Add(numVerts + (start + 6) % numVerts);
            wallTris.Add(numVerts + (start + 3) % numVerts);
            wallTris.Add(numVerts + (start + 3) % numVerts);
            wallTris.Add(numVerts + (start + 6) % numVerts);
            wallTris.Add(numVerts + (start + 2) % numVerts);
        }

        Mesh wallMesh = new Mesh();
        GetComponentsInChildren<MeshFilter>()[1].mesh = wallMesh;
        wallMesh.vertices = wallVerts.ToArray();
        wallMesh.triangles = wallTris.ToArray();
        wallMesh.RecalculateNormals();

        MeshCollider cmc = GetComponentsInChildren<MeshCollider>()[1];
        cmc.sharedMesh = null;
        cmc.sharedMesh = wallMesh;



        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (ShowNodes) Debug.DrawLine(Nodes[i], Nodes[(i + 1) % NumNodes], Color.yellow);
            if (ShowInnerWall) Debug.DrawLine(InnerWall[i], InnerWall[(i + 1) % NumNodes], Color.red);
            if (ShowSpokes) Debug.DrawLine(Nodes[i], Vector3.zero, Color.blue);
            if (ShowOuterWall) Debug.DrawLine(OuterWall[i], OuterWall[(i + 1) % NumNodes], Color.red);
            if (ShowCrossRoadLines) Debug.DrawLine(OuterWall[i], InnerWall[i], Color.green);
        }
        if (ShowTenMeterMarks)
        {
            Debug.DrawLine(new Vector3(0, 0, -10), new Vector3(0, 0, 10));
            for (int i = 10; i < 100; i += 10)
            {
                Debug.DrawLine(new Vector3(i, 0, -5), new Vector3(i, 0, 5));
                Debug.DrawLine(new Vector3(-i, 0, -5), new Vector3(-i, 0, 5));
            }
        }
    }
}
