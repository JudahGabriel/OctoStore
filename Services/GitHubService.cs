using Microsoft.Extensions.Options;
using Octokit;
using OctoStore.Models;
using System.Text;

namespace OctoStore.Services;

/// <summary>
/// Service that interacts with the GitHub API.
/// </summary>
public class GitHubService
{
    private readonly GitHubClient client;
    public GitHubService(IOptions<AppSettings> appSettings, ILogger<GitHubService> logger)
    {
        client = new GitHubClient(new ProductHeaderValue("OctoStore"));
        if (!string.IsNullOrEmpty(appSettings.Value.GitHubToken))
        {
            client.Credentials = new Credentials(appSettings.Value.GitHubToken);
        }
    }

    /// <summary>
    /// Searches GitHub for the specified file name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<SearchCode>> SearchFileName(string fileName, int take = 100)
    {
        var searchRequest = new SearchCodeRequest(fileName)
        {
            In = [CodeInQualifier.Path],
            PerPage = take,
            FileName = fileName
        };
        var searchResults = await client.Search.SearchCode(searchRequest);
        return searchResults.Items;
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

    public Task<string> GetFileContent(string repoUrl, string path, string? branch = null)
    {
        var urlParts = repoUrl.Split('/');
        var owner = urlParts[^2];
        var repo = urlParts[^1];
        return GetFileContent(owner, repo, path, branch);
    }
}
