using System;
using System.IO.Compression;
using System.Xml.Linq;

namespace Maui.Dogfood.Feeder;

public class WorkloadInstaller
{
    public IEnumerable<Workload> Workloads { get; set; }

    List<string> Sources = new List<string> {
        "https://api.nuget.org/v3/index.json",
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json",
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json",
    };

    public WorkloadInstaller(IEnumerable<Workload> workloads, IEnumerable<string> feeds)
    {
        Workloads = workloads;
        Sources.AddRange(feeds);
    }

    string WriteNuGetConfig()
    {
        var nugetConfigPath = Path.Combine(Env.TempDirectory, "nuget.config");

        if (File.Exists(nugetConfigPath))
            File.Delete(nugetConfigPath);

        Directory.CreateDirectory(Env.TempDirectory);

        // If using any internal sources, we will need to configure the nuget.config file with a packageSourceCredentials element
        string username = string.Empty;
        string password = string.Empty;
        bool hasInternalSources = Sources.Any(s => s.Contains("dnceng/internal", StringComparison.OrdinalIgnoreCase));
        if (hasInternalSources)
        {
            if (!File.Exists (Env.DncEngTokenFilePath))
                throw new FileNotFoundException($"Your configuration includes internal feeds, please provide a '{Env.DncEngTokenFilePath}' file.");

            var tokenFileLines = File.ReadAllLines(Env.DncEngTokenFilePath);
            username = tokenFileLines[0];
            password = tokenFileLines[1];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new Exception($"Could not determine username and token from the '{Env.DncEngTokenFilePath}' file.");
        }

        var pkgSourcesElement = new XElement("packageSources");
        var pkgSourceCredsElement = new XElement("packageSourceCredentials");
        int sourceIndex = 0;
        foreach (var source in Sources)
        {
            var sourceElement = new XElement("add");
            var sourceName = $"source{++sourceIndex}";
            sourceElement.SetAttributeValue("key", sourceName);
            sourceElement.SetAttributeValue("value", source);
            pkgSourcesElement.Add(sourceElement);

            if (source.Contains("dnceng/internal", StringComparison.OrdinalIgnoreCase))
            {
                var sourceCredsUserElement = new XElement("add");
                sourceCredsUserElement.SetAttributeValue("key", "Username");
                sourceCredsUserElement.SetAttributeValue("value", username);
                var sourceCredsPasswordElement = new XElement("add");
                sourceCredsPasswordElement.SetAttributeValue("key", "ClearTextPassword");
                sourceCredsPasswordElement.SetAttributeValue("value", password);

                var sourceCredsElement = new XElement(sourceName);
                sourceCredsElement.Add(sourceCredsUserElement, sourceCredsPasswordElement);
                pkgSourceCredsElement.Add(sourceCredsElement);
            }
        }

        var doc = new XDocument(new XElement("configuration"));
        doc.Root?.Add (pkgSourcesElement, pkgSourceCredsElement);
        doc.Save(nugetConfigPath);
        return nugetConfigPath;
    }

    // TODO: Support SxS manifest installs
    public async Task InstallAsync()
    {
        if (!Workloads.Any())
            return;

        // Download .nupkgs
        var workloadsToDownload = Workloads.DistinctBy(w => w.GithubCommit);
        await Parallel.ForEachAsync(workloadsToDownload, async (wl, token) =>
        {
            using var artifactClient = new ArtifactClient();
            await artifactClient.DownloadNuGetsAsync(wl.GithubOrg, wl.GithubRepo, wl.GithubCommit, wl.DownloadCache);
        });

        // Set up package sources
        Sources.AddRange(workloadsToDownload.Select(wl => wl.DownloadCache));
        var nugetConfigFile = WriteNuGetConfig();

        foreach (var wl in Workloads)
        {
            // Extract and copy manifest files to sdk-manifests folder
            var manifestPack = Directory.EnumerateFiles(wl.DownloadCache, $"{wl.ManifestPrefix}.Manifest*").FirstOrDefault();
            if (!File.Exists(manifestPack))
                throw new FileNotFoundException($"Could not find manifest pack '{wl.ManifestPrefix}' in '{wl.DownloadCache}'.");

            string tempDest = Path.Combine(Env.TempDirectory, $"{wl.Name}-{wl.ShortHash}");
            if (Directory.Exists(tempDest))
                Directory.Delete(tempDest, true);

            Directory.CreateDirectory(tempDest);
            Console.WriteLine($"Extracting {wl.Name} manifest to {tempDest}");
            ZipFile.ExtractToDirectory(manifestPack, tempDest);

            var manifestFiles = Directory.EnumerateFiles(tempDest, "WorkloadManifest*", SearchOption.AllDirectories);
            var manifestFileDest = Path.Combine(Env.DotnetPreviewManifestDirectory, wl.ManifestPrefix.ToLower());
            Directory.CreateDirectory(manifestFileDest);

            foreach (var file in manifestFiles)
                File.Copy(file, Path.Combine(Env.DotnetPreviewManifestDirectory, wl.ManifestPrefix.ToLower(), Path.GetFileName(file)), true);

            // Install workload
            var didInstall = DotnetPreview.Run("workload", string.Join(" ", new string[] {
                "install", wl.Name,
                "--skip-manifest-update",
                "--verbosity diag",
                $"--temp-dir \"{tempDest}\"",
                $"--configfile \"{nugetConfigFile}\"",
            }));

            if (!didInstall)
                throw new Exception($"Failed to install workload '{wl.Name}', check the output.");
        }

    }
}

