using System;
using Maui.Dogfood.Feeder;

namespace Maui.Dogfood.Desktop;

public partial class MainPage : ContentPage
{
    public string SdkArchivePath { get; set; } = string.Empty;

    public string AndroidCommit { get; set; } = string.Empty;

    public string MaciOSCommit { get; set; } = string.Empty;

    public string MauiCommit { get; set; } = string.Empty;

    public static List<string> Feeds { get; set; } = new List<string>();


    public MainPage()
    {
        InitializeComponent();
        Console.SetOut(new Logger());
    }

    public void OnSdkArchiveEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        SdkArchivePath = SdkArchiveEntry.Text;
    }

    public void OnAndroidEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        AndroidCommit = AndroidEntry.Text;
    }

    public void OnMaciOSEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        MaciOSCommit = MaciosEntry.Text;
    }

    public void OnMauiEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        MauiCommit = MauiEntry.Text;
    }

    public void OnFeedsEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        Feeds.Clear();
        using var sr = new StringReader(FeedsEditor.Text);
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
                Feeds.Add(line.Trim());
        }
    }

    async void InstallButton_Clicked(object sender, EventArgs e)
    {
        Logger.StartNewLogFile();
        UpdateInstallIndicator(true, "Installing...");

        if (!string.IsNullOrWhiteSpace(SdkArchivePath)) {
            try {
                var sdkInstaller = new SdkInstaller(SdkArchivePath);
                if (!sdkInstaller.Install()) {
                    UpdateInstallIndicator(false, $"Failed to extract SDK archive: '{SdkArchivePath}'!");
                    return;
                }
            } catch (Exception ex) {
                UpdateInstallIndicator(false, $"Failed to extract SDK archive: '{SdkArchivePath}'!\n{ex}");
                return;
            }
        }

        var workloadsToInstall = new List<Workload>();
        if (!string.IsNullOrWhiteSpace(AndroidCommit))
            workloadsToInstall.Add(new AndroidWorkload { GithubCommit = AndroidCommit });

        if (!string.IsNullOrWhiteSpace(MaciOSCommit))
        {
            workloadsToInstall.Add(new iOSWorkload { GithubCommit = MaciOSCommit });
            workloadsToInstall.Add(new MacCatalystWorkload { GithubCommit = MaciOSCommit });
            workloadsToInstall.Add(new macOSWorkload { GithubCommit = MaciOSCommit });
            workloadsToInstall.Add(new tvOSWorkload { GithubCommit = MaciOSCommit });
        }

        if (!string.IsNullOrWhiteSpace(MauiCommit))
            workloadsToInstall.Add(new MauiWorkload { GithubCommit = MauiCommit });

        var installer = new WorkloadInstaller(workloadsToInstall, Feeds);

        try
        {
            await installer.InstallAsync();
            UpdateInstallIndicator(false, "Install complete.");
        }
        catch (Exception ex)
        {
            UpdateInstallIndicator(false, $"Install failed with:\n{ex}");
        }
    }

    void UpdateInstallIndicator(bool isInstalling, string text)
    {
        InstallButton.IsEnabled = !isInstalling;
        Console.WriteLine(text);
        InstallLabel.Text = text;
        InstallIndicator.IsRunning = isInstalling;
    }
}

