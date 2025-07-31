namespace OctoStore.Common;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A JSON converter that converts a string to an absolute URI given some base URI and path.
/// For example, new GitHubRepoAbsoluteUriConverter("https://github.com/owner/repo") will convert "/blob/master/foo.png" to "https://github.com/owner/repo/blob/master/foo.png"
/// </summary>
public class GitHubRepoUriConverter : JsonConverter<Uri>
{
    private static readonly Uri GitHub = new Uri("https://github.com", UriKind.Absolute);

    private readonly string gitHubRepoFullName;

    /// <summary>
    /// Creates a new instance of the GitHubRepoUriConverter.
    /// </summary>
    /// <param name="gitHubRepoFullName">The FullName of a GitHub repo. This is typically "owner/repo", e.g. "JudahGabriel/etzmitzvot".</param>
    /// <exception cref="ArgumentNullException"></exception>
    public GitHubRepoUriConverter(string gitHubRepoFullName)
    {
        this.gitHubRepoFullName = gitHubRepoFullName ?? throw new ArgumentNullException(nameof(gitHubRepoFullName));
    }

    public override Uri? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var uriString = reader.GetString();
        if (string.IsNullOrWhiteSpace(uriString))
        {
            return null;
        }

        var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
        {
            var pathWithOwnerAndRepo = $"/{gitHubRepoFullName}/{uriString.TrimStart('/')}"; // "/foo/bar.png" -> "/owner/repo/foo/bar.png"
            return new Uri(GitHub, pathWithOwnerAndRepo);
        }
        return uri;
    }

    public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
