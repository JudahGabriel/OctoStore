using Microsoft.AspNetCore.Mvc;
using OctoStore.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace OctoStore.Controllers;

[ApiController]
[Route("[controller]")]
public class AppSubmissionController : ControllerBase
{
    private readonly ILogger<AppSubmissionController> _logger;

    public AppSubmissionController(ILogger<AppSubmissionController> logger)
    {
        _logger = logger;
    }

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="appManifest"></param>
    ///// <param name="key"></param>
    ///// <returns></returns>
    //[HttpPost]
    //public IActionResult Submit(
    //    [FromBody] StorePublishManifest appManifest, 
    //    [FromQuery] string? key,
    //    [FromServices] IAsyncDocumentSession dbSession)
    //{
    //    return Ok();
    //}

    [HttpGet("")]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll([FromServices] IAsyncDocumentSession dbSession)
    {
        var submissions = await dbSession.Query<AppSubmission>().ToListAsync();
        return Ok(submissions);
    }
}
