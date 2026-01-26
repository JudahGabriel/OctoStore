using Microsoft.Extensions.Options;
using Octokit;
using OctoStore.Models;

namespace OctoStore.Services;

/// <summary>
/// Service that interacts with the GitHub API.
/// </summary>
public class GitHubService
{
    private readonly GitHubClient client;
    private readonly HttpClient httpClient;
    private readonly ILogger<GitHubService> logger;

    public GitHubService(IOptions<GitHubSettings> gitHubSettings, HttpClient httpClient, ILogger<GitHubService> logger)
    {
        client = new GitHubClient(new ProductHeaderValue("OctoStore"));
        if (!string.IsNullOrEmpty(gitHubSettings.Value.Token))
        {
            client.Credentials = new Credentials(gitHubSettings.Value.Token);
        }

        this.httpClient = httpClient;
        this.logger = logger;
    }

    /// <summary>
    /// Searches all GitHub public repositories for the specified file name. Note that GitHub does not search forks unless they have more stars than the upstream repo.
    /// </summary>
    /// <param name="fileName">The file name to search for.</param>
    /// <param name="page">The page number to search.</param>
    /// <param name="take">The number of items to return in each page.</param>
    /// <returns>A list of search results.</returns>
    public async Task<IReadOnlyList<SearchCode>> SearchFileName(string fileName, int page = 1, int take = 100)
    {
        var searchRequest = new SearchCodeRequest(fileName)
        {
            In = [CodeInQualifier.Path],
            PerPage = take,
            Page = page,
            FileName = fileName
        };
        var searchResults = await client.Search.SearchCode(searchRequest);
        return searchResults.Items;
    }

    /// <summary>
    /// Gets the URL of a GitHub repository, or null if the repository doesn't exist.
    /// </summary>
    /// <param name="repoOwner">The GitHub user who owns the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <returns>The HTML URL of the repository if it exists, otherwise null.</returns>
    public async Task<Uri?> GetRepoUrl(string repoOwner, string repoName)
    {
        try
        {
            var repo = await client.Repository.Get(repoOwner, repoName);
            return new Uri(repo.HtmlUrl);
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Unable to find repo URL for {owner}/{repoName} due to an error.", repoOwner, repoName);
            return null;
        }
    }

    /// <summary>
    /// Gets the repo owner and repo name from a GitHub repository URL.
    /// </summary>
    /// <param name="gitHubUrl">A GitHub URL.</param>
    /// <returns>The repository's unique name, e.g. "JudahGabriel/Chavah"</returns>
    /// <exception cref="ArgumentException"></exception>
    public string GetRepoNameFromGitHubUrl(Uri gitHubUrl)
    {
        if (!gitHubUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("The provided URL must be an absolute URL.", nameof(gitHubUrl));
        }

        if (gitHubUrl.Host != "github.com")
        {
            throw new ArgumentException("The provided URL is not a valid GitHub URL.", nameof(gitHubUrl));
        }

        var segments = gitHubUrl.AbsolutePath.Trim('/').Split('/');
        if (segments.Length < 2)
        {
            throw new ArgumentException("The provided URL is not a valid GitHub repository URL.", nameof(gitHubUrl));
        }

        var repoOwner = segments[0];
        var repoName = segments[1];
        return $"{repoOwner}/{repoName}";
    }

    public async Task<SearchCode?> SearchRepoForFileName(string fileName, string repoOwner, string repoName, int page = 1, int take = 10)
    {
        // See if GitHub Search will find it. Note that Search does not index forked repos.
        var searchRequest = new SearchCodeRequest(fileName)
        {
            In = [CodeInQualifier.Path],
            PerPage = take,
            Page = page,
            FileName = fileName,
            Repos = [$"{repoOwner}/{repoName}"]
        };
        var searchResults = await client.Search.SearchCode(searchRequest);
        if (searchResults.TotalCount > 0)
        {
            return searchResults.Items.FirstOrDefault();
        }
        
        // Enumerate all the files in the repo's default branch and see if we can find it that way.
        var repo = await client.Repository.Get(repoOwner, repoName);
        var branch = repo.DefaultBranch;
        var reference = await client.Git.Reference.Get(repoOwner, repoName, $"heads/{branch}");
        var sha = reference.Object.Sha;
        var tree = await client.Git.Tree.GetRecursive(repoOwner, repoName, sha);
        var match = tree.Tree.FirstOrDefault(item =>
            item.Type == TreeType.Blob &&
            item.Path.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            return new SearchCode(match.Path, match.Path, match.Sha, match.Url, "", $"{repo.HtmlUrl}/blob/{sha}/{match.Path}", repo);
        }

        return null;
    }

    /// <summary>
    /// Gets the raw string content of a file on GitHub.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="repo"></param>
    /// <param name="path"></param>
    /// <param name="branch"></param>
    /// <returns></returns>
    public async Task<string> GetFileContent(string owner, string repo, string path, string? branch = null)
    {
        var fileContent = await client.Repository.Content.GetRawContent(owner, repo, path);
        return System.Text.Encoding.UTF8.GetString(fileContent);
    }

    /// <summary>
    /// Gets the raw string content of a file on GitHub using the repository URL.
    /// </summary>
    /// <param name="repoUrl"></param>
    /// <param name="path"></param>
    /// <param name="branch"></param>
    /// <returns></returns>
    public Task<string> GetFileContent(string repoUrl, string path, string? branch = null)
    {
        var urlParts = repoUrl.Split('/');
        var owner = urlParts[^2];
        var repo = urlParts[^1];
        return GetFileContent(owner, repo, path, branch);
    }

    /// <summary>
    /// Gets the latest release from the specified repository.
    /// </summary>
    /// <param name="repo">The repo to get the latest release from.</param>
    /// <returns>The latest production release for the repo. Null if no production release was found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The repo's full name isn't formatted right.</exception>
    public async Task<Release?> TryGetLatestRelease(Repository repo)
    {
        var parts = repo.FullName.Split('/');
        if (parts.Length != 2)
        {
            logger.LogWarning("Unable to get latest release for repository. Repository expected to be in the format 'owner/repo', but was {repoFullName}", repo.FullName);
            return null;
        }

        var owner = parts[0];
        var repoName = parts[1];
        try 
        {
            var latestRelease = await client.Repository.Release.GetLatest(owner, repoName);
            return latestRelease;
        }
        catch (NotFoundException)
        {
            logger.LogInformation("No releases found for repository {repoFullName}", repo.FullName);
            return null;
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Unable to fetch latest release for {repo}", repo.FullName);
            return null;
        }
    }

    /// <summary>
    /// Downloads a file from the specified GitHub release URL and returns its content as a stream.
    /// </summary>
    /// <param name="gitHubReleaseFile">The URI of the GitHub release file to download.</param>
    /// <returns>A stream containing the downloaded file content.</returns>
    public async Task<Stream> DownloadGitHubReleaseFile(Uri gitHubReleaseFile)
    {
        if (!gitHubReleaseFile.IsAbsoluteUri)
        {
            throw new ArgumentException("The provided URL must be an absolute URL.", nameof(gitHubReleaseFile));
        }
        if (gitHubReleaseFile.Host != "github.com")
        {
            throw new ArgumentException("The provided URL must be a GitHub URL.", nameof(gitHubReleaseFile));
        }
        if (gitHubReleaseFile.AbsolutePath.Contains("/releases/download/") == false)
        {
            throw new ArgumentException("The provided URL must be a GitHub release download URL.", nameof(gitHubReleaseFile));
        }

        try
        {
            httpClient.DefaultRequestHeaders.Remove("User-Agent");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "OctoStore");
            
            logger.LogInformation("Downloading file from {url}", gitHubReleaseFile);
            var response = await httpClient.GetAsync(gitHubReleaseFile);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStreamAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download file from {url}", gitHubReleaseFile);
            throw;
        }
    }
}
