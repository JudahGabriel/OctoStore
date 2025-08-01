using OctoStore.Models;
using Raven.Client.Documents;

namespace OctoStore.Services;

/// <summary>
/// Background service that periodically searches all of GitHub for ms-store-publish.json files and stores them in the database as AppSubmission objects.
/// </summary>
public class GitHubPublishManifestFinder : TimedBackgroundServiceBase
{
    private readonly GitHubService gitHub;
    private readonly IDocumentStore db;
    private readonly AppSubmissionService appSubmissionSvc;

    public GitHubPublishManifestFinder(
        GitHubService gitHubService, 
        AppSubmissionService appSubmissionService,
        IDocumentStore db, 
        ILogger<GitHubPublishManifestFinder> logger)
        : base(TimeSpan.FromSeconds(5), TimeSpan.FromHours(6), logger)
    {
        this.gitHub = gitHubService;
        this.appSubmissionSvc = appSubmissionService;
        this.db = db;
    }

    public override async Task DoWorkAsync(CancellationToken cancelToken)
    {
        logger.LogInformation("Finding ms-store-publish.json manifests on GitHub at {date}", DateTime.UtcNow);
        try
        {
            var processedFileCount = await FindManifestsOnGitHub();
            logger.LogInformation("Completed processing {count} ms-store-publish.json files from GitHub.", processedFileCount);
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error while finding manifests on GitHub. Will try again later.");
        }
    }

    public async Task<int> FindManifestsOnGitHub()
    {
        // Note: this will return the search result from the repo's default branch, e.g. main or master, unless otherwise specified.
        // SearchFile name will not return results from other branches, which is what we want.
        var manifestFiles = await gitHub.SearchFileName(StorePublishManifest.ManifestFileName, 1, 100);
        int processedFileCount = 0;
        if (manifestFiles.Count == 0)
        {
            logger.LogInformation("Found no ms-store-publish.json files.");
            return processedFileCount;
        }

        using var dbSession = db.OpenAsyncSession();
        foreach (var file in manifestFiles)
        {
            try
            {
                await appSubmissionSvc.SaveAppSubmission(file, dbSession);
                await dbSession.SaveChangesAsync();
                processedFileCount++;
            }
            catch (Exception error)
            {
                logger.LogError(error, "Error while processing ms-store-publish.json file on {repo}. Skipping and continuing processing other files.", file.Repository.FullName);
            }
        }

        return processedFileCount;
    }
}
