using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork
{
    static class Utils
    {
        public static Random Random = new Random();
        public static float PositiveFloat { get => (float)Random.NextDouble(); }
        public static float NomalizedFloat { get => (float)Random.NextDouble() * 2f - 1; }

        public static float[][][] FillWeightArray(int[] config, Func<int, int, int, float> fillMethod)
        {
            float[][][] result = new float[config.Length - 1][][];
            for (int i = 0; i < config.Length - 1; i++)
            {
                result[i] = new float[config[i + 1]][];
                for (int j = 0; j < config[i + 1]; j++)
                {
                    result[i][j] = new float[config[i]];
                    for (int k = 0; k < config[i]; k++)
                    {
                        result[i][j][k] = fillMethod(i, j, k);
                    }
                }
            }
            return result;
        }

        public static float[][] FillBiasArray(int[] config, Func<int, int, float> fillMethod)
        {
            float[][] result = new float[config.Length - 1][];
            for (int i = 0; i < config.Length - 1; i++)
            {
                result[i] = new float[config[i + 1]];
                for (int j = 0; j < config[i + 1]; j++)
                {
                    result[i][j] = fillMethod(i, j);
                }
            }
            return result;
        }

        public static void AddWeights(float[][][] a, float[][][] b)
        {
            for (int i = 0; i < a.Length; i++)
                for (int j = 0; j < a[i].Length; j++)
                    for (int k = 0; k < a[i][j].Length; k++)
                        a[i][j][k] += b[i][j][k];
        }

        public static void AddBiases(float[][] a, float[][] b)
        {
            for (int i = 0; i < a.Length; i++)
                for (int j = 0; j < a[i].Length; j++)
                    a[i][j] += b[i][j];
        }

        public static void SubWeights(float[][][] a, float[][][] b, float learningRate)
        {
            for (int i = 0; i < a.Length; i++)
                for (int j = 0; j < a[i].Length; j++)
                    for (int k = 0; k < a[i][j].Length; k++)
                        a[i][j][k] -= b[i][j][k] * learningRate;
        }

        public static void SubBiases(float[][] a, float[][] b, float learningRate)
        {
            for (int i = 0; i < a.Length; i++)
                for (int j = 0; j < a[i].Length; j++)
                    a[i][j] -= b[i][j] * learningRate;
        }


        public static float[] ElementwiseMultiply(float[] a, float[] b)
        {
            float[] result = new float[a.Length];

            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] * b[i];
            }

            return result;
        }

        private static float[] ElementwiseSubtract(float[] a, float[] b)
        {
            float[] result = new float[a.Length];

            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] - b[i];
            }

            return result;
        }

        public static void PrintWeights(float[][][] weights)
        {
            for (int layer = 0; layer < weights.Length; layer++)
            {
                for (int node = 0; node < weights[layer].Length; node++)
                {
                    for (int w = 0; w < weights[layer][node].Length; w++)
                    {
                        Console.Write(weights[layer][node][w].ToString().PadRight(12));
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }
    }
}
