using System.Text.Json.Serialization;

namespace RepoDownloader;

public class RepositoryEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
}