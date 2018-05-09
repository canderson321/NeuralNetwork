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

        public static float Sigmoid(float x)
        {
            return x / (1 + Math.Abs(x));
        }
    }
}
