using Microsoft.AspNetCore.Mvc;
using OctoStore.Models;
using OctoStore.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Octokit;

namespace OctoStore.Controllers;

[ApiController]
[Route("[controller]")]
public class AppSubmissionController : Controller
{
    private readonly ILogger<AppSubmissionController> _logger;
    private readonly GitHubService gitHub;

    public AppSubmissionController(GitHubService gitHub, ILogger<AppSubmissionController> logger)
    {
        this.gitHub = gitHub;
        _logger = logger;
    }

    [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> Status([FromQuery]string repo, [FromServices] IAsyncDocumentSession dbSession)
    {
        var appsSubmissionForRepo = await dbSession.LoadAsync<AppSubmission>(AppSubmission.GetIdFromRepositoryName(repo));
        return View("status", appsSubmissionForRepo);
    }

    [HttpGet("dashboard")]
    [Authorize]
    public async Task<IActionResult> Dashboard([FromServices] IAsyncDocumentSession dbSession)
    {
        var accessToken = HttpContext.User.FindFirst("access_token")?.Value;
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge("GitHub");
        }

        // Create a GitHub client with the user's access token
        var userGitHubClient = new GitHubClient(new ProductHeaderValue("OctoStore"))
        {
            Credentials = new Credentials(accessToken)
        };

        try
        {
            // Get all repositories for the authenticated user
            var repos = await userGitHubClient.Repository.GetAllForCurrent();
            
            // Get app submissions for each repository
            var dashboardData = new List<DashboardRepositoryInfo>();
            
            foreach (var repo in repos)
            {
                var submissionId = AppSubmission.GetIdFromRepositoryName(repo.FullName);
                var submission = await dbSession.LoadAsync<AppSubmission>(submissionId);
                
                // Check if the repository has ms-store-publish.json
                var hasManifest = submission?.Manifest != null;
                
                dashboardData.Add(new DashboardRepositoryInfo
                {
                    Repository = repo,
                    AppSubmission = submission,
                    HasManifest = hasManifest
                });
            }

            // Sort repositories: those with manifests first, then by name
            var sortedData = dashboardData
                .OrderByDescending(r => r.HasManifest)
                .ThenBy(r => r.Repository.Name)
                .ToList();

            return View("dashboard", sortedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching repositories for dashboard");
            return View("Error");
        }
    }

    /// <summary>
    /// Schedules a scan of the specified repository for ms-store-publish.json files. If found, the app submission will be created in the database.
    /// </summary>
    /// <param name="repo">The repository's full name in the form of "owner/repo", e.g. "JudahGabriel/etzmitzvot"</param>
    /// <returns>OK if the repository was scheduled for scanning.</returns>
    [HttpPost("scan")]
    public async Task<IActionResult> Scan(
        [FromQuery] string repo,
        [FromServices] IAsyncDocumentSession dbSession)
    {
        var (owner, repoName) = RepositoryScanRequest.ValidateRepositoryFullName(repo);
        var repoScanRequest = new RepositoryScanRequest
        {
            Owner = owner,
            Repo = repoName
        };
        await dbSession.StoreAsync(repoScanRequest, $"RepositoryScanRequests/{owner}/{repoName}");
        await dbSession.SaveChangesAsync();
        return Ok("Scan requested");
    }

    //[HttpGet("getAll")]
    //public async Task<IActionResult> GetAll([FromServices] IAsyncDocumentSession dbSession)
    //{
    //    var submissions = await dbSession.Query<AppSubmission>().ToListAsync();
    //    return Ok(submissions);
    //}
}
