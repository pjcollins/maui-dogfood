using System;

namespace Maui.Dogfood.Feeder;

public class GitHubStatusArtifact
{
    public string url { get; set; } = string.Empty;
    public string build { get; set; } = string.Empty;
    public string branch { get; set; } = string.Empty;
    public int size { get; set; }
}

