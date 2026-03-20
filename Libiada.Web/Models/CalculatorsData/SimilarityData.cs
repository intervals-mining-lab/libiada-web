namespace Libiada.Web.Models.CalculatorsData;

/// <summary>
/// Represents similarity metrics between two sequences.
/// </summary>
public record SimilarityData
{
    /// <summary>
    /// Gets or sets the first similarity formula result.
    /// </summary>
    public double Formula1 { get; init; }

    /// <summary>
    /// Gets or sets the second similarity formula result.
    /// </summary>
    public double Formula2 { get; init; }

    /// <summary>
    /// Gets or sets the third similarity formula result.
    /// </summary>
    public double Formula3 { get; init; }

    /// <summary>
    /// Gets or sets the count of absolutely equal subsequences in the first sequence.
    /// </summary>
    public int FirstAbsolutelyEqualElementsCount { get; init; }

    /// <summary>
    /// Gets or sets the count of nearly equal subsequences in the first sequence.
    /// </summary>
    public int FirstNearlyEqualElementsCount { get; init; }

    /// <summary>
    /// Gets or sets the count of not equal subsequences in the first sequence.
    /// </summary>
    public int FirstNotEqualElementsCount { get; init; }

    /// <summary>
    /// Gets or sets the total count subsequences in the first sequence.
    /// </summary>
    public int FirstTotalElementsCount { get; init; }

    /// <summary>
    /// Gets or sets the count of absolutely equal subsequences in the second sequence.
    /// </summary>
    public int SecondAbsolutelyEqualElementsCount { get; init; }

    /// <summary>
    /// Gets or sets the count of nearly equal subsequences in the second sequence.
    /// </summary>
    public int SecondNearlyEqualElementsCount { get; init; }

    /// <summary>
    /// Gets or sets the count of not equal subsequences in the second sequence.
    /// </summary>
    public int SecondNotEqualElementsCount { get; init; }

    /// <summary>
    /// Gets or sets the total count subsequences in the second sequence.
    /// </summary>
    public int SecondTotalElementsCount { get; init; }
}
