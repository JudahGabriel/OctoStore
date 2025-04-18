namespace OctoStore.Models;

public class ProgressiveWebAppPackage
{
    public required Uri Url { get; set; }
    public Uri? ManifestUrl { get; set; }
    public Uri? ServiceWorkerUrl { get; set; }
}
