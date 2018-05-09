using System;
using System.Linq;
using System.Collections.Generic;

namespace NeuralNetwork
{
    public class Network
    {
        private int[] _layerConfiguration;
        private float[][][] _weights;
        private float[][] _biases;

        public int[] GetLayerConfiguration() { return _layerConfiguration; }
        public float[][][] GetWeights() { return _weights; }
        public float[][] GetBiases() { return _biases; }

        public Network(int[] layerConfig)
        {
            if (layerConfig.Length < 1)
                throw new Exception("One or more layers are required.");
            if (layerConfig.Any(x => x <= 0))
                throw new Exception("Layer sizes must be greater than zero.");

            _layerConfiguration = layerConfig;

            // Create random biases
            for (int i = 0; i < layerConfig.Length - 1; i++)
            {
                _biases[i] = new float[layerConfig[i + 1]];
                for (int j = 0; j < layerConfig[i + 1]; j++)
                {
                    _biases[i][j] = Utils.NomalizedFloat;
                }
            }

            // Create random weights
            _weights = new float[layerConfig.Length - 1][][];
            for (int i = 0; i < layerConfig.Length - 1; i++)
            {
                _weights[i] = new float[layerConfig[i + 1]][];
                for (int j = 0; j < layerConfig[i + 1]; j++)
                {
                    _weights[i][j] = new float[layerConfig[i]];
                    for (int k = 0; k < layerConfig[i]; k++)
                    {
                        _weights[i][j][k] = Utils.NomalizedFloat;
                    }
                }
            }
        }

        public Network(float[][][] weights, float[][] biases)
        {
            _weights = weights;
            _biases = biases;
            _layerConfiguration = new int[weights.Length + 1];
            _layerConfiguration[0] = weights[0][0].Length;
            for (int i = 0; i < weights.Length; i++)
            {
                _layerConfiguration[i + 1] = _weights[i].Length;
            }
        }

        public float[] FeedForward(float[] inputs)
        {
            if (inputs.Length != _layerConfiguration[0]) throw new Exception("Input layer was specified with <" + _layerConfiguration[0] + " Nodes>. <" + inputs.Length + "> found.");
            float[] a = inputs;

            for (int layer = 0; layer < _layerConfiguration.Length - 1; layer++)
            {
                float[] newA = new float[_layerConfiguration[layer + 1]];
                for (int i = 0; i < newA.Length; i++)
                {
                    float sum = 0;
                    for (int j = 0; j < a.Count(); j++)
                    {
                        sum += a[j] * _weights[layer][i][j];
                    }
                    sum += _biases[layer][i];
                    sum = Utils.Sigmoid(sum);
                    newA[i] = sum;
                }
                a = newA;
            }
            return a;
        }
    }
}
