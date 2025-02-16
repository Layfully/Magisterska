using BenchmarkDotNet.Attributes;
using GeneticSharp;
using System;

namespace master_thesis;

public class Benchmark
{
    private GeneticAlgorithm ga;
    private string qwertyLayout = "uakft,gsxpnw/dq.iymljye;crzhob";
    private KeyboardFitness fitness;
    private KeyboardChromosome chromosome;

    [GlobalSetup]
    public void Setup()
    {
        fitness = new KeyboardFitness("E:\\CSharpDataset");
        chromosome = new KeyboardChromosome(qwertyLayout);
    }

    [Benchmark]
    public void RunGeneticAlgorithm_Population10_Mutation0_1()
    {
        SetupGeneticAlgorithm(10, 0.1f);
        ga.Start();
    }

    [Benchmark]
    public void RunGeneticAlgorithm_Population20_Mutation0_2()
    {
        SetupGeneticAlgorithm(20, 0.2f);
        ga.Start();
    }

    [Benchmark]
    public void RunGeneticAlgorithm_Population50_Mutation0_3()
    {
        SetupGeneticAlgorithm(50, 0.3f);
        ga.Start();
    }

    [Benchmark]
    public void RunGeneticAlgorithm_Population100_Mutation0_4()
    {
        SetupGeneticAlgorithm(100, 0.4f);
        ga.Start();
    }

    [Benchmark]
    public void RunGeneticAlgorithm_Population200_Mutation0_5()
    {
        SetupGeneticAlgorithm(200, 0.5f);
        ga.Start();
    }

    private void SetupGeneticAlgorithm(int populationSize, float mutationProbability)
    {
        EliteSelection selection = new();
        KeyboardCrossover crossover = new();
        KeyboardMutation mutation = new();

        Population population = new(populationSize, populationSize, chromosome);

        ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new FitnessStagnationTermination(100),
            MutationProbability = mutationProbability
        };
    }
}
