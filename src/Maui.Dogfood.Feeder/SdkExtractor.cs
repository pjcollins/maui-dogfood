using System;

namespace Maui.Dogfood.Feeder;

public abstract class SdkExtractor
{
    public abstract bool Unzip (string file);
}

public class PkgExtractor : SdkExtractor
{
    public override bool Unzip (string file)
    {
        var xarOut = Path.Combine (Env.TempDirectory, "tempsdk");
        if (Directory.Exists(xarOut)) {
            Directory.Delete(xarOut, true);
        }
        if (Directory.Exists(Env.DotnetPreviewDirectory)) {
            Directory.Delete(Env.DotnetPreviewDirectory, true);
        }
        Directory.CreateDirectory(xarOut);
        Directory.CreateDirectory(Env.DotnetPreviewDirectory);

        if (!ToolRunner.Run ("xar", $"-xf \"{file}\" -C \"{xarOut}\"")) {
             Console.WriteLine ($"Failed to 'xar' extract '{file}'!");
             return false;
        }

        var sdkComponents = Directory.GetDirectories(xarOut, "com.microsoft.dotnet.*.pkg");
        if (!sdkComponents.Any()) {
            Console.WriteLine ($"Failed to find sdk component directory in '{xarOut}'!");
            return false;
        }
        foreach (var sdkComponentDir in sdkComponents) {
            var gunzipPayload = Path.Combine (xarOut, sdkComponentDir, "Payload");
            if (!ToolRunner.Run ("gunzip", $"-fS \"\" \"{gunzipPayload}\"")) {
                Console.WriteLine ($"Failed to 'gunzip' '{gunzipPayload}'!");
                return false;
            }

            if (!ToolRunner.Run ("cpio", $"-iF \"{gunzipPayload}\"", workingDirectory: Env.DotnetPreviewDirectory)) {
                Console.WriteLine ($"Failed to copy '{gunzipPayload}' to '{Env.DotnetPreviewDirectory}'!");
                return false;
            }
        }

        if (Directory.Exists(xarOut)) {
            Directory.Delete(xarOut, true);
        }

       return File.Exists(DotnetPreview.DotnetTool);
    }
}

public class MsiExtractor : SdkExtractor
{
    public override bool Unzip (string file)
    {
        throw new NotImplementedException ();
    }
}

public class ZipExtractor : SdkExtractor
{
    public override bool Unzip (string file)
    {
        throw new NotImplementedException ();
    }
}

