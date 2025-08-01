namespace OctoStore.Models;
using System.Linq;

/// <summary>
/// A request to scan a repository for ms-store-publish.json files.
/// </summary>
public class RepositoryScanRequest
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Scanned { get; set; }
    public required string Owner { get; set; }
    public required string Repo { get; set; }

    public static (string owner, string repo) ValidateRepositoryFullName(string repositoryFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(repositoryFullName, nameof(repositoryFullName));
        var parts = repositoryFullName.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 && parts.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Repository full name must be in the format 'owner/repo'.", nameof(repositoryFullName));
        }

        return (parts[0], parts[1]);
    }
}
