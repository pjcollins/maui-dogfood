using System;
using System.Diagnostics;
using CommandLine;
using Maui.Dogfood.Feeder;

namespace Maui.Dogfood.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, Dogfooder!");
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(RunWithOptions);

        stopWatch.Stop();
        Console.WriteLine($"Time elapsed: {stopWatch.Elapsed.TotalSeconds} (s)");
        Console.WriteLine("Enjoy your Dogfood!");
    }

    static async Task RunWithOptions(Options opts)
    {
        if (!string.IsNullOrWhiteSpace(opts.SdkArchivePath)) {
            var sdkInstaller = new SdkInstaller(opts.SdkArchivePath);
            if (!sdkInstaller.Install()) {
                throw new Exception($"Failed to extract SDK archive: '{opts.SdkArchivePath}'!");
            }
        }

        var workloadsToInstall = new List<Workload>();
        if (!string.IsNullOrWhiteSpace(opts.AndroidCommit))
            workloadsToInstall.Add(new AndroidWorkload { GithubCommit = opts.AndroidCommit });

        if (!string.IsNullOrWhiteSpace(opts.MaciOSCommit))
        {
            workloadsToInstall.Add(new iOSWorkload { GithubCommit = opts.MaciOSCommit });
            workloadsToInstall.Add(new MacCatalystWorkload { GithubCommit = opts.MaciOSCommit });
            workloadsToInstall.Add(new macOSWorkload { GithubCommit = opts.MaciOSCommit });
            workloadsToInstall.Add(new tvOSWorkload { GithubCommit = opts.MaciOSCommit });
        }

        if (!string.IsNullOrWhiteSpace(opts.MauiCommit))
            workloadsToInstall.Add(new MauiWorkload { GithubCommit = opts.MauiCommit });

        Console.WriteLine($"Attempting to install {string.Join(", ", workloadsToInstall.Select(w => w.Name))} workloads with {string.Join(", ", opts.Feeds)} feeds.");
        var installer = new WorkloadInstaller(workloadsToInstall, opts.Feeds);
        await installer.InstallAsync();
    }

    static void HandleParseError(IEnumerable<Error> errs)
    {
    }
}

class Options
{
    [Option("android", HelpText = "Android Commit hash to install.")]
    public string AndroidCommit { get; set; } = string.Empty;

    [Option("macios", HelpText = "MaciOS Commit hash to install.")]
    public string MaciOSCommit { get; set; } = string.Empty;

    [Option("maui", HelpText = "Maui Commit hash to install.")]
    public string MauiCommit { get; set; } = string.Empty;

    [Option("sdk-archive", HelpText = ".NET SDK archive to extract and install workloads in.")]
    public string SdkArchivePath { get; set; } = string.Empty;

    [Option('f', "feed", HelpText = "Feeds to use for installation.")]
    public IEnumerable<string> Feeds { get; set; } = new List<string>();
}

