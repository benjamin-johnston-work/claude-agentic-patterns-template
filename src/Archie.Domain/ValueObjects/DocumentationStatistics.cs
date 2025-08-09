namespace Archie.Domain.ValueObjects;

public class DocumentationStatistics
{
    public int TotalSections { get; private set; }
    public int CodeReferences { get; private set; }
    public int WordCount { get; private set; }
    public TimeSpan GenerationTime { get; private set; }
    public double AccuracyScore { get; private set; }
    public List<string> CoveredTopics { get; private set; }

    public static DocumentationStatistics Empty => new();

    protected DocumentationStatistics() // EF Constructor
    {
        TotalSections = 0;
        CodeReferences = 0;
        WordCount = 0;
        GenerationTime = TimeSpan.Zero;
        AccuracyScore = 0.0;
        CoveredTopics = new List<string>();
    }

    public DocumentationStatistics(
        int totalSections, 
        int codeReferences, 
        int wordCount, 
        TimeSpan generationTime, 
        double accuracyScore,
        List<string> coveredTopics)
    {
        if (totalSections < 0)
            throw new ArgumentOutOfRangeException(nameof(totalSections), "Total sections cannot be negative");
        
        if (codeReferences < 0)
            throw new ArgumentOutOfRangeException(nameof(codeReferences), "Code references cannot be negative");
        
        if (wordCount < 0)
            throw new ArgumentOutOfRangeException(nameof(wordCount), "Word count cannot be negative");
        
        if (generationTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(generationTime), "Generation time cannot be negative");
        
        if (accuracyScore < 0.0 || accuracyScore > 1.0)
            throw new ArgumentOutOfRangeException(nameof(accuracyScore), "Accuracy score must be between 0.0 and 1.0");

        TotalSections = totalSections;
        CodeReferences = codeReferences;
        WordCount = wordCount;
        GenerationTime = generationTime;
        AccuracyScore = accuracyScore;
        CoveredTopics = coveredTopics ?? new List<string>();
    }

    public DocumentationStatistics WithUpdatedSections(int totalSections)
    {
        return new DocumentationStatistics(totalSections, CodeReferences, WordCount, GenerationTime, AccuracyScore, CoveredTopics);
    }

    public DocumentationStatistics WithUpdatedCodeReferences(int codeReferences)
    {
        return new DocumentationStatistics(TotalSections, codeReferences, WordCount, GenerationTime, AccuracyScore, CoveredTopics);
    }

    public DocumentationStatistics WithUpdatedWordCount(int wordCount)
    {
        return new DocumentationStatistics(TotalSections, CodeReferences, wordCount, GenerationTime, AccuracyScore, CoveredTopics);
    }

    public DocumentationStatistics WithUpdatedGenerationTime(TimeSpan generationTime)
    {
        return new DocumentationStatistics(TotalSections, CodeReferences, WordCount, generationTime, AccuracyScore, CoveredTopics);
    }

    public DocumentationStatistics WithUpdatedAccuracyScore(double accuracyScore)
    {
        return new DocumentationStatistics(TotalSections, CodeReferences, WordCount, GenerationTime, accuracyScore, CoveredTopics);
    }

    public DocumentationStatistics WithUpdatedCoveredTopics(List<string> coveredTopics)
    {
        return new DocumentationStatistics(TotalSections, CodeReferences, WordCount, GenerationTime, AccuracyScore, coveredTopics);
    }
}