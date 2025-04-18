namespace OctoStore.Models
{
    public class AppSubmission
    {
        public required string Id { get; set; }
        public DateTimeOffset SubmissionDate { get; set; } = DateTimeOffset.UtcNow;
        public required StorePublishManifest Manifest { get; set; }
        public string? ManifestSha { get; set; }
        public required Uri ManifestUrl { get; set; }
        public required Uri RepositoryUrl { get; set; }
        public required AppSubmissionStatus Status { get; set; }
        public string? ErrorMessage { get; set; }

        public static string GetIdFromManifestUrl(string manifestUrl)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(manifestUrl, nameof(manifestUrl));

            return $"AppSubmissions/{manifestUrl.Replace("https://github.com/", string.Empty)}";
        }
    }
}
