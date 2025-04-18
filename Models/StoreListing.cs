namespace OctoStore.Models;

public class StoreListing
{
    public required string Language { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required List<Screenshot> Screenshots { get; set; }

    public string? WhatsNew { get; set; }
    public List<string> Features { get; set; } = [];
    public List<Screenshot> Trailers { get; set; } = [];
    public string? ShortName { get; set; }
    public string? VoiceTitle { get; set; }
    public string? ShortDescription { get; set; }
    public List<string> Keywords { get; set; } = [];
    public string? CopyrightAndTrademarkInfo { get; set; }
    public string? AdditionalLicenseTerms { get; set; }
    public string? DevelopedBy { get; set; }
    public string? PublishedBy { get; set; }
}
