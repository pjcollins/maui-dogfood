using System;

namespace Maui.Dogfood.Feeder;

public class CommitStatusResult
{
    public string state { get; set; } = string.Empty;
    public List<CommitStatus> statuses { get; set; } = new List<CommitStatus>();
    public string sha { get; set; } = string.Empty;
    public int total_count { get; set; }
    public string commit_url { get; set; } = string.Empty;
    public string url { get; set; } = string.Empty;
}

public class CommitStatus
{
    public object? id { get; set; }
    public string state { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string target_url { get; set; } = string.Empty;
    public string context { get; set; } = string.Empty;
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}

