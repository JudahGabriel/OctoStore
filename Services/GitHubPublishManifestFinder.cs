using Microsoft.Extensions.Options;
using Octokit;
using OctoStore.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Raven.Client.Documents;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OctoStore.Services;

/// <summary>
/// Background service that periodically searches GitHub for ms-store-publish.json files and begins the publish process.
/// </summary>
public class GitHubPublishManifestFinder : TimedBackgroundServiceBase
{
    private const string ManifestFileName = "ms-store-publish.json";
    private readonly GitHubService gitHubService;
    private readonly IDocumentStore db;

    public GitHubPublishManifestFinder(GitHubService gitHubService, IDocumentStore db, ILogger<GitHubPublishManifestFinder> logger)
        : base(TimeSpan.FromSeconds(5), TimeSpan.FromHours(6), logger)
    {
        this.gitHubService = gitHubService;
        this.db = db;
    }

    public override async Task DoWorkAsync(CancellationToken cancelToken)
    {
        try
        {
            await FindManifestsOnGitHub();
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error while finding manifests on GitHub. Will try again later.");
        }
    }

    public async Task FindManifestsOnGitHub()
    {
        // Note: this will return the search result from the repo's default branch, e.g. main or master, unless otherwise specified.
        // SearchFile name will not return results from other branches, which is what we want.
        var manifestFiles = await gitHubService.SearchFileName(ManifestFileName, 100);
        if (manifestFiles.Count == 0)
        {
            logger.LogInformation("Found no ms-store-publish.json files");
        }

        using var dbSession = db.OpenAsyncSession();
        foreach (var file in manifestFiles)
        {
            var submissionId = AppSubmission.GetIdFromRepositoryName(file.Repository.FullName);

            // See if we have this submission already.
            var existingSubmission = await dbSession.LoadAsync<AppSubmission>(submissionId);
            if (existingSubmission != null && existingSubmission.ManifestSha == file.Sha)
            {
                logger.LogInformation("Found ms-store-publish.json at {url} with SHA {sha}, but we've already processed the file.", file.HtmlUrl, file.Sha);
                continue;
            }

            // Create the app submission and save it to the database.
            var submission = new AppSubmission
            {
                Id = submissionId,
                ManifestSha = file.Sha,
                ManifestUrl = new Uri(file.HtmlUrl),
                RepositoryUrl = new Uri(file.Repository.Url),
                SubmissionDate = DateTimeOffset.UtcNow,
                Status = AppSubmissionStatus.Processing
            };

            // See if the ms-store-publish.json file can be loaded and parsed.
            var manifest = await TryLoadManifest(file);
            manifest.Match(val => submission.Manifest = val);
            manifest.MatchException(err => 
            {
                submission.Status = AppSubmissionStatus.Error;
                submission.ErrorMessage = err;
                logger.LogError("Failed to load manifest from {url}. Error: {error}", file.HtmlUrl, err);
            });

            await dbSession.StoreAsync(submission);
            await dbSession.SaveChangesAsync();
            logger.LogInformation("Found ms-store-publish.json at {url} with SHA {sha}. Saved to database.", file.HtmlUrl, file.Sha);
        }
    }

    private async Task<Either<StorePublishManifest, string>> TryLoadManifest(SearchCode file)
    {
        string manifestContent;
        try
        {
            manifestContent = await gitHubService.GetFileContent(file.Repository.Url, file.Path);
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
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
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
