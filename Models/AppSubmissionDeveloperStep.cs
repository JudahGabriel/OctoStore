namespace OctoStore.Models
{
    /// <summary>
    /// The steps a developer has taken to get their open source app on GitHub published to the Microsoft Store. These are sequential steps the developer must take to publish their app in the Store.
    /// </summary>
    public enum AppSubmissionDeveloperStep
    {
        /// <summary>
        /// The developer hasn't yet done the first step of adding an ms-store-publish.json file to their repo.
        /// </summary>
        None = 0,

        /// <summary>
        /// The developer has added an ms-store-publish.json manifest file to their repository.
        /// </summary>
        AddManifestToRepo = 1,

        /// <summary>
        /// The developer has linked their Partner Center account.
        /// </summary>
        LinkPartnerCenterAccount = 2,

        /// <summary>
        /// The developer has invited OctoStore to their Partner Center account.
        /// </summary>
        /// <remarks>
        /// The user must go to https://admin.cloud.microsoft/?#/GuestUsers, signing in with their Partner Center account, and add opensourceappsongithub@outlook.com as a guest user.
        /// </remarks>
        InviteOctoStoreToPartnerCenter = 3,

        /// <summary>
        /// The developer has assigned the Developer role to OctoStore in their Partner Center account.
        /// </summary>
        /// <remarks>
        /// This can be done via Partner Center → Account settings → User management → Select your user → Assign roles → Check "Developer" → Save changes. This allows OctoStore to create and manage app submissions on behalf of the developer.
        /// </remarks>
        AssignDeveloperRoleToOctoStore = 4
    }
}
