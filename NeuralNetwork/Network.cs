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
            _weights = Utils.FillWeightArray(layerConfig, (a, b, c) => Utils.NomalizedFloat);
            _biases = Utils.FillBiasArray(layerConfig, (a, b) => Utils.NomalizedFloat);
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
            float[] activations = inputs;

            for (int layer = 0; layer < _layerConfiguration.Length - 1; layer++)
            {
                float[] nextActivations = new float[_layerConfiguration[layer + 1]];
                for (int i = 0; i < nextActivations.Length; i++)
                {
                    float z = _biases[layer][i];
                    for (int j = 0; j < activations.Length; j++)
                    {
                        z += activations[j] * _weights[layer][i][j];
                    }
                    nextActivations[i] = Sigmoid(z); ;
                }
                activations = nextActivations;
            }
            return activations;
        }


        /*****************************************************************************
        | BACK PROPAGATION                                                           |
        *****************************************************************************/
        public void ApplyTrainingData(float[] inputs, float[] outputs, float learningRate, Action<object> print = null) => ApplyTrainingData(new float[][][] { new float[][] { inputs }, new float[][] { outputs } }, learningRate, print);
        public void ApplyTrainingData(float[][][] trainingData, float learningRate, Action<object> print = null)
        {
            //print("length: " + trainingData[0].Length);

            if (trainingData.Length != 2) throw new Exception("training data must contain inputs[][] and outputs[][]");
            if (trainingData[0].Length != trainingData[1].Length) throw new Exception("inputs and outputs differ in length");

            learningRate /= trainingData[0].Length;
            float[][][] weightGradient = Utils.FillWeightArray(_layerConfiguration, (i, j, k) => 0);
            float[][] biasGradient = Utils.FillBiasArray(_layerConfiguration, (i, j) => 0);

            for (int i = 0; i < trainingData[0].Length; i++)
            {
                Network deltas = BackProp(trainingData[0][i], trainingData[1][i], print);
                float[][][] deltaWeightGradient = deltas._weights;
                float[][] deltaBiasGradient = deltas._biases;

                
                //for (int j = 0; j < deltaWeightGradient.Length; j++)
                //{
                //    string str = "";
                //    for (int k = 0; k < deltaWeightGradient[j].Length; k++)
                //        for (int l = 0; l < deltaWeightGradient[j][k].Length; l++)
                //            str += deltaWeightGradient[j][k][l] + ", ";
                //    print?.Invoke(str);
                //}

               


                Utils.AddWeights(weightGradient, deltaWeightGradient);
                Utils.AddBiases(biasGradient, deltaBiasGradient);
            }

            Utils.SubWeights(_weights, weightGradient, learningRate);
            Utils.SubBiases(_biases, biasGradient, learningRate);

            //Console.WriteLine("After");
            //Utils.PrintWeights(_weights);
        }

        private Network BackProp(float[] inputs, float[] outputs, Action<object> print)
        {
            float[][][] deltaWeights = Utils.FillWeightArray(_layerConfiguration, (i, j, k) => 0);
            float[][] deltaBiases = Utils.FillBiasArray(_layerConfiguration, (i, j) => 0);

            // FORWARD PASS
            float[] activation = inputs; // inputs for current layer (l - 1)
            float[][] activations = new float[_layerConfiguration.Length][]; // all activations
            activations[0] = activation; // set activations for inputs layer
            float[][] zs = new float[_weights.Length][]; // all pre-squished values

            for (int layer = 0; layer < zs.Length; layer++)
            {
                float[] z = new float[_weights[layer].Length];
                for (int k = 0; k < z.Length; k++)
                {
                    z[k] = _biases[layer][k];
                    for (int j = 0; j < activation.Length; j++)
                    {
                        z[k] += activation[j] * _weights[layer][k][j];
                    }
                }
                zs[layer] = z;
                activations[layer + 1] = activation = z.Select(zk => Sigmoid(zk)).ToArray();
            }

            // BACKWARD PASS
            float[] delta = GetInitialDelta(activations[activations.Length - 1], outputs, zs[zs.Length - 1]);

            deltaBiases[deltaBiases.Length - 1] = delta;

            deltaWeights[_weights.Length - 1] = new float[delta.Length][];
            for (int node = 0; node < delta.Length; node++)
            {
                deltaWeights[_weights.Length - 1][node] = new float[_weights[_weights.Length - 1][node].Length];
                for (int w = 0; w < deltaWeights[_weights.Length - 1][node].Length; w++)
                {
                    deltaWeights[_weights.Length - 1][node][w] = activations[activations.Length - 2][w] * delta[node];
                }
            }

            for (int layer = zs.Length - 2; layer >= 0; layer--)
            {
                float[] z = zs[layer];
                float[] sp = new float[z.Length];
                for (int spz = 0; spz < sp.Length; spz++) sp[spz] = SigmoidPrime(z[spz]);

                float[] newDelta = new float[z.Length];
                for (int node = 0; node < z.Length; node++)
                {
                    float sum = 0;
                    for (int next = 0; next < _weights[layer + 1].Length; next++)
                    {
                        sum += _weights[layer + 1][next][node] * delta[next];
                    }
                    newDelta[node] = sum * sp[node];
                }
                delta = newDelta;
                deltaBiases[layer] = delta;

                deltaWeights[layer] = new float[delta.Length][];
                for (int node = 0; node < delta.Length; node++)
                {
                    deltaWeights[layer][node] = new float[_weights[layer][node].Length];
                    for (int w = 0; w < deltaWeights[layer][node].Length; w++)
                    {
                        deltaWeights[layer][node][w] = activations[layer][w] * delta[node];
                    }
                }
            }

            return new Network(deltaWeights, deltaBiases);
        }

        private float[] GetInitialDelta(float[] a, float[] y, float[] z)
        {
            float[] delta = new float[a.Length];

            for (int i = 0; i < a.Length; i++)
            {
                // sp = a[i] * (1 - a[i]) ?
                delta[i] = (a[i] - y[i]) * SigmoidPrime(z[i]);
            }

            return delta;
        }

        private float[] GetInnerDelta(float[] z)
        {
            float[] sp = new float[z.Length];
            for (int spz = 0; spz < sp.Length; spz++) sp[spz] = SigmoidPrime(z[spz]);

            float[] delta = new float[z.Length];



            return delta;
        }

        public static float Sigmoid(float x)
        {
            //return x / (1 + Math.Abs(x));
            return 1 / (1 + (float)Math.Exp(-x));
        }

        public static float SigmoidPrime(float x)
        {
            float s = Sigmoid(x);
            return s * (1 - s);
        }


    }
}

// (x / (1 + |x|)) * (1 - (x / (1 + |x|)))
// (1 / (1 + e^(-x))) * (1 - (1 / (1 + e^(-x))))
