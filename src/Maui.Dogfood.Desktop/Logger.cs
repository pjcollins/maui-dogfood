using System;
using System.Text;

namespace Maui.Dogfood.Desktop;

public class Logger : TextWriter
{
    public static string LogFile { get; set; }
    static ReaderWriterLock locker = new ReaderWriterLock();

    public Logger()
    {
        Directory.CreateDirectory(Feeder.Env.LogDirectory);
        StartNewLogFile();
    }

    public override Encoding Encoding
    {
        get
        {
            return Encoding.UTF8;
        }
    }

    public override void WriteLine(string text)
    {
        try
        {
            locker.AcquireWriterLock(int.MaxValue);
            File.AppendAllLines(LogFile, new string[] { text });
        }
        catch
        {
        }
        finally
        {
            locker.ReleaseWriterLock();
        }
    }

    public static string ReadLogFile()
    {
        try
        {
            locker.AcquireWriterLock(int.MaxValue);
            return File.ReadAllText(LogFile);
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            locker.ReleaseWriterLock();
        }
    }

    public static void StartNewLogFile()
    {
        LogFile = Path.Combine(Feeder.Env.LogDirectory, $"install-{DateTime.Now.ToFileTime()}.txt");
    }
}

