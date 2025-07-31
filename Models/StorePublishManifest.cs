namespace OctoStore.Models;

// v1 omits:
// pricing, availability, architectures, subcategory, system requirements, age rating, MSIX and exe submissions,
// custom package image overrides, submission options, notes for certification, version

public class StorePublishManifest
{
    /// <summary>
    /// The app name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The URL to the app icon. This should be a publicly accessible URL that points to the icon image. This can be either an absolute path or a path relative to the GitHub repository URL. It should be a PNG or JPG image.
    /// </summary>
    public required Uri IconUrl { get; set; }

    /// <summary>
    /// The category of the app.
    /// </summary>
    public required Category Category { get; set; }

    /// <summary>
    /// The email address of the developer who submitted the app. This is used to contact the developer about the status of their submission.
    /// </summary>
    public required string? DeveloperEmail { get; set; }

    /// <summary>
    /// The URL to the privacy policy of the app.
    /// </summary>
    public required Uri PrivacyPolicyUrl { get; set; }

    /// <summary>
    /// The store listings for the app, one for each language for the Store listing. Most apps will have an English ("en") listing, but they can also have listings for other languages such as "fr" (French), "de" (German), etc.
    /// </summary>
    public required List<StoreListing> StoreListings { get; set; }

    /// <summary>
    /// The secondary category of the app, if applicable.
    /// </summary>
    public Category? SecondaryCategory { get; set; }

    /// <summary>
    /// The support information for the app.
    /// </summary>
    public SupportInfo? Support { get; set; }

    /// <summary>
    /// The app package information for a progressive web app (PWA). This will be null if the app is not a PWA.
    /// </summary>
    public ProgressiveWebAppPackage? PwaPackage { get; set; }
}
