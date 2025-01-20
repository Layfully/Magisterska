using GeneticSharp;
using System;

namespace master_thesis;

internal class Program
{
    private static void Main(string[] args)
    {
        string qwertyLayout = "uakft,gsxpnw/dq.iymljye;crzhob";

        EliteSelection selection = new();

        KeyboardCrossover crossover = new();
        KeyboardMutation mutation = new();
        KeyboardFitness fitness = new("E:\\CSharpDataset");
        KeyboardChromosome chromosome = new(qwertyLayout);

        Population population = new(10, 10, chromosome);

        GeneticAlgorithm ga = new(population, fitness, selection, crossover, mutation)
        {
            Termination = new FitnessStagnationTermination(100),
            MutationProbability = 0.1f
        };

        ga.GenerationRan += (s, e) =>
        {
            Console.WriteLine($"Generation {ga.GenerationsNumber}. Best fitness: {ga.BestChromosome.Fitness.Value}");

            var genes = ((KeyboardChromosome)ga.BestChromosome).GetGenes();

            for (int i = 0; i < 3; i++)
            {
                var row = "";
                for (int j = i; j < genes.Length; j += 3)
                {
                    if (j < genes.Length)
                    {
                        row += genes[j].Value;
                    }
                }

                Console.WriteLine(row);
            }
        };

        Console.WriteLine("GA running...");
        ga.Start();

        Console.WriteLine();
        Console.WriteLine($"Best solution found has fitness: {ga.BestChromosome.Fitness}");
        Console.WriteLine($"Best solution:");

        var genes = ((KeyboardChromosome)ga.BestChromosome).GetGenes();

        for (int i = 0; i < 3; i++)
        {
            var row = "";
            for (int j = i; j < genes.Length; j += 3)
            {
                if (j < genes.Length)
                {
                    row += genes[j].Value;
                }
            }

            Console.WriteLine(row);
        }

        Console.WriteLine($"Elapsed time: {ga.TimeEvolving}");
        Console.ReadKey();
    }
}
