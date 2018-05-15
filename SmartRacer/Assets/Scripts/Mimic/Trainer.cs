using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Trainer : MonoBehaviour {

    public int ImageWidth = 24;
    public int ImageHeight = 24;

    public RenderTexture ImageInput;
    public Camera captureCamera;

    private List<Transform> arms = new List<Transform>();

    private NeuralNetwork.Network network;

    public bool AutoTrain = false;

    public Transform Planet1;

    public float TargetFramerate = 10f;

    public int numSamples = 10;

    private float[] angle = new float[] { 0, 0, 0 };
    private float[] angleDelta = new float[] { 0.001f, 0.002f, 0.003f };

    public List<Transform> OthersArms;

	// Use this for initialization
	void Start () {
        foreach (Transform child in transform)
        {
            arms.Add(child);
        }
        ImageInput.width = ImageWidth;
        ImageInput.height = ImageHeight;

        int numInputs = ImageWidth * ImageHeight * 4;
        int numOutputs = 2 * arms.Count;
        network = new NeuralNetwork.Network(new int[] { numInputs, 12, numOutputs});
    }

    private void Update()
    {
        if(AutoTrain)
        {
            if (Time.deltaTime > 1f / (TargetFramerate + 1) && numSamples > 1) numSamples--;
            if (Time.deltaTime < 1f / (TargetFramerate - 1)) numSamples++;
            GenerateTrainingSet(numSamples);
        }

        TestNetwork();
    }

    private float[][] GetData()
    {
        for (int i = 0; i < 3; i++)
        {
            angle[i] += angleDelta[i];
        }
        float[] coords = new float[3 * arms.Count];
        for(int i = 0; i < arms.Count; i++)
        {
            //float zRot = UnityEngine.Random.value * 2 * Mathf.PI;

            float xPos = Mathf.Cos(angle[i]);
            float yPos = Mathf.Sin(angle[i]);

            arms[i].localPosition = new Vector3(xPos * 3, yPos * 3, 0);

            coords[i * 2 + 0] = xPos / 4f + 0.5f;
            coords[i * 2 + 1] = yPos / 4f + 0.5f;
        }

        captureCamera.Render();
        RenderTexture.active = ImageInput;
        Texture2D texture = new Texture2D(ImageInput.width, ImageInput.height);
        texture.ReadPixels(new Rect(0, 0, ImageInput.width, ImageInput.height), 0, 0);
        texture.Apply();

        Color[] colors = texture.GetPixels();

        float[] values = new float[colors.Length * 4];
        for (int i = 0; i < colors.Length; i++)
        {
            values[i * 3 + 0] = colors[i].r;
            values[i * 3 + 1] = colors[i].g;
            values[i * 3 + 2] = colors[i].b;
            values[i * 3 + 3] = colors[i].a;
        }
        Destroy(texture);
        return new float[][] { values, coords };
    }

    private void DisplayInputValues(float[] rotations)
    {
        for (int i = 0; i < arms.Count; i++)
        {
            float x = (rotations[i * 3 + 0] - 0.25f) * 720;
            float y = (rotations[i * 3 + 1] - 0.25f) * 720;
            float z = (rotations[i * 3 + 2] - 0.25f * 720);
            arms[i].rotation = Quaternion.Euler(x, y, z);
        }
    }

    public void GenerateTrainingSet(int setSize)
    {
        float[][][] trainingData = new float[2][][] { new float[setSize][], new float[setSize][] };
        for (int i = 0; i < setSize; i++)
        {
            float[][] newData = GetData();
            trainingData[0][i] = newData[0];
            trainingData[1][i] = newData[1];
        }
        network.ApplyTrainingData(trainingData, 0.3f, Debug.Log);
    }

    private void TestNetwork()
    {
        float[][] newData = GetData();
        float[] outputs = network.FeedForward(newData[0]);

        float error = 0;
        string str = "";
        for (int i = 0; i < outputs.Length; i++)
        {
            str += ((newData[1][i] - 0.5f) * 4f).ToString().PadRight(12) + ((outputs[i] - 0.5f) * 4f).ToString().PadRight(12) + "|   ";
            error += Mathf.Abs(newData[1][i] - outputs[i]);
        }

        Debug.Log(str + "    Error: " + error);
        for (int i = 0; i < OthersArms.Count; i++)
        {
            Vector3 pos = new Vector3((outputs[i * 2 + 0] - 0.5f) * 12, (outputs[i * 2 + 1] - 0.5f) * 12, 0);
            OthersArms[i].localPosition = pos;
        }

    }
}
