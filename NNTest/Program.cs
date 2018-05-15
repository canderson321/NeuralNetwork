using System;
using System.Linq;
using NeuralNetwork;

namespace NetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //int numInputs = 100;
            //float[] inputs = new float[numInputs];
            //GenePool pool = new GenePool();

            //pool.Initialize(new int[] { numInputs, 100, 1 });

            //while (true)
            //{
            //    pool.NextGeneration();

            //    for (int i = 0; i < numInputs; i++) inputs[i] = 0;
            //    int active = 0;

            //    for (int i = 0; i < numInputs; i++)
            //    {
            //        inputs[active] = 1f;
            //        foreach (Individual individual in pool.Individuals)
            //        {
            //            individual.fitness += Math.Abs((individual.FeedForward(inputs)[0] * 2 + 1) * numInputs - active);
            //        }
            //        inputs[active] = 0f;
            //        active = (active + 1) % numInputs;
            //    }

            //    pool.SortByFitness(true);

            //    Console.WriteLine("Generation: " + pool.Generation);
            //    Console.WriteLine("Best:  " + pool.Individuals.First().fitness);
            //    Console.WriteLine("Worst: " + pool.Individuals.Last().fitness);
            //    Console.WriteLine("Avg:   " + pool.Individuals.Average(i => i.fitness));

            //    Console.CursorLeft = 0;
            //    Console.CursorTop = 0;

            //Console.ReadKey(true);
            //}

            int numInputs = 100;
            float[] inputs = new float[numInputs];
            Network network = new Network(new int[] { numInputs, 10, 1 });
            int active = 0;
            for (int i = 0; i < numInputs; i++) inputs[i] = 0;

            int count = 0;
            float error = 0;
            int rounds = 0;

            while (true)
            {
                inputs[active] = 1;

                float expectedOutput = (float)active / numInputs / 2.0f + 0.25f;

                network.ApplyTrainingData(inputs, new float[] { expectedOutput }, 0.1f);

                float transformedResult = (network.FeedForward(inputs)[0] - 0.25f) * numInputs * 2.0f;
                error += Math.Abs(active - transformedResult);

                inputs[active] = 0;
                active = (active + 1) % numInputs;

                count = (count + 1) % 10000;
                if (count == 0)
                {
                    rounds++;
                    Console.WriteLine("Iteration #" + 10000 * rounds);
                    Console.WriteLine("\tError: " + error / 10000);
                    error = 0;
                }
            }
        }
    }
}
