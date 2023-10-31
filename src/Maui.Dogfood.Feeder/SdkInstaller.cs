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
        var extractor = GetExtractor ();
        return extractor.Unzip (SdkArchivePath);
    }

    SdkExtractor GetExtractor ()
    {
        if (SdkArchivePath.EndsWith (".pkg", StringComparison.OrdinalIgnoreCase))
            return new PkgExtractor ();
        else if (SdkArchivePath.EndsWith (".msi", StringComparison.OrdinalIgnoreCase))
            return new MsiExctractor ();
        else if (SdkArchivePath.EndsWith (".zip", StringComparison.OrdinalIgnoreCase))
            return new ZipExtractor ();
        else
            throw new Exception ($"Unknown SDK archive type: {SdkArchivePath}");
    }

}

