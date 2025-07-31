namespace OctoStore.Models
{
    /// <summary>
    /// A submission of an app on GitHub to the Microsoft Store.
    /// </summary>
    public class AppSubmission
    {
        /// <summary>
        /// The ID of the app submission. This is typically derived from the repository name, e.g. "AppSubmissions/owner/repo". See <see cref="GetIdFromRepositoryName(string)"/> for details."/>
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// The submission date of the app. This is set to the current UTC time when the submission is created.
        /// </summary>
        public DateTimeOffset SubmissionDate { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The parsed ms-store-publish.json manifest. This will be null if the manifest could not be parsed due to malformed content.
        /// </summary>
        public StorePublishManifest? Manifest { get; set; }

        /// <summary>
        /// The SHA of the ms-store-publish.json file in the repository. This is used to determine if the manifest has changed since the last submission.
        /// </summary>
        public string? ManifestSha { get; set; }

        /// <summary>
        /// The URI to the ms-store-publish.json file in the repository.
        /// </summary>
        public required Uri ManifestUrl { get; set; }

        /// <summary>
        /// The URI to the GitHub repository where the ms-store-publish.json file is located.
        /// </summary>
        public required Uri RepositoryUrl { get; set; }

        /// <summary>
        /// The status of the app submission.
        /// </summary>
        public required AppSubmissionStatus Status { get; set; }

        /// <summary>
        /// Any error message associated with the submission. This is set if the submission fails to process or if the manifest cannot be parsed.
        /// This error message is shown to the developer when they view their submission status.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets the ID for an app submission based on the repository name.
        /// </summary>
        /// <param name="repoName">The full name of the repository on GitHub which includes both the owner's GitHub name and the repository name, e.g. "JudahGabriel/etzmitzvot"</param>
        /// <returns>An ID for the app submission, e.g. "AppSubmissions/JudahGabriel/etzmitzvot"</returns>
        public static string GetIdFromRepositoryName(string repoName)
        {
            ArgumentException.ThrowIfNullOrEmpty(repoName, nameof(repoName));

            return $"AppSubmissions/{repoName}";
        }
    }
}
