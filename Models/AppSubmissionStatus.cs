using System.ComponentModel;

namespace OctoStore.Models;

/// <summary>
/// The status of an app submission. 
/// </summary>
public enum AppSubmissionStatus
{
    /// <summary>
    /// The repository is being scanned for the ms-store-publish.json file.
    /// </summary>
    [Description("Scanning the repository for ms-store-publish.json")]
    Scanning,

    /// <summary>
    /// The app submission is waiting for the developer to sign the Microsoft Store App Developer Agreement: https://docs.microsoft.com/legal/windows/agreements/app-developer-agreement
    /// </summary>
    [Description("Waiting for you to sign the App Developer Agreement")]
    WaitingToSignAppDevAgreement,

    /// <summary>
    /// The app submission is being processed by the Store team.
    /// </summary>
    [Description("Processing")]
    Processing,

    /// <summary>
    /// The app submission is awaiting review from Partner Center.
    /// </summary>
    [Description("Awaiting review")]
    AwaitingReview,

    /// <summary>
    /// The app submission has been published to the Microsoft Store. The app submission will have a StoreProductId assigned to it.
    /// </summary>
    [Description("Published")]
    Published,

    /// <summary>
    /// The app submission is in an error state. The app submission will have an ErrorMessage with details.
    /// </summary>
    [Description("Error")]
    Error
}
