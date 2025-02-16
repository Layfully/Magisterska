using GeneticSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace master_thesis;

public enum Hand { Left, Right }
public enum Finger { Pinky, Ring, Middle, Index }
public enum Row { Top, Middle, Bottom }

public class KeyboardFitness : IFitness
{
    private readonly List<string> datasetFiles;

    public KeyboardFitness(string datasetPath)
    {
        datasetFiles = DatasetLoader.CollectTextFiles(datasetPath);
        qwertyScores = CalculateLayoutScores(qwerty);
    }

    private string qwerty = "qazwsxedcrfvtgbyhnujmik,ol.p;/";
    private readonly (double fingerTravel, double fingerStrength, double handAlternation, double pressDirection) qwertyScores;

    public double Evaluate(IChromosome chromosome)
    {
        string layout = new(chromosome.GetGenes().Select(gene => Convert.ToChar(gene.Value)).ToArray());

        // Calculate scores for both layouts (current layout and QWERTY)
        var (totalFingerTravelDistanceScore, totalFingerStrengthScore, totalHandAlternation, totalPressDirectionScore) = CalculateLayoutScores(layout);
        var (totalFingerTravelDistanceScoreQwerty, totalFingerStrengthScoreQwerty, totalHandAlternationQwerty, totalPressDirectionScoreQwerty) = CalculateLayoutScores(qwerty);

        // Normalize current layout scores using QWERTY as a reference
        double normalizedFingerTravelDistanceScore = NormalizeScore(totalFingerTravelDistanceScore, totalFingerTravelDistanceScoreQwerty);
        double normalizedFingerStrengthScore = NormalizeScore(totalFingerStrengthScore, totalFingerStrengthScoreQwerty);
        double normalizedHandAlternationScore = NormalizeScore(totalHandAlternation, totalHandAlternationQwerty);
        double normalizedPressDirectionScore = NormalizeScore(totalPressDirectionScore, totalPressDirectionScoreQwerty);

        // Apply weights to the normalized scores
        const double fingerTravelWeight = 0.7;
        const double fingerStrengthWeight = 0.5;
        const double handAlternationWeight = 1.0;
        const double pressDirectionWeight = 0.6;

        double totalScore =
              normalizedFingerTravelDistanceScore * fingerTravelWeight
            + normalizedFingerStrengthScore * fingerStrengthWeight
            + normalizedHandAlternationScore * handAlternationWeight
            + normalizedPressDirectionScore * pressDirectionWeight;

        // Set the score on the chromosome
        ((KeyboardChromosome)chromosome).Score = -totalScore;

        return -totalScore;
    }

    private static double NormalizeScore(double currentScore, double qwertyScore)
    {
        return currentScore / qwertyScore;
    }

    private (double, double, double, double) CalculateLayoutScores(string layout)
    {
        double totalFingerTravelDistanceScore = 0, totalFingerStrengthScore = 0, totalHandAlternation = 0, totalPressDirectionScore = 0;
        object lockObj = new();

        Parallel.ForEach(datasetFiles, filePath =>
        {
            string text = File.ReadAllText(filePath);

            // Calculate scores for the current layout
            double fingerTravelDistanceScore = FitnessScoreCalculator.CalculateTravelDistance(text, layout);
            double fingerStrengthScore = FitnessScoreCalculator.CalculateFingerStrengthFactor();
            (double handAlternationScore, double pressDirectionScore) = FitnessScoreCalculator.CalculateHandMetrics(text, layout);

            lock (lockObj)
            {
                totalFingerStrengthScore += fingerStrengthScore;
                totalFingerTravelDistanceScore += fingerTravelDistanceScore;
                totalHandAlternation += handAlternationScore;
                totalPressDirectionScore += pressDirectionScore;
            }
        });

        return (totalFingerTravelDistanceScore, totalFingerStrengthScore, totalHandAlternation, totalPressDirectionScore);
    }
}

//public double Evaluate(IChromosome chromosome)
//{
//    string layout = new(chromosome.GetGenes().Select(gene => Convert.ToChar(gene.Value)).ToArray());
//    double totalFingerTravelDistanceScore = 0, totalFingerStrengthScore = 0, totalHandAlternation = 0, totalPressDirectionScore = 0;

//    object lockObj = new();

//    Parallel.ForEach(datasetFiles, filePath =>
//    {
//        string text = File.ReadAllText(filePath);
//        double fingerTravelDistanceScore = FitnessScoreCalculator.CalculateTravelDistance(text, layout);
//        double fingerStrengthScore = FitnessScoreCalculator.CalculateFingerStrengthFactor();
//        (double handAlternationScore, double pressDirectionScore) = FitnessScoreCalculator.CalculateHandMetrics(text, layout);

//        lock (lockObj)
//        {
//            totalFingerStrengthScore += fingerStrengthScore;
//            totalFingerTravelDistanceScore += fingerTravelDistanceScore;
//            totalHandAlternation += handAlternationScore;
//            totalPressDirectionScore += pressDirectionScore;
//        }
//    });

//    double fingerTravelScoreScaled = 1 / totalFingerTravelDistanceScore;
//    double fingerStrengthScoreScaled = 1 / totalFingerStrengthScore;
//    double handAlternationScoreScaled = 1 / totalHandAlternation;
//    double pressDirectionScoreScaled = 1 / totalPressDirectionScore;

//    double scaleSum = fingerTravelScoreScaled + fingerStrengthScoreScaled + handAlternationScoreScaled + pressDirectionScoreScaled;

//    fingerTravelScoreScaled /= scaleSum;
//    fingerStrengthScoreScaled /= scaleSum;
//    handAlternationScoreScaled /= scaleSum;
//    pressDirectionScoreScaled /= scaleSum;


//    double scaledFingerTravelDistanceScore = totalFingerTravelDistanceScore * fingerTravelScoreScaled;
//    double scaledFingerStrengthScore = totalFingerStrengthScore * fingerStrengthScoreScaled;
//    double scaledHandAlternationScore = totalHandAlternation * handAlternationScoreScaled;
//    double scaledPressDirectionScore = totalPressDirectionScore * pressDirectionScoreScaled;

//    double totalScore = scaledFingerTravelDistanceScore + scaledFingerStrengthScore + scaledHandAlternationScore + scaledPressDirectionScore;

//    ((KeyboardChromosome)chromosome).Score = -totalScore;

//    return -totalScore;
//}

