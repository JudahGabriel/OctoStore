namespace OctoStore.Models
{
    public class AppSubmission
    {
        public required string Id { get; set; }
        public DateTimeOffset SubmissionDate { get; set; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// The parsed ms-store-publish.json manifest. This will be null if the manifest could not be parsed due to malformed content.
        /// </summary>
        public StorePublishManifest? Manifest { get; set; }
        public string? ManifestSha { get; set; }
        public required Uri ManifestUrl { get; set; }
        public required Uri RepositoryUrl { get; set; }
        public required AppSubmissionStatus Status { get; set; }
        public string? ErrorMessage { get; set; }

        public static string GetIdFromRepositoryName(string repoName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(repoName, nameof(repoName));

            return $"AppSubmissions/{repoName}";
        }
    }
}
