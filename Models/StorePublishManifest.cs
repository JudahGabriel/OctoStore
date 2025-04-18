namespace OctoStore.Models;

// v1 omits:
// pricing, availability, architectures, subcategory, system requirements, age rating, MSIX and exe submissions,
// custom package image overrides, submission options, notes for certification

public class StorePublishManifest
{
    public required string Name { get; set; }
    public required Uri IconUrl { get; set; }
    public required Category Category { get; set; }
    public required Uri PrivacyPolicyUrl { get; set; }
    public required List<StoreListing> StoreListings { get; set; }

    public Category? SecondaryCategory { get; set; }
    public SupportInfo? Support { get; set; }
}
