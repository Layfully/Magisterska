using master_thesis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GeneticSharp;

[DisplayName("Keyboard")]
public class KeyboardCrossover : CrossoverBase
{
    public KeyboardCrossover() : base(2, 2)
    {
    }

    protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
    {
        IChromosome parent1 = parents[0];
        IChromosome parent2 = parents[1];

        IChromosome child1 = Crossover(parent1, parent2);
        IChromosome child2 = Crossover(parent2, parent1);

        return [child1, child2];
    }

    private IChromosome Crossover(IChromosome parent1, IChromosome parent2)
    {
        IChromosome child = new KeyboardChromosome();
        Random random = new();

        int startIndex = random.Next(0, 30);
        int length = random.Next(0, 30);

        HashSet<char> usedGenes = new(child.GetGenes().Select(gene => Convert.ToChar(gene.Value)));

        // Copy a slice from parent1 to the child
        for (int i = 0; i < length; i++)
        {
            int index = (startIndex + i) % 30;
            child.ReplaceGene(index, parent1.GetGene(index));
            usedGenes.Add(Convert.ToChar(parent1.GetGene(index).Value));
        }

        // Fill in the remaining genes from parent2, skipping any that are already used
        int currentIndex = (startIndex + length) % 30;
        for (int i = 0; i < 30; i++)
        {
            int index = (startIndex + i) % 30;
            char geneChar = Convert.ToChar(parent2.GetGene(index).Value);

            if (!usedGenes.Contains(geneChar))
            {
                child.ReplaceGene(currentIndex, parent2.GetGene(index));
                usedGenes.Add(geneChar);
                currentIndex = (currentIndex + 1) % 30;
            }
        }

        return child;
    }
}