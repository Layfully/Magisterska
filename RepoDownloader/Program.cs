internal class Program
{
    private static void Main()
    {
        try
        {
            string targetDirectory = "D:\\CSharpDataset";
            string cloneDirectory = "C:\\Clones";
            string jsonFilePath = "C:\\dataset\\repositories.json";
            string progressFilePath = "C:\\dataset\\progress.txt";

            int i = ReadProgress(progressFilePath);

            string[] allowedExtensions = { "cs", "xaml", "resx", "md", "ps1", "csx", "json", "xml", "yml", "aspx", "ascx", "master", "cshtml", "js", "ts", "web.config", "css", "bat", "psi", "razor", "sql" };

            // Read JSON file
            string[] lines = File.ReadAllLines(jsonFilePath);
            for (; i < lines.Length; i++)
            {
                string line = lines[i];
                RepositoryEntry? repositoryEntry = JsonSerializer.Deserialize<RepositoryEntry>(line);

                if (repositoryEntry is not null)
                {
                    DownloadFilesFromRepository(repositoryEntry.Name, repositoryEntry.Url, targetDirectory, cloneDirectory, allowedExtensions);

                    WriteProgress(progressFilePath, i + 1);
                }
                else
                {
                    Console.WriteLine("Error: RepositoryEntry is null");
                }
            }

            Console.WriteLine("Process completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void DownloadFilesFromRepository(string repoName, string repoUrl, string targetDirectory, string cloneDirectory, string[] allowedExtensions)
    {
        Console.WriteLine($"Downloading files with specified extensions from repository '{repoName}'...");

        string repositoryUrl = GetGitRepoUrlFromApiUrl(repoUrl);
        string repositoryDirectory = Path.Combine(cloneDirectory, repoName);
        string repositoryCommandDirectory = Path.Combine(cloneDirectory, repoName).ToCommandFormat();
        string repositoryCommandDirectoryParent = Path.Combine(cloneDirectory, repoName.Split('/')[0]);

        // Clone the repository
        string cloneCommand = $"git clone --filter=blob:none --no-checkout --depth 1 --sparse https://code@github.com/{repositoryUrl}.git {repositoryCommandDirectory}";

        Console.WriteLine(cloneCommand);

        var result = CommandExecutor.ExecuteCommand(cloneCommand);

        if (result.ExitCode == 128)
        {
            Console.WriteLine("Cloning repo failed. Skipping this one");
            DeleteDirectory(repositoryCommandDirectoryParent);
            return;
        }

        // Use Git status to get the list of files and filter by allowed extensions
        string statusCommand = $"git -C {repositoryDirectory.ToCommandFormat()} status --porcelain";
        Console.WriteLine(statusCommand);

        var statusResult = CommandExecutor.ExecuteCommand(statusCommand);

        // Extract file extensions from git status output
        var existingExtensions = statusResult.Output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                string extension = Path.GetExtension(line.Trim()[3..]);
                return !string.IsNullOrEmpty(extension) ? extension.TrimStart('.') : null;
            })
            .Where(ext => !string.IsNullOrEmpty(ext))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // Filter allowed extensions based on existing extensions in git status
        var filteredExtensions = allowedExtensions.Intersect(existingExtensions, StringComparer.Ordinal).ToArray();

        if (filteredExtensions.Length == 0)
        {
            Console.WriteLine("No files with specified extensions found in the repository. Skipping.");
            DeleteDirectory(repositoryCommandDirectoryParent);
            return;
        }

        // Use Git archive to download files directly
        string archivePath = Path.Combine(cloneDirectory, repoName + ".zip");
        ;
        string extractDirectory = Path.Combine(cloneDirectory, repoName);

        string archiveCommand = $"git -C {repositoryDirectory.ToCommandFormat()} archive --output={archivePath.ToCommandFormat()} HEAD --format=zip -- {string.Join(" ", filteredExtensions.Select(ext => $"*.{ext}"))}";

        Console.WriteLine(archiveCommand);

        CommandExecutor.ExecuteCommand(archiveCommand);

        // Create the extraction directory if it doesn't exist
        Directory.CreateDirectory(repositoryDirectory);

        ZipFile.ExtractToDirectory(archivePath, repositoryDirectory);

        File.Delete(archivePath);

        MoveFiles(allowedExtensions, repositoryDirectory, targetDirectory);

        Console.WriteLine($"Download from repository '{repoName}' completed.");
    }

    private static string GetGitRepoUrlFromApiUrl(string apiUrl)
    {
        // Extract the repository path from the GitHub API URL
        // Example input: "https://api.github.com/repos/shanselman/SmallestDotNet"
        // Example output: "shanselman/SmallestDotNet"
        var uri = new Uri(apiUrl);
        string[] segments = uri.Segments;
        return string.Join("/", segments.Select(s => s.Replace("/", "")).Skip(2)).TrimEnd('/');
    }

    private static void MoveFiles(string[] allowedExtensions, string sourceDirectory, string targetDirectory)
    {
        try
        {
            // Move files to the final destination with conflict resolution
            foreach (var allowedExtension in allowedExtensions)
            {
                string sourcePattern = Path.Combine(sourceDirectory, $"*.{allowedExtension}");
                string[] files = Directory.GetFiles(sourceDirectory, $"*.{allowedExtension}", SearchOption.AllDirectories);

                foreach (var sourceFilePath in files)
                {
                    string relativePath = Path.GetRelativePath(sourceDirectory, sourceFilePath);
                    string destinationFileName = Path.Combine(targetDirectory, relativePath);

                    // Handle file conflicts by appending a unique identifier
                    int counter = 1;
                    while (File.Exists(destinationFileName))
                    {
                        string newFileName = $"{Path.GetFileNameWithoutExtension(relativePath)}_{counter}{Path.GetExtension(relativePath)}";
                        destinationFileName = Path.Combine(targetDirectory, newFileName);
                        counter++;
                    }

                    // Move the file to the destination
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));
                    File.Move(sourceFilePath, destinationFileName);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        DeleteDirectory(sourceDirectory);
    }

    private static int ReadProgress(string progressFilePath)
    {
        if (File.Exists(progressFilePath))
        {
            string progressString = File.ReadAllText(progressFilePath);
            if (int.TryParse(progressString, out int progress))
            {
                return progress;
            }
        }
        return 0; // Default to starting from the beginning if progress file doesn't exist or is invalid
    }

    private static void WriteProgress(string progressFilePath, int progress)
    {
        File.WriteAllText(progressFilePath, progress.ToString());
    }

    public static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
    }
}
