namespace RepoDownloader;

internal static class Extensions
{
    public static string ToCommandFormat(this string path)
    {
        // Normalize path separators to forward slashes
        return path.Replace("\\", "/");
    }
}
