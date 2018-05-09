using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;


public class TrackGenerator : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("this is a test");
        GenePool pool = new GenePool();
        pool.Initialize(new int[]{ 1, 2, 3 });

        Debug.Log(pool.Individuals[0].FeedForward(new float[] { 1 })[0]);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
