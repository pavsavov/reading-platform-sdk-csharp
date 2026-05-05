namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents the input required to upload or replace book content.
/// </summary>
public sealed class UploadBookContentRequest
{
    /// <summary>
    /// Gets or sets the content stream.
    /// </summary>
    public Stream File { get; set; } = Stream.Null;

    /// <summary>
    /// Gets or sets the uploaded file name sent to the API.
    /// </summary>
    public string FileName { get; set; } = "content";

    /// <summary>
    /// Gets or sets the optional media content type (for example application/pdf).
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the optional content format (for example pdf or epub).
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets an optional idempotency key used to safely retry uploads.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
