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

        // Ensure the logged in GitHub user is the owner of this repo, or is an admin of OctoStore.

        var isAuthorized = appsSubmissionForRepo == null || appsSubmissionForRepo.IsAuthorized(User.Identity?.Name);
        if (!isAuthorized)
        {
            // User is not the owner of the app submission
            return Unauthorized();
        }

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

            var submissionIds = repos.Select(r => AppSubmission.GetIdFromRepositoryName(r.FullName));
            var submissionsMap = await dbSession.LoadAsync<AppSubmission?>(submissionIds);
            foreach (var repo in repos)
            {
                var submissionId = AppSubmission.GetIdFromRepositoryName(repo.FullName);
                var submission = submissionsMap[submissionId];
                
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

        // See if the repo exists on GitHub.
        var repoHtmlUrl = await gitHub.GetRepoUrl(owner, repoName);
        if (repoHtmlUrl == null)
        {
            return BadRequest("The specified repository does not exist on GitHub.");
        }

        var repoId = AppSubmission.GetIdFromRepositoryName(repo);
        var existingRepo = await dbSession.LoadAsync<AppSubmission>(repoId);
        if (existingRepo == null)
        {
            existingRepo = new AppSubmission
            {
                Id = repoId,
                Status = AppSubmissionStatus.Scanning,
                RepositoryUrl = repoHtmlUrl,
                RepositoryUserName = owner
            };
            await dbSession.StoreAsync(existingRepo);
        }
        existingRepo.Status = AppSubmissionStatus.Scanning;

        await dbSession.StoreAsync(repoScanRequest, $"RepositoryScanRequests/{owner}/{repoName}");
        await dbSession.SaveChangesAsync();
        return RedirectToAction("dashboard");
    }

    /// <summary>
    /// Marks the developer agreement as confirmed for a given repository's app submission.
    /// </summary>
    [HttpPost("confirmDeveloperAgreement")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDeveloperAgreement([FromForm] string repo, [FromServices] IAsyncDocumentSession dbSession)
    {
        if (string.IsNullOrWhiteSpace(repo))
        {
            return BadRequest("Repository is required.");
        }

        var id = AppSubmission.GetIdFromRepositoryName(repo);
        var submission = await dbSession.LoadAsync<AppSubmission>(id);
        if (submission is null)
        {
            return NotFound();
        }

        // Ensure the logged in GitHub user is authorized to do this.
        if (!submission.IsAuthorized(User.Identity?.Name))
        {
            return Unauthorized();
        }

        if (!submission.ConfirmedAppDeveloperAgreement)
        {
            submission.ConfirmedAppDeveloperAgreement = true;
            await dbSession.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Status), new { repo });
    }

}
