﻿using System;
using System.Diagnostics;

namespace Maui.Dogfood.Feeder;

public class DotnetPreview
{
    public static readonly string DotnetTool = Path.Combine(Env.DotnetPreviewDirectory, "dotnet");
    const int DEFAULT_TIMEOUT = 900;

    public static bool Build(string projectFile, string config, string target = "", string framework = "", IEnumerable<string>? properties = null, string binlogPath = "")
    {
        var binlogName = $"build-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
        var buildArgs = $"\"{projectFile}\" -c {config}";

        if (!string.IsNullOrEmpty(target))
        {
            binlogName = $"{target}-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
            buildArgs += $" -t:{target}";
        }

        if (!string.IsNullOrEmpty(framework))
            buildArgs += $" -f:{framework}";

        if (properties != null)
        {
            foreach (var p in properties)
            {
                buildArgs += $" -p:{p}";
            }
        }

        if (string.IsNullOrEmpty(binlogPath))
        {
            binlogPath = Path.Combine(Path.GetDirectoryName(projectFile) ?? "", binlogName);
        }

        return Run("build", $"{buildArgs} -bl:\"{binlogPath}\"");
    }

    public static bool New(string shortName, string outputDirectory, string framework = "")
    {
        var args = $"{shortName} -o \"{outputDirectory}\"";

        if (!string.IsNullOrEmpty(framework))
            args += $" -f {framework}";

        var output = RunForOutput("new", args, out int exitCode, timeoutInSeconds: 300);
        Console.WriteLine(output);
        return exitCode == 0;
    }

    public static bool Run(string command, string args, int timeoutInSeconds = DEFAULT_TIMEOUT)
    {
        var runOutput = RunForOutput(command, args, out int exitCode, timeoutInSeconds);
        if (exitCode != 0)
            Console.WriteLine(runOutput);

        return exitCode == 0;
    }

    public static string RunForOutput(string command, string args, out int exitCode, int timeoutInSeconds = DEFAULT_TIMEOUT)
    {
        var pinfo = new ProcessStartInfo(DotnetTool, $"{command} {args}");
        pinfo.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = "0";
        //Workaround: https://github.com/dotnet/linker/issues/3012
        pinfo.EnvironmentVariables["DOTNET_gcServer"] = "0";
        return ToolRunner.Run(pinfo, out exitCode, timeoutInSeconds: timeoutInSeconds);
    }
}

