using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;

namespace NeuralNetwork
{
    public class GenePool
    {
        public int PoolSize = 100;
        public float CrossoverRate = 0.1f;
        public float MutationRate = 0.0025f;

        public bool UseAdaptiveMutationRate = true;
        public float MutationsPerIndividual = 0.6f;

        private int _numMutations = 0;
        private int _generation = 0;
        private List<Individual> _individuals = new List<Individual>();

        public void Initialize(int[] layerConfiguration)
        {
            for (int i = 0; i < PoolSize; i++)
            {
                _individuals.Add(new Individual(layerConfiguration));
            }
        }

        public void NexGeneration()
        {
            if (UseAdaptiveMutationRate)
                MutationRate *= MutationsPerIndividual / ((float)(_numMutations + 1) / PoolSize);

            _numMutations = 0;
            _generation++;
            List<Individual> nextGeneration = new List<Individual>();
            for (int i = 0; i < PoolSize; i++)
            {
                nextGeneration.Add(MakeChild());
            }
            _individuals = nextGeneration;
        }

        private Individual MakeChild()
        {
            Individual parentA = GetParent();
            Individual parentB = GetParent();

            float[][][] childWeights = GetChildWeights(parentA, parentB);
            float[][] childBiases = GetChildBiases(parentA, parentB);

            return new Individual(childWeights, childBiases);
        }

        private float[][][] GetChildWeights(Individual parentA, Individual parentB)
        {
            bool inheritFromA = true;
            float[][][] a = parentA.GetWeights();
            float[][][] b = parentB.GetWeights();

            float[][][] c = new float[a.Length][][];
            for (int layer = 0; layer < a.Length; layer++)
            {
                c[layer] = new float[a[layer].Length][];
                for (int column = 0; column < a[layer].Length; column++)
                {
                    c[layer][column] = new float[a[layer][column].Length];
                    for (int node = 0; node < a[layer][column].Length; node++)
                    {
                        if (Math.Abs(Utils.PositiveFloat) < MutationRate)
                        {
                            c[layer][column][node] = Utils.NomalizedFloat;
                            _numMutations++;
                        }
                        else
                        {
                            c[layer][column][node] = inheritFromA ? a[layer][column][node] : b[layer][column][node];
                        }
                        if (Math.Abs(Utils.PositiveFloat) < CrossoverRate)
                        {
                            inheritFromA = !inheritFromA;
                        }
                    }
                }
            }
            return c;
        }

        public float[][] GetChildBiases(Individual parentA, Individual parentB)
        {
            bool inheritFromA = true;
            float[][] a = parentA.GetBiases();
            float[][] b = parentB.GetBiases();

            float[][] c = new float[a.Length][];
            for (int layer = 0; layer < a.Length; layer++)
            {
                c[layer] = new float[a[layer].Length];
                for (int column = 0; column < a[layer].Length; column++)
                {
                    if (Math.Abs(Utils.PositiveFloat) < MutationRate)
                    {
                        c[layer][column] = (float)Utils.NomalizedFloat;
                        _numMutations++;
                    }
                    else
                        c[layer][column] = inheritFromA ? a[layer][column] : b[layer][column];
                    if (Math.Abs(Utils.PositiveFloat) < CrossoverRate) inheritFromA = !inheritFromA;
                }
            }

            return c;
        }

        private Individual GetParent()
        {
            int[] selections = new int[10];
            for (int i = 0; i < 10; i++)
            {
                selections[i] = Utils.Random.Next(0, PoolSize);
            }
            return _individuals[selections.Max()];
        }
    }

    public class Individual : Network
    {
        public Individual(int[] LayerSizes) : base(LayerSizes) { }
        public Individual(float[][][] weights, float[][] biases) : base(weights, biases) { }
        public float fitness = 0;
    }
}
