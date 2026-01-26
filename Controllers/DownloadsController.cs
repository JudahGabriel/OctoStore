using Microsoft.AspNetCore.Mvc;
using OctoStore.Models;
using OctoStore.Services;
using Raven.Client.Documents.Session;

namespace OctoStore.Controllers;

[ApiController]
[Route("[controller]")]
public class DownloadsController : Controller
{
    /// <summary>
    /// Downloads a GitHub release binary from the provided URL. This endpoint fetches the binary without any redirects, which is necessary for Rapid App Publish flow.
    /// The URL must point to a GitHub release URL (e.g. https://github.com/{org}/{app}/releases/download...), and the org and app must have an app submission in progress.
    /// </summary>
    /// <param name="gitHubReleaseBinaryUrl">The URL to a binary on GitHub releases.</param>
    /// <param name="gitHub">The GitHub service.</param>
    /// <returns>A file containing the binary.</returns>
    [HttpGet("getReleaseBinary")]
    public async Task<IActionResult> GetReleaseBinary(Uri gitHubReleaseBinaryUrl, [FromServices] IAsyncDocumentSession dbSession, [FromServices] GitHubService gitHub)
    {
        // Ensure this is a GitHub release URL.
        if (!gitHubReleaseBinaryUrl.IsAbsoluteUri)
        {
            return BadRequest("The provided URL must be an absolute URL.");
        }

        if (gitHubReleaseBinaryUrl.Host != "github.com")
        {
            return BadRequest("The provided URL must be a GitHub URL.");
        }

        if (gitHubReleaseBinaryUrl.LocalPath.Contains("/releases/download/") == false)
        {
            return BadRequest("The provided URL must be a GitHub release download URL.");
        }

        var repoName = gitHub.GetRepoNameFromGitHubUrl(gitHubReleaseBinaryUrl);

        // Ensure the repo has an app submission in progress.
        var appSubmission = await dbSession.LoadAsync<AppSubmission>(AppSubmission.GetIdFromRepositoryName(repoName));
        if (appSubmission == null)
        {
            return NotFound("No app submission found for the provided repository.");
        }

        // OK, all is well. We have an app submission. Fetch the download.
        try
        {
            var fileStream = await gitHub.DownloadGitHubReleaseFile(gitHubReleaseBinaryUrl);
            var fileName = Path.GetFileName(gitHubReleaseBinaryUrl.LocalPath);
            return File(fileStream, "application/octet-stream", fileName);
        }
        catch (Exception)
        {
            return Problem("Failed to download the file from GitHub.");
        }
        
    }
}
