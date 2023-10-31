using System;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Maui.Dogfood.Feeder;

public class ArtifactClient : HttpClient
{
    const int RESULTS_PER_PAGE = 100;

    const string NUGET_ARTIFACT_CONTEXT = "nuget-artifacts";

    readonly string AuthToken;

    public ArtifactClient()
    {
        var tokenFromFile = File.Exists(Env.GitHubTokenFilePath) ? File.ReadAllText(Env.GitHubTokenFilePath) : string.Empty;
        var tokenFromVar = Environment.GetEnvironmentVariable(Env.GitHubTokenEnvVarName);
        AuthToken = !string.IsNullOrEmpty(tokenFromVar) ? tokenFromVar : tokenFromFile;
        if (string.IsNullOrEmpty(AuthToken))
            throw new InvalidOperationException($"You must provide a GitHub PAT" +
                $" via the `{Path.Combine(Env.RootDirectory, "tokens", "github.token")}` file or `MAUI_DOGFOOD_GH_TOKEN` variable.");
    }

    void SetGitHubRestApiHeaders ()
    {
        DefaultRequestHeaders.Accept.Clear();
        DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("maui-dogfooder", "1"));
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.text-match+json"));
        DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
    }

    void SetInternalxHeaders()
    {
        DefaultRequestHeaders.Accept.Clear();
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", AuthToken);
    }

    public static string FixInternalXUrl (string url)
    {
        return url.Replace("bosstoragemirror.azureedge.net", "dl.internalx.com").Replace("bosstoragemirror.blob.core.windows.net", "dl.internalx.com");
    }

    public async Task<string> GetNuGetArtifactStatusTargetUrlAsync(string org, string repo, string commit)
    {
        SetGitHubRestApiHeaders();

        using Stream statusResultStream = await GetStreamAsync($"https://api.github.com/repos/{org}/{repo}/commits/{commit}/status?per_page={RESULTS_PER_PAGE}");
        CommitStatusResult? statusResult = await JsonSerializer.DeserializeAsync<CommitStatusResult>(statusResultStream);

        if (statusResult is null)
            throw new Exception($"Failed to deserialize '{org}/{repo}/commits/{commit}/status'.");

        if (statusResult.total_count < 1)
            throw new Exception($"The commit '{repo}@{commit}' did not contain any commit statuses, please provide a different one.");

        CommitStatus? artifactStatus = statusResult.statuses.FirstOrDefault(s => s.context == NUGET_ARTIFACT_CONTEXT);

        if (artifactStatus is null || artifactStatus == default(CommitStatus))
            throw new Exception($"The commit '{repo}@{commit}' did not contain the required '{NUGET_ARTIFACT_CONTEXT}' commit status.");

        return artifactStatus.target_url;
    }

    public async Task DownloadNuGetsAsync(string org, string repo, string commit, string destination)
    {
        if (Directory.Exists(destination)) {
            if (Directory.GetFiles(destination, "*.nupkg").Length > 0)
                return;
            else
                Directory.Delete(destination, true);
        }
        Directory.CreateDirectory(destination);

        var artifactsJsonUrl = await GetNuGetArtifactStatusTargetUrlAsync(org, repo, commit);

        SetInternalxHeaders();
        using Stream artifactsJsonStream = await GetStreamAsync(artifactsJsonUrl);
        List<GitHubStatusArtifact>? artifacts = await JsonSerializer.DeserializeAsync<List<GitHubStatusArtifact>>(artifactsJsonStream);

        if (artifacts is null)
            throw new Exception($"Failed to deserialize artifacts.json '{artifactsJsonUrl}'.");

        if (artifacts.Count < 1)
            throw new Exception($"The artifacts.json '{artifactsJsonUrl}' did not contain any artifacts.");


        Console.WriteLine($"Downloading artifacts from '{repo}@{commit}' to '{destination}'.");

        var options = new ParallelOptions {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
        };
        await Parallel.ForEachAsync(artifacts, options, async (a, token) =>
        {
            // Ignore .nupkg files containing MSIs as we are performing a file based install
            if (a.url.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase) && !a.url.Contains(".Msi.", StringComparison.OrdinalIgnoreCase))
            {
                string artifactUrl = FixInternalXUrl(a.url);
                string fileName = Path.GetFileName(artifactUrl);
                using Stream artifactStream = await GetStreamAsync(artifactUrl);
                using Stream streamToWriteTo = File.Open(Path.Combine(destination, fileName), FileMode.Create);
                await artifactStream.CopyToAsync(streamToWriteTo);
            }
        });
    }
}

