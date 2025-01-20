using GeneticSharp;
using System.Collections.Generic;

namespace master_thesis;

internal class KeyboardChromosome : ChromosomeBase
{
    private static readonly List<char> chars = new("______________________________");

    public double Score { get; internal set; }

    public KeyboardChromosome()
        : base(30)
    {
        for (int i = 0; i < chars.Count; i++)
        {
            ReplaceGene(i, new Gene(chars[i]));
        }
    }

    public KeyboardChromosome(string layout) : base(30)
    {
        for (int i = 0; i < layout.Length; i++)
        {
            ReplaceGene(i, new Gene(layout[i]));
        }
    }

    public KeyboardChromosome(Gene[] genes) : base(30)
    {

        int[] letterIndexes = RandomizationProvider.Current.GetUniqueInts(30, 0, 30);

        for (int i = 0; i < letterIndexes.Length; i++)
        {
            ReplaceGene(i, genes[letterIndexes[i]]);
        }
    }

    public override Gene GenerateGene(int geneIndex)
    {
        char randomLetter = chars[RandomizationProvider.Current.GetInt(0, chars.Count)];

        return new Gene(randomLetter);
    }

    public override IChromosome CreateNew()
    {
        return new KeyboardChromosome(GetGenes());
    }

    /// <summary>
    /// Creates a clone.
    /// </summary>
    /// <returns>The chromosome clone.</returns>
    public override IChromosome Clone()
    {
        KeyboardChromosome clone = base.Clone() as KeyboardChromosome;
        clone.Score = Score;
        return clone;
    }
}
