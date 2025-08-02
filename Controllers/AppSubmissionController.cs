using Microsoft.AspNetCore.Mvc;
using OctoStore.Models;
using OctoStore.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

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
    public async Task<IActionResult> Status([FromQuery]string repo, [FromServices] IAsyncDocumentSession dbSession)
    {
        var appsSubmissionForRepo = await dbSession.LoadAsync<AppSubmission>(AppSubmission.GetIdFromRepositoryName(repo));
        return View("status", appsSubmissionForRepo);
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
