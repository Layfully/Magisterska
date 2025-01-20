using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace master_thesis;

public class FitnessScoreCalculator
{
    private static readonly int[] top_indices = [0, 3, 6, 9, 12, 15, 18, 21, 24, 27];
    private static readonly int[] middle_indices = [1, 4, 7, 10, 13, 16, 19, 22, 25, 28];
    private static readonly int[] bottom_indices = [2, 5, 8, 11, 14, 17, 20, 23, 26, 29];

    private static ConcurrentDictionary<string, ConcurrentDictionary<string, double>> distanceCache = new();
    private static ConcurrentDictionary<char, int> keyFrequencies = new();
    private static Dictionary<Hand, Dictionary<Finger, double>> fingerStrengths = new()
    {
        { Hand.Left, new Dictionary<Finger, double>()
             {
                 { Finger.Pinky, 234.5 },
                 { Finger.Ring, 230.1 },
                 { Finger.Middle, 247.1 },
                 { Finger.Index, 256.6 }
             }
        },
        { Hand.Right, new Dictionary<Finger, double>()
             {
                 { Finger.IndexPinky, 310.6 },
                 { Finger.Middle, 293 },
                 { Finger.Ring, 263 },
                 { Finger.Pinky, 231 },
             }
        }
    };
    private static Dictionary<Hand, Dictionary<Finger, string>> handLetterAssignements;

    public static double CalculateTravelDistance(ReadOnlySpan<char> text, string layout)
    {
        double totalDistance = 0;

        List<char> topRow = top_indices.Select(i => layout[i]).ToList();
        List<char> middleRow = middle_indices.Select(i => layout[i]).ToList();
        List<char> bottomRow = bottom_indices.Select(i => layout[i]).ToList();

        Dictionary<string, double> costs = new()
        {
            { "top_row", 1.032 },
            { "bottom_row", 1.118 },
            { "vertical", 2.138 },
            { $"{layout[10]}{layout[12]}", 1.247 }, { $"{layout[12]}{layout[10]}", 1.247 },
            { $"{layout[16]}{layout[18]}", 1.247 }, { $"{layout[18]}{layout[16]}", 1.247 },
            { $"{layout[9]}{layout[13]}", 1.605 }, { $"{layout[13]}{layout[9]}", 1.605 },
            { $"{layout[15]}{layout[19]}", 1.605 }, { $"{layout[19]}{layout[15]}", 1.605 },
            { $"{layout[10]}{layout[14]}", 1.803 }, { $"{layout[14]}{layout[10]}", 1.803 },
            { $"{layout[16]}{layout[20]}", 1.803 }, { $"{layout[20]}{layout[16]}", 1.803 },
            { $"{layout[9]}{layout[14]}", 2.661 }, { $"{layout[14]}{layout[9]}", 2.661 },
            { $"{layout[15]}{layout[20]}", 2.661 }, { $"{layout[20]}{layout[15]}", 2.661 },
            { $"{layout[12]}{layout[11]}", 2.015 }, { $"{layout[11]}{layout[12]}", 2.015 },
            { $"{layout[18]}{layout[17]}", 2.015 }, { $"{layout[17]}{layout[18]}", 2.015 },
            { $"{layout[9]}{layout[12]}", 1 }, { $"{layout[12]}{layout[9]}", 1 },
            { $"{layout[10]}{layout[13]}", 1 }, { $"{layout[13]}{layout[10]}", 1 },
            { $"{layout[11]}{layout[14]}", 1 }, { $"{layout[14]}{layout[11]}", 1 },
            { $"{layout[18]}{layout[15]}", 1 }, { $"{layout[15]}{layout[18]}", 1 },
            { $"{layout[19]}{layout[16]}", 1 }, { $"{layout[16]}{layout[19]}", 1 },
            { $"{layout[20]}{layout[17]}", 1 }, { $"{layout[17]}{layout[20]}", 1 }
        };

        var startingHands = CreateStartingHands(layout);
        var currentHands = DeepCopy(startingHands);

        handLetterAssignements = new()
        {
            { Hand.Left, new Dictionary<Finger, string> {
                    { Finger.Pinky, layout[0..3] },
                    { Finger.Ring, layout[3..6] },
                    { Finger.Middle, layout[6..9] },
                    { Finger.Index, layout[9..15] }
                }
            },
            { Hand.Right, new Dictionary<Finger, string> {
                    { Finger.Index, layout[15..21] },
                    { Finger.Middle, layout[21..24] },
                    { Finger.Ring, layout[24..27] },
                    { Finger.Pinky, layout[27..30] }
                }
            }
        };

        foreach (var currChar in text)
        {
            if (currChar == ' ')
            {
                currentHands = DeepCopy(startingHands);
                continue;
            }

            char charToLower = char.ToLower(currChar);

            Hand currentHandName = Hand.Left;
            Finger currentFinger = Finger.Pinky;

            foreach (var (hand, fingers) in handLetterAssignements)
            {
                foreach (var (finger, column) in fingers)
                {
                    if (column.IndexOf(charToLower) != -1)
                    {
                        currentFinger = finger;
                        currentHandName = hand;
                        break;
                    }
                }
            }

            char prevHeldChar = currentHands[currentHandName][currentFinger];
            string keyPair = prevHeldChar.ToString() + charToLower;

            double distance = 0.0;

            // Check the cache first
            if (distanceCache.ContainsKey(layout) && distanceCache[layout].TryGetValue(keyPair, out double value))
            {
                distance = value;
            }

            if (costs.ContainsKey(keyPair))
            {
                distance = costs[keyPair];
            }
            else if (prevHeldChar == charToLower)
            {
                distance = 0; // No movement
            }
            else if (middleRow.Contains(prevHeldChar))
            {
                if (bottomRow.Contains(charToLower))
                {
                    distance = costs["bottom_row"];
                }
                else if (topRow.Contains(charToLower))
                {
                    distance = costs["top_row"];
                }
                else
                {
                    continue;
                }
            }
            else if (topRow.Contains(prevHeldChar))
            {
                if (middleRow.Contains(charToLower))
                {
                    distance = costs["top_row"];
                }
                else if (bottomRow.Contains(charToLower))
                {
                    distance = costs["vertical"];
                }
                else
                {
                    continue;
                }
            }
            else if (bottomRow.Contains(prevHeldChar))
            {
                if (middleRow.Contains(charToLower))
                {
                    distance = costs["bottom_row"];
                }
                else if (topRow.Contains(charToLower))
                {
                    distance = costs["vertical"];
                }
                else
                {
                    continue;
                }
            }
            else
            {
                Console.WriteLine($"Unexpected character {currChar}");
                continue;
            }

            if (!distanceCache.ContainsKey(layout))
            {
                distanceCache[layout] = [];
            }

            keyFrequencies.TryGetValue(charToLower, out var currentCount);
            keyFrequencies[charToLower] = currentCount + 1;
            distanceCache[layout][keyPair] = distance;

            totalDistance += distance;

            currentHands[currentHandName][currentFinger] = charToLower;
        }

        return totalDistance;
    }

    public static double CalculateFingerStrengthFactor()
    {
        double effortScore = 0;
        foreach (var keyFrequency in keyFrequencies)
        {
            char keyChar = keyFrequency.Key;

            (Hand hand, Finger finger) = handLetterAssignements
                .SelectMany(hand => hand.Value
                    .Where(kvp => kvp.Value.IndexOf(keyChar) != -1)
                    .Select(kvp => (Hand: hand.Key, Finger: kvp.Key)))
                .FirstOrDefault();

            effortScore += keyFrequency.Value / fingerStrengths[hand][finger];
        }

        return effortScore;
    }

    public static double CalculateHandAlternation(ReadOnlySpan<char> text, string layout)
    {
        double alternationScore = 0;

        char? previousChar = null;

        foreach (char currentChar in text)
        {
            if (currentChar == ' ')
            {
                previousChar = null;
                continue;
            }

            char charToLower = char.ToLower(currentChar);

            if (previousChar.HasValue)
            {
                (Hand prevHand, _) = GetHandAndFingerForChar(previousChar.Value);
                (Hand currentHand, _) = GetHandAndFingerForChar(charToLower);

                if (prevHand == currentHand)
                {
                    alternationScore++;
                }
            }

            previousChar = charToLower;
        }

        return alternationScore;
    }

    public static double CalculateHitDirection(ReadOnlySpan<char> text, string layout)
    {
        double wrongDirectionScore = 0;
        char? previousChar = null;

        foreach (char currentChar in text)
        {
            if (currentChar == ' ')
            {
                previousChar = null;
                continue;
            }

            char charToLower = char.ToLower(currentChar);

            if (previousChar.HasValue)
            {
                (Hand previousHand, Finger previousFinger) = GetHandAndFingerForChar(previousChar.Value);
                (Hand currentHand, Finger currentFinger) = GetHandAndFingerForChar(charToLower);

                if (previousHand == currentHand && previousFinger != currentFinger)
                {
                    int prevPosition = GetFingerPosition(layout, previousChar.Value);
                    int currentPosition = GetFingerPosition(layout, charToLower);

                    if (IsWrongDirection(currentHand, prevPosition, currentPosition))
                    {
                        wrongDirectionScore++;
                    }
                }
            }

            previousChar = charToLower;
        }

        return wrongDirectionScore;
    }

    public static (double alternationScore, double hitDirectionScore) CalculateHandMetrics(ReadOnlySpan<char> text, string layout)
    {
        double alternationScore = 0;
        double wrongDirectionScore = 0;
        char? previousChar = null;

        foreach (char currentChar in text)
        {
            if (currentChar == ' ')
            {
                previousChar = null;
                continue;
            }

            char charToLower = char.ToLower(currentChar);

            if (previousChar.HasValue)
            {
                (Hand previousHand, Finger previousFinger) = GetHandAndFingerForChar(previousChar.Value);
                (Hand currentHand, Finger currentFinger) = GetHandAndFingerForChar(charToLower);

                // Check hand alternation
                if (previousHand == currentHand)
                {
                    alternationScore++;

                    // Only check hit direction if using the same hand and different fingers
                    if (previousFinger != currentFinger)
                    {
                        int prevPosition = GetFingerPosition(layout, previousChar.Value);
                        int currentPosition = GetFingerPosition(layout, charToLower);

                        if (IsWrongDirection(currentHand, prevPosition, currentPosition))
                        {
                            wrongDirectionScore++;
                        }
                    }
                }
            }

            previousChar = charToLower;
        }

        return (alternationScore, wrongDirectionScore);
    }

    private static int GetFingerPosition(string layout, char c)
    {
        return layout.IndexOf(c);
    }

    private static bool IsWrongDirection(Hand hand, int prevPos, int currentPos)
    {
        if (hand == Hand.Left)
        {
            return prevPos > currentPos;
        }
        else
        {
            return prevPos < currentPos;
        }
    }

    private static (Hand, Finger) GetHandAndFingerForChar(char c)
    {
        foreach (var (hand, fingers) in handLetterAssignements)
        {
            foreach (var (finger, column) in fingers)
            {
                if (column.IndexOf(c) != -1)
                {
                    return (hand, finger);
                }
            }
        }

        throw new ArgumentException($"Character {c} not found in layout");
    }

    private static Dictionary<Hand, Dictionary<Finger, char>> DeepCopy(Dictionary<Hand, Dictionary<Finger, char>> dict)
    {
        Dictionary<Hand, Dictionary<Finger, char>> copy = [];
        foreach (var kvp in dict)
        {
            copy[kvp.Key] = new(kvp.Value);
        }
        return copy;
    }

    private static Dictionary<Hand, Dictionary<Finger, char>> CreateStartingHands(string layout)
    {
        Dictionary<Hand, Dictionary<Finger, char>> startingHands = new()
        {
            { Hand.Left, new Dictionary<Finger, char>() },
            { Hand.Right, new Dictionary<Finger, char>() }
        };

        startingHands[Hand.Left][Finger.Pinky] = layout[1];
        startingHands[Hand.Left][Finger.Ring] = layout[4];
        startingHands[Hand.Left][Finger.Middle] = layout[7];
        startingHands[Hand.Left][Finger.Index] = layout[10];

        startingHands[Hand.Right][Finger.Index] = layout[19];
        startingHands[Hand.Right][Finger.Middle] = layout[22];
        startingHands[Hand.Right][Finger.Ring] = layout[25];
        startingHands[Hand.Right][Finger.Pinky] = layout[28];

        return startingHands;
    }
}

