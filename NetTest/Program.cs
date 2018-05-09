using System;
using System.Linq;
using NeuralNetwork;

namespace NetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int numInputs = 10;
            float[] inputs = new float[numInputs];
            GenePool pool = new GenePool();

            pool.Initialize(new int[] { numInputs, 1 });

            while (true)
            {
                pool.NextGeneration();

                for (int i = 0; i < numInputs; i++) inputs[i] = 0;
                int active = 0;

                for (int i = 0; i < numInputs; i++)
                {
                    inputs[active] = 1f;
                    foreach (Individual individual in pool.Individuals)
                    {
                        individual.fitness += Math.Abs((individual.FeedForward(inputs)[0] * 2 + 1) * numInputs - active);
                    }
                    inputs[active] = 0f;
                    active = (active + 1) % numInputs;
                }

                pool.SortByFitness(false);

                Console.WriteLine("Generation: " + pool.Generation);
                Console.WriteLine("Best:  " + pool.Individuals.First().fitness);
                Console.WriteLine("Worst: " + pool.Individuals.Last().fitness);
                Console.WriteLine("Avg:   " + pool.Individuals.Average(i => i.fitness));

                Console.CursorLeft = 0;
                Console.CursorTop = 0;

                //Console.ReadKey(true);
            }
        }
    }
}
