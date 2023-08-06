using System;
using Maui.Dogfood.Feeder;

namespace Maui.Dogfood.Desktop;

public partial class MainPage : ContentPage
{
    public string AndroidCommit { get; set; }

    public string MaciOSCommit { get; set; }

    public string MauiCommit { get; set; }

    public static List<string> Feeds { get; set; } = new List<string>();


    public MainPage()
    {
        InitializeComponent();
        Console.SetOut(new Logger());
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
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
                Feeds.Add(line.Trim());
        }
    }

    async void InstallButton_Clicked(object sender, EventArgs e)
    {
        Logger.StartNewLogFile();
        UpdateInstallIndicator(true);
        Console.WriteLine("Installing workloads...");

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
            UpdateInstallIndicator(false);
            Console.WriteLine("Workload installation complete!");
            InstallLabel.Text = "Workload installation complete!";
            InstallLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            UpdateInstallIndicator(false);
            Console.WriteLine($"Workload installation failed with:\n{ex};");
            InstallLabel.Text = $"Workload installation failed with:\n {ex.Message}";
            InstallLabel.IsVisible = true;
        }
    }

    public void UpdateInstallIndicator(bool isStarting)
    {
        InstallButton.IsEnabled = !isStarting;
        InstallLabel.Text = "Installing workloads...";
        InstallLabel.IsVisible = isStarting;
        InstallIndicator.IsRunning = isStarting;
    }

}

