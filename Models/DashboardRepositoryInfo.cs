using Octokit;

namespace OctoStore.Models;

public class DashboardRepositoryInfo
{
    public required Repository Repository { get; set; }
    public AppSubmission? AppSubmission { get; set; }
    public bool HasManifest { get; set; }
}