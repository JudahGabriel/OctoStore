namespace OctoStore.Models
{
    /// <summary>
    /// Represents a Partner Center feedback comment on an app submission.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// The author of the comment.
        /// </summary>
        public required string Author { get; set; }

        /// <summary>
        /// The content/text of the comment.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// The created date of the comment.
        /// </summary>
        public DateTimeOffset Created { get; set; }
    }
}