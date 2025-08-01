using Microsoft.Extensions.Logging;
using Octokit;
using OctoStore.Common;
using OctoStore.Models;
using Raven.Client.Documents.Session;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OctoStore.Services;

/// <summary>
/// Service that creates and manages app submissions in the database.
/// </summary>
public class AppSubmissionService
{
    private readonly GitHubService gitHub;
    private readonly ILogger<AppSubmissionService> logger;
    public AppSubmissionService(GitHubService gitHub, ILogger<AppSubmissionService> logger)
    {
        this.gitHub = gitHub;
        this.logger = logger;
    }

    /// <summary>
    /// Ensures that a ms-store-publish.json file on GitHub is saved as an AppSubmission in the database. 
    /// Note that callers are responsible for committing the changes to the database by calling dbSession.SaveChangesAsync().
    /// </summary>
    /// <param name="file">The ms-store-publish.json file on GitHub.</param>
    /// <param name="dbSession">The database session.</param>
    /// <returns>The saved AppSubmission.</returns>
    public async Task<AppSubmission> SaveAppSubmission(SearchCode file, IAsyncDocumentSession dbSession)
    {
        var submissionId = AppSubmission.GetIdFromRepositoryName(file.Repository.FullName);

        // See if we already have this submission.
        // If so, we can skip processing it if the SHA of the manifest hasn't changed and there are no new releases on GitHub.
        var existingSubmission = await dbSession.LoadAsync<AppSubmission>(submissionId);
        var latestReleaseOnGitHub = await gitHub.TryGetLatestRelease(file.Repository);
        if (existingSubmission != null)
        {
            // We have an existing submission. See if we need to update it.
            // We need to update it if the SHA has changed or if there's a new release on GitHub.
            var hasNewReleaseOnGitHub = latestReleaseOnGitHub?.PublishedAt != null && existingSubmission.LatestGitHubReleaseDate < latestReleaseOnGitHub.PublishedAt;
            if (existingSubmission.ManifestSha == file.Sha && !hasNewReleaseOnGitHub)
            {
                logger.LogInformation("Skipping ms-store-pbulish.json at {url} with SHA {sha}. It's unchanged since we last processed it and there are no new releases on GitHub (GitHub last release date {ghLastReleaseDate}, last processed release date {lastProcessedReleaseDate}", file.HtmlUrl, file.Sha, latestReleaseOnGitHub?.PublishedAt, existingSubmission.LatestGitHubReleaseDate);
                return existingSubmission;
            }

            existingSubmission.ManifestSha = file.Sha;
        }
        else
        {
            // No existing app submission for this ms-store-publish.json file. Create a new app submission.
            existingSubmission = new AppSubmission
            {
                Id = submissionId,
                ManifestSha = file.Sha,
                ManifestUrl = new Uri(file.HtmlUrl),
                RepositoryUrl = new Uri(file.Repository.Url),
                SubmissionDate = DateTimeOffset.UtcNow,
                LatestGitHubReleaseDate = latestReleaseOnGitHub?.PublishedAt,
                Status = AppSubmissionStatus.Processing
            };
        }

        // See if the ms-store-publish.json file can be loaded and parsed.
        var manifestOrError = await TryLoadManifest(file);
        manifestOrError.Match(val => existingSubmission.Manifest = val);
        manifestOrError.MatchException(err => ManifestParseFailed(err, file, latestReleaseOnGitHub, existingSubmission));

        // Save it to the database.
        await dbSession.StoreAsync(existingSubmission);
        logger.LogInformation("Found ms-store-publish.json at {url} with SHA {sha} with latest GitHub release at {date}. Saved to database.", file.HtmlUrl, file.Sha, latestReleaseOnGitHub?.PublishedAt);
        return existingSubmission;
    }

    private void ManifestParseFailed(string errorMessage, SearchCode file, Release? latestReleaseOnGitHub, AppSubmission existingSubmission)
    {
        existingSubmission.ErrorMessage = errorMessage;
        existingSubmission.Manifest = null;
        existingSubmission.Status = AppSubmissionStatus.Error;
        existingSubmission.LatestGitHubReleaseDate = latestReleaseOnGitHub?.PublishedAt;
        logger.LogError("Failed to load manifest from {url}. Error: {error}", file.HtmlUrl, errorMessage);
    }

    private async Task<Either<StorePublishManifest, string>> TryLoadManifest(SearchCode file)
    {
        string manifestContent;
        try
        {
            manifestContent = await gitHub.GetFileContent(file.Repository.Url, file.Path);
        }
        catch (Exception error)
        {
            logger.LogError(error, "Failed to load manifest content from {url}", file.HtmlUrl);
            return new Either<StorePublishManifest, string>(error.Message);
        }

        try
        {
            var manifest = JsonSerializer.Deserialize<StorePublishManifest>(manifestContent, new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new GitHubRepoUriConverter(file.Repository.FullName) // FullName = "owner/repo". This will resolve relative URLs using github.com/owner/repo base the base.
                }
            });
            if (manifest == null)
            {
                logger.LogError("Failed to deserialize manifest content from {url}. Result was null.", file.HtmlUrl);
                return new Either<StorePublishManifest, string>("Failed to deserialize manifest content.");
            }

            return new Either<StorePublishManifest, string>(manifest);
        }
        catch (Exception deserializeError)
        {
            logger.LogError(deserializeError, "Failed to deserialize manifest content from {url}.", file.HtmlUrl);
            return new Either<StorePublishManifest, string>(deserializeError.Message);
        }
    }
}
