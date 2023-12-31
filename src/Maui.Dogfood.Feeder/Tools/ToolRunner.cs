﻿using System;
using System.Diagnostics;
using System.Text;

namespace Maui.Dogfood.Feeder;

public static class ToolRunner
{
    public static bool Run(string tool, string args,
        string workingDirectory = "",
        int timeoutInSeconds = 600)
    {
        var info = new ProcessStartInfo(tool, args);

        if (Directory.Exists(workingDirectory))
            info.WorkingDirectory = workingDirectory;

        var runOutput = Run(info, out int exitCode, timeoutInSeconds);
        if (exitCode != 0)
            Console.WriteLine(runOutput);

        return exitCode == 0;
    }

    public static string Run(string tool, string args, out int exitCode,
        string workingDirectory = "",
        int timeoutInSeconds = 600)
    {
        var info = new ProcessStartInfo(tool, args);

        if (Directory.Exists(workingDirectory))
            info.WorkingDirectory = workingDirectory;

        return Run(info, out exitCode, timeoutInSeconds);
    }

    public static string Run(ProcessStartInfo info, out int exitCode,
        int timeoutInSeconds = 600)
    {
        var procOutput = new StringBuilder();
        using (var p = new Process())
        {
            p.StartInfo = info;
            Console.WriteLine($"[ToolRunner] Running: {p.StartInfo.FileName} {p.StartInfo.Arguments}");
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += (sender, o) =>
            {
                if (!string.IsNullOrEmpty(o?.Data))
                {
                    lock (procOutput)
                        procOutput.AppendLine(o.Data);
                }
            };
            p.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e?.Data))
                {
                    lock (procOutput)
                        procOutput.AppendLine(e.Data);
                }
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            if (p.WaitForExit(timeoutInSeconds * 1000))
            {
                exitCode = p.ExitCode;
                Console.WriteLine($"[ToolRunner] Process '{Path.GetFileName(p.StartInfo.FileName)}' exited with code: {exitCode}");
            }
            else
            {
                exitCode = -1;
            }
        }

        return procOutput.ToString();
    }
}

