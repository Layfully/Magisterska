using System;
using System.ComponentModel;

namespace GeneticSharp;

[DisplayName("Keyboard")]
public class KeyboardMutation : MutationBase
{
    protected override void PerformMutate(IChromosome chromosome, float probability)
    {
        ArgumentNullException.ThrowIfNull(chromosome);

        Gene[] genes = chromosome.GetGenes();

        if (RandomizationProvider.Current.GetDouble() <= probability)
        {
            int point1 = RandomizationProvider.Current.GetInt(0, genes.Length);
            int point2 = RandomizationProvider.Current.GetInt(0, genes.Length);

            Gene gene1 = genes[point1];
            Gene gene2 = genes[point2];

            chromosome.ReplaceGene(point1, gene2);
            chromosome.ReplaceGene(point2, gene1);
        }
    }
}