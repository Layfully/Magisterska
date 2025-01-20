using System;
using System.Collections.Generic;
using System.IO;

namespace master_thesis;

public class DatasetLoader
{
    public static List<string> CollectTextFiles(string directory)
    {
        List<string> filePaths = [];
        int maxFiles = 3000;
        double targetMdRatio = 0.075;
        double targetYmlRatio = 0.075;
        double targetJsonRatio = 0.075;

        int mdCount = 0;
        int ymlCount = 0;
        int jsonCount = 0;

        foreach (var dir in Directory.EnumerateDirectories(directory))
        {
            foreach (var filePath in Directory.EnumerateFiles(dir))
            {
                if (filePath.EndsWith(".md"))
                {
                    if ((double)mdCount / (filePaths.Count + 1) > targetMdRatio)
                    {
                        continue;
                    }
                    mdCount++;
                }
                else if (filePath.EndsWith(".yml"))
                {
                    if ((double)ymlCount / (filePaths.Count + 1) > targetYmlRatio)
                    {
                        continue;
                    }
                    ymlCount++;
                }
                else if (filePath.EndsWith(".json"))
                {
                    if ((double)jsonCount / (filePaths.Count + 1) > targetJsonRatio)
                    {
                        continue;
                    }
                    jsonCount++;
                }

                filePaths.Add(filePath);

                if (filePaths.Count >= maxFiles)
                {
                    Console.WriteLine($"Read {filePaths.Count} files");
                    return filePaths;
                }
            }
        }

        Console.WriteLine($"Read {filePaths.Count} files");
        return filePaths;
    }
}