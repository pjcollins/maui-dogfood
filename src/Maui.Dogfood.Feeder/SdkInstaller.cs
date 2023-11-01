using System;

namespace Maui.Dogfood.Feeder;

public class SdkInstaller
{
    public string SdkArchivePath { get; set; }

    public SdkInstaller (string sdkArchivePath)
    {
        SdkArchivePath = sdkArchivePath;
    }

    public bool Install ()
    {
        if (!File.Exists (SdkArchivePath))
            return false;

        var extractor = GetExtractor ();
        return extractor.Unzip (SdkArchivePath);
    }

    SdkExtractor GetExtractor ()
    {
        if (SdkArchivePath.EndsWith (".pkg", StringComparison.OrdinalIgnoreCase))
            return new PkgExtractor ();
        else if (SdkArchivePath.EndsWith (".msi", StringComparison.OrdinalIgnoreCase))
            return new MsiExtractor ();
        else if (SdkArchivePath.EndsWith (".zip", StringComparison.OrdinalIgnoreCase))
            return new ZipExtractor ();
        else
            throw new Exception ($"Unable to extract unknown SDK archive type: '{SdkArchivePath}'!");
    }

}

