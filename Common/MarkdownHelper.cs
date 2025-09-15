using Markdig;
using Microsoft.AspNetCore.Html;

namespace OctoStore.Common
{
    /// <summary>
    /// Helper class for rendering markdown to HTML.
    /// </summary>
    public static class MarkdownHelper
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml()  // Disable raw HTML for security
            .Build();

        /// <summary>
        /// Converts markdown text to safe HTML.
        /// </summary>
        /// <param name="markdown">The markdown text to convert.</param>
        /// <returns>HTML markup wrapped in an HtmlString for safe rendering in Razor views.</returns>
        public static HtmlString ToHtml(string? markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return new HtmlString(string.Empty);
            }

            var html = Markdown.ToHtml(markdown, Pipeline);
            return new HtmlString(html);
        }
    }
}