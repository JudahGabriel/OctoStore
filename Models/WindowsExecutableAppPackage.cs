namespace OctoStore.Models;

/// <summary>
/// Information about the Windows executable app package that will be submitted to the Microsoft Store.
/// </summary>
public class WindowsExecutableAppPackage
{
    /// <summary>
    /// The file name of the x64 version of the app in GitHub releases. This can be null if there is no x64 version of the app in GitHub releases.
    /// This file name can contain {{tag}} or {{version}} placeholders, e.g. "MyApp-{{tag}}-x64.exe" or "MyApp-{{version:3}}-x64.exe". 
    /// Tags are usually strings like "v4.9.0.0". Version is the tag excluding letters, e.g. "4.9.0.0". You can specify a optional number between 1 and 4 to indicate how many numbers from the version to use, e.g. "MyApp-{{version:3}}.exe" will become "MyApp-4.9.0.exe"
    /// </summary>
    public string? GitHubReleasesX64FileName { get; set; }

    /// <summary>
    /// The file name of the ARM64 version of the app in GitHub releases. This can be null if there is no ARM64 version of the app in GitHub releases.
    /// This file name can contain {{tag}} or {{version}} placeholders, e.g. "MyApp-{{tag}}-arm64.exe" or "MyApp-{{version:3}}-arm64.exe". 
    /// Tags are usually strings like "v4.9.0.0". Version is the tag excluding letters, e.g. "4.9.0.0". You can specify a optional number between 1 and 4 to indicate how many numbers from the version to use, e.g. "MyApp-{{version:3}}.exe" will become "MyApp-4.9.0.exe"
    /// </summary>
    public string? GitHubReleasesArm64FileName { get; set; }
}
