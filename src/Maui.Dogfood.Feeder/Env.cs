using System;

namespace Maui.Dogfood.Feeder;

public static class Env
{
    public static string RootDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "maui-previews");

    public static readonly string CacheDirectory = Path.Combine(RootDirectory, "_cache");

    public static readonly string TempDirectory = Path.Combine(RootDirectory, "_temp");

    public static readonly string LogDirectory = Path.Combine(RootDirectory, "_logs");

    public static readonly string DotnetPreviewDirectory = Path.Combine(RootDirectory, "dotnet");

    public static string DotnetPreviewManifestDirectory => Path.Combine(DotnetPreviewDirectory, "sdk-manifests", GetSdkBand ());

    public const string GitHubTokenEnvVarName = "MAUI_DOGFOOD_GH_TOKEN";

    public static readonly string GitHubTokenFilePath = Path.Combine(RootDirectory, "tokens", "github.token");

    public static readonly string DncEngTokenFilePath = Path.Combine(RootDirectory, "tokens", "dnceng.token");


    static string _sdkBand = "";
    public static string GetSdkBand ()
    {
        if (Directory.Exists(_sdkBand))
            return _sdkBand;

        string sdkManifestDir = Path.Combine(DotnetPreviewDirectory, "sdk-manifests");
        if (!Directory.Exists(sdkManifestDir))
            throw new DirectoryNotFoundException($"Unable to locate sdk-manifests folder in path '{DotnetPreviewDirectory}'");

        var latestSdkManifestDir = Directory.EnumerateDirectories(sdkManifestDir)?.Last();

        if (Directory.Exists(latestSdkManifestDir))
            return _sdkBand = latestSdkManifestDir;
        else
            throw new DirectoryNotFoundException($"Unable to locate sdk band folder in path '{sdkManifestDir}'");
    }

}

