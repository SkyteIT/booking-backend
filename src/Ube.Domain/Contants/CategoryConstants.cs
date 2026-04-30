namespace Ube.Domain.Constants;

/// <summary>
/// Well-known category name constants shared across all layers.
/// Keeping this in Ube.Domain ensures Application and Infrastructure
/// can both reference it without creating an illegal layer dependency.
/// </summary>
public static class CategoryConstants
{
    /// <summary>
    /// Internal sentinel category used to hold listings whose real category
    /// was deleted.  Never shown in the UI (status = Inactive).
    /// </summary>
    public const string UncategorizedName = "__Uncategorized__";
}
