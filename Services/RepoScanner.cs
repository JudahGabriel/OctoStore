using OctoStore.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace OctoStore.Services;

/// <summary>
/// Background service that looks for RepositoryScanRequests in the database and scans them.
/// </summary>
public class RepoScanner : TimedBackgroundServiceBase
{
    private readonly GitHubService gitHubService;
    private readonly IDocumentStore db;
    private readonly AppSubmissionService appSubmissionService;

    public RepoScanner(
        GitHubService gitHubService,
        AppSubmissionService appSubmissionService,
        IDocumentStore db,
        ILogger<RepoScanner> logger)
        : base(TimeSpan.FromSeconds(5), TimeSpan.FromHours(6), logger)
    {
        this.gitHubService = gitHubService;
        this.appSubmissionService = appSubmissionService;
        this.db = db;
    }

    public override async Task DoWorkAsync(CancellationToken cancelToken)
    {
        logger.LogInformation("Looking for repository scan requests at {date}", DateTime.UtcNow);
        try
        {
            await ProcessRepoScanRequests();
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error while processing repo scan requests. Will try again later.");
        }
    }

    private async Task ProcessRepoScanRequests()
    {
        var yesterday = DateTimeOffset.UtcNow.AddDays(-1);
        using var dbSession = db.OpenAsyncSession();
        var repoScanRequests = await dbSession
            .Query<RepositoryScanRequest>()
            .Where(x => x.Scanned == null || x.Scanned < yesterday)
            .ToListAsync();
        foreach (var scanRequest in repoScanRequests)
        {
            try
            {
                await ProcessRepoScanRequest(scanRequest, dbSession);
            }
            catch (Exception error)
            {
                logger.LogError(error, "Error while processing repository scan request for {owner}/{repo}. Skipping.", scanRequest.Owner, scanRequest.Repo);
            }
        }
    }

    private async Task ProcessRepoScanRequest(RepositoryScanRequest scanRequest, IAsyncDocumentSession dbSession)
    {
        logger.LogInformation("Processing repository scan request for {owner}/{repo}", scanRequest.Owner, scanRequest.Repo);

        // Scan the repo for ms-store-publish.json files.
        var msStorePublishFile = await gitHubService.SearchRepoForFileName(StorePublishManifest.ManifestFileName, scanRequest.Owner, scanRequest.Repo, 1, 5);
        if (msStorePublishFile == null)
        {
            logger.LogInformation("Repo scan request for {owner}/{repo} found no ms-store-publish.json files.", scanRequest.Owner, scanRequest.Repo);
            scanRequest.Scanned = DateTimeOffset.UtcNow;
            return;
        }

        logger.LogInformation("Found ms-store-publish.json files in {owner}/{repo}.", scanRequest.Owner, scanRequest.Repo);
        scanRequest.Scanned = DateTimeOffset.UtcNow;
        await appSubmissionService.SaveAppSubmission(msStorePublishFile, dbSession);
        await dbSession.SaveChangesAsync();
    }
}
