using Microsoft.Extensions.Options;
using Octokit;
using OctoStore.Models;
using Raven.Client.Documents.Linq.Indexing;
using System.Text;

namespace OctoStore.Services;

/// <summary>
/// Service that interacts with the GitHub API.
/// </summary>
public class GitHubService
{
    private readonly GitHubClient client;
    private readonly ILogger<GitHubService> logger;

    public GitHubService(IOptions<AppSettings> appSettings, ILogger<GitHubService> logger)
    {
        client = new GitHubClient(new ProductHeaderValue("OctoStore"));
        if (!string.IsNullOrEmpty(appSettings.Value.GitHubToken))
        {
            client.Credentials = new Credentials(appSettings.Value.GitHubToken);
        }

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
}
