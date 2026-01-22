namespace VodDashboard.Api.Domain;

public static class Validation
{
    /// <summary>
    /// Validates a job ID to prevent directory traversal attacks.
    /// </summary>
    /// <param name="id">The job ID to validate.</param>
    /// <returns>True if the ID is valid; otherwise, false.</returns>
    public static bool IsValidJobId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        // Validate id to prevent directory traversal attacks
        if (id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || id.Contains(Path.DirectorySeparatorChar)
            || id.Contains(Path.AltDirectorySeparatorChar)
            || id.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sanitizes a job ID by extracting only the filename component.
    /// This should only be called after validating the ID with IsValidJobId.
    /// </summary>
    /// <param name="id">The job ID to sanitize.</param>
    /// <returns>The sanitized job ID.</returns>
    public static string SanitizeJobId(string id)
    {
        return Path.GetFileName(id);
    }
}
