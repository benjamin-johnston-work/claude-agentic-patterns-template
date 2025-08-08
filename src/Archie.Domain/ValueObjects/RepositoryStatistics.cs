namespace Archie.Domain.ValueObjects;

public record RepositoryStatistics
{
    public int FileCount { get; }
    public int LineCount { get; }
    public IReadOnlyDictionary<string, LanguageStats> LanguageBreakdown { get; }

    public RepositoryStatistics(
        int fileCount,
        int lineCount,
        IReadOnlyDictionary<string, LanguageStats> languageBreakdown)
    {
        if (fileCount < 0)
            throw new ArgumentException("File count cannot be negative", nameof(fileCount));
        
        if (lineCount < 0)
            throw new ArgumentException("Line count cannot be negative", nameof(lineCount));

        FileCount = fileCount;
        LineCount = lineCount;
        LanguageBreakdown = languageBreakdown ?? new Dictionary<string, LanguageStats>();
    }

    public static RepositoryStatistics Empty => new(0, 0, new Dictionary<string, LanguageStats>());
}

public record LanguageStats
{
    public string Language { get; }
    public int FileCount { get; }
    public int LineCount { get; }
    public double Percentage { get; }

    public LanguageStats(string language, int fileCount, int lineCount, double percentage)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));
        
        if (fileCount < 0)
            throw new ArgumentException("File count cannot be negative", nameof(fileCount));
        
        if (lineCount < 0)
            throw new ArgumentException("Line count cannot be negative", nameof(lineCount));
        
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Percentage must be between 0 and 100", nameof(percentage));

        Language = language;
        FileCount = fileCount;
        LineCount = lineCount;
        Percentage = percentage;
    }
}