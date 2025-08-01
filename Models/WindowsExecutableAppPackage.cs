namespace OctoStore.Models;

/// <summary>
/// Information about the Windows executable app package that will be submitted to the Microsoft Store.
/// </summary>
public class WindowsExecutableAppPackage
{
    /// <summary>
    /// The URL to the Windows executable file (.exe or .msi) that will be submitted to the Microsoft Store. 
    /// This can be an absolute URL or a relative path on the repo. 
    /// The URI may also contain {{tag}} or {{version}} to the latest release on GitHub, e.g. "/{{latest-release}}/npp.{{version:3}}.Installer.exe"
    /// </summary>
    public required Uri Url { get; set; }
}
