using System;

namespace Maui.Dogfood.Feeder;

public class Workload
{
    public string GithubOrg { get; set; } = string.Empty;

    public string GithubRepo { get; set; } = string.Empty;

    public string GithubCommit { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ManifestPrefix { get; set; } = string.Empty;

    public string ShortHash => GithubCommit.Substring(0, 7);

    public string DownloadCache => Path.Combine(Env.CacheDirectory, ShortHash);
}

public class AndroidWorkload : Workload
{
    public AndroidWorkload()
    {
        GithubOrg = "xamarin";
        GithubRepo = "xamarin-android";
        Name = "android";
        ManifestPrefix = "Microsoft.NET.Sdk.Android";
    }
}

public class iOSWorkload : Workload
{
    public iOSWorkload()
    {
        GithubOrg = "xamarin";
        GithubRepo = "xamarin-macios";
        Name = "ios";
        ManifestPrefix = "Microsoft.NET.Sdk.iOS";
    }
}

public class MacCatalystWorkload : Workload
{
    public MacCatalystWorkload()
    {
        GithubOrg = "xamarin";
        GithubRepo = "xamarin-macios";
        Name = "maccatalyst";
        ManifestPrefix = "Microsoft.NET.Sdk.MacCatalyst";
    }
}

public class macOSWorkload : Workload
{
    public macOSWorkload()
    {
        GithubOrg = "xamarin";
        GithubRepo = "xamarin-macios";
        Name = "macos";
        ManifestPrefix = "Microsoft.NET.Sdk.macOS";
    }
}

public class MauiWorkload : Workload
{
    public MauiWorkload()
    {
        GithubOrg = "dotnet";
        GithubRepo = "maui";
        Name = "maui";
        ManifestPrefix = "Microsoft.NET.Sdk.Maui";
    }
}

public class tvOSWorkload : Workload
{
    public tvOSWorkload()
    {
        GithubOrg = "xamarin";
        GithubRepo = "xamarin-macios";
        Name = "tvos";
        ManifestPrefix = "Microsoft.NET.Sdk.tvOS";
    }
}

