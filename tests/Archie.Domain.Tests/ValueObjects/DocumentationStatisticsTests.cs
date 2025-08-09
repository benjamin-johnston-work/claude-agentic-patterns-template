using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.ValueObjects;

[TestFixture]
public class DocumentationStatisticsTests
{
    [Test]
    public void Constructor_ValidInputs_CreatesStatistics()
    {
        // Arrange
        var totalSections = 5;
        var codeReferences = 10;
        var wordCount = 1500;
        var generationTime = TimeSpan.FromMinutes(2);
        var accuracyScore = 0.85;
        var coveredTopics = new List<string> { "overview", "usage", "api" };

        // Act
        var statistics = new DocumentationStatistics(totalSections, codeReferences, wordCount, generationTime, accuracyScore, coveredTopics);

        // Assert
        Assert.That(statistics.TotalSections, Is.EqualTo(totalSections));
        Assert.That(statistics.CodeReferences, Is.EqualTo(codeReferences));
        Assert.That(statistics.WordCount, Is.EqualTo(wordCount));
        Assert.That(statistics.GenerationTime, Is.EqualTo(generationTime));
        Assert.That(statistics.AccuracyScore, Is.EqualTo(accuracyScore));
        Assert.That(statistics.CoveredTopics.Count, Is.EqualTo(3));
        Assert.That(statistics.CoveredTopics, Is.EqualTo(coveredTopics));
    }

    [Test]
    public void Constructor_NullCoveredTopics_CreatesEmptyList()
    {
        // Arrange
        var totalSections = 5;
        var codeReferences = 10;
        var wordCount = 1500;
        var generationTime = TimeSpan.FromMinutes(2);
        var accuracyScore = 0.85;

        // Act
        var statistics = new DocumentationStatistics(totalSections, codeReferences, wordCount, generationTime, accuracyScore, null);

        // Assert
        Assert.That(statistics.CoveredTopics, Is.Not.Null);
        Assert.That(statistics.CoveredTopics.Count, Is.EqualTo(0));
    }

    [TestCase(-1)]
    [TestCase(-10)]
    public void Constructor_NegativeTotalSections_ThrowsArgumentOutOfRangeException(int negativeSections)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new DocumentationStatistics(negativeSections, 10, 1500, TimeSpan.FromMinutes(2), 0.85, null));
    }

    [TestCase(-1)]
    [TestCase(-5)]
    public void Constructor_NegativeCodeReferences_ThrowsArgumentOutOfRangeException(int negativeReferences)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new DocumentationStatistics(5, negativeReferences, 1500, TimeSpan.FromMinutes(2), 0.85, null));
    }

    [TestCase(-1)]
    [TestCase(-100)]
    public void Constructor_NegativeWordCount_ThrowsArgumentOutOfRangeException(int negativeWordCount)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new DocumentationStatistics(5, 10, negativeWordCount, TimeSpan.FromMinutes(2), 0.85, null));
    }

    [Test]
    public void Constructor_NegativeGenerationTime_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var negativeTime = TimeSpan.FromMinutes(-1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new DocumentationStatistics(5, 10, 1500, negativeTime, 0.85, null));
    }

    [TestCase(-0.1)]
    [TestCase(1.1)]
    [TestCase(2.0)]
    public void Constructor_InvalidAccuracyScore_ThrowsArgumentOutOfRangeException(double invalidScore)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), invalidScore, null));
    }

    [TestCase(0.0)]
    [TestCase(0.5)]
    [TestCase(1.0)]
    public void Constructor_ValidAccuracyScore_AcceptsScore(double validScore)
    {
        // Act
        var statistics = new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), validScore, null);

        // Assert
        Assert.That(statistics.AccuracyScore, Is.EqualTo(validScore));
    }

    [Test]
    public void Empty_ReturnsDefaultStatistics()
    {
        // Act
        var empty = DocumentationStatistics.Empty;

        // Assert
        Assert.That(empty.TotalSections, Is.EqualTo(0));
        Assert.That(empty.CodeReferences, Is.EqualTo(0));
        Assert.That(empty.WordCount, Is.EqualTo(0));
        Assert.That(empty.GenerationTime, Is.EqualTo(TimeSpan.Zero));
        Assert.That(empty.AccuracyScore, Is.EqualTo(0.0));
        Assert.That(empty.CoveredTopics.Count, Is.EqualTo(0));
    }

    [Test]
    public void WithUpdatedSections_ValidValue_ReturnsNewInstanceWithUpdatedSections()
    {
        // Arrange
        var original = new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), 0.85, new List<string> { "test" });
        var newSectionCount = 8;

        // Act
        var updated = original.WithUpdatedSections(newSectionCount);

        // Assert
        Assert.That(updated.TotalSections, Is.EqualTo(newSectionCount));
        Assert.That(updated.CodeReferences, Is.EqualTo(original.CodeReferences));
        Assert.That(updated.WordCount, Is.EqualTo(original.WordCount));
        Assert.That(updated.GenerationTime, Is.EqualTo(original.GenerationTime));
        Assert.That(updated.AccuracyScore, Is.EqualTo(original.AccuracyScore));
        Assert.That(updated.CoveredTopics, Is.EqualTo(original.CoveredTopics));
        
        // Ensure original is unchanged
        Assert.That(original.TotalSections, Is.EqualTo(5));
    }

    [Test]
    public void WithUpdatedCodeReferences_ValidValue_ReturnsNewInstanceWithUpdatedReferences()
    {
        // Arrange
        var original = new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), 0.85, new List<string> { "test" });
        var newReferenceCount = 15;

        // Act
        var updated = original.WithUpdatedCodeReferences(newReferenceCount);

        // Assert
        Assert.That(updated.CodeReferences, Is.EqualTo(newReferenceCount));
        Assert.That(updated.TotalSections, Is.EqualTo(original.TotalSections));
        Assert.That(original.CodeReferences, Is.EqualTo(10)); // Original unchanged
    }

    [Test]
    public void WithUpdatedWordCount_ValidValue_ReturnsNewInstanceWithUpdatedWordCount()
    {
        // Arrange
        var original = new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), 0.85, new List<string> { "test" });
        var newWordCount = 2000;

        // Act
        var updated = original.WithUpdatedWordCount(newWordCount);

        // Assert
        Assert.That(updated.WordCount, Is.EqualTo(newWordCount));
        Assert.That(updated.TotalSections, Is.EqualTo(original.TotalSections));
        Assert.That(original.WordCount, Is.EqualTo(1500)); // Original unchanged
    }

    [Test]
    public void WithUpdatedGenerationTime_ValidValue_ReturnsNewInstanceWithUpdatedTime()
    {
        // Arrange
        var original = new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), 0.85, new List<string> { "test" });
        var newGenerationTime = TimeSpan.FromMinutes(5);

        // Act
        var updated = original.WithUpdatedGenerationTime(newGenerationTime);

        // Assert
        Assert.That(updated.GenerationTime, Is.EqualTo(newGenerationTime));
        Assert.That(updated.TotalSections, Is.EqualTo(original.TotalSections));
        Assert.That(original.GenerationTime, Is.EqualTo(TimeSpan.FromMinutes(2))); // Original unchanged
    }

    [Test]
    public void WithUpdatedAccuracyScore_ValidValue_ReturnsNewInstanceWithUpdatedScore()
    {
        // Arrange
        var original = new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), 0.85, new List<string> { "test" });
        var newAccuracyScore = 0.92;

        // Act
        var updated = original.WithUpdatedAccuracyScore(newAccuracyScore);

        // Assert
        Assert.That(updated.AccuracyScore, Is.EqualTo(newAccuracyScore));
        Assert.That(updated.TotalSections, Is.EqualTo(original.TotalSections));
        Assert.That(original.AccuracyScore, Is.EqualTo(0.85)); // Original unchanged
    }

    [Test]
    public void WithUpdatedCoveredTopics_ValidValue_ReturnsNewInstanceWithUpdatedTopics()
    {
        // Arrange
        var original = new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), 0.85, new List<string> { "test" });
        var newTopics = new List<string> { "overview", "usage", "configuration" };

        // Act
        var updated = original.WithUpdatedCoveredTopics(newTopics);

        // Assert
        Assert.That(updated.CoveredTopics, Is.EqualTo(newTopics));
        Assert.That(updated.TotalSections, Is.EqualTo(original.TotalSections));
        Assert.That(original.CoveredTopics.Count, Is.EqualTo(1)); // Original unchanged
    }

    [Test]
    public void ImmutabilityTest_UpdatingOneProperty_DoesNotAffectOthers()
    {
        // Arrange
        var original = new DocumentationStatistics(5, 10, 1500, TimeSpan.FromMinutes(2), 0.85, new List<string> { "test" });

        // Act
        var updated1 = original.WithUpdatedSections(10);
        var updated2 = updated1.WithUpdatedCodeReferences(20);
        var updated3 = updated2.WithUpdatedWordCount(3000);

        // Assert - Each update creates a new instance
        Assert.That(original.TotalSections, Is.EqualTo(5));
        Assert.That(updated1.TotalSections, Is.EqualTo(10));
        Assert.That(updated2.TotalSections, Is.EqualTo(10));
        Assert.That(updated3.TotalSections, Is.EqualTo(10));

        Assert.That(original.CodeReferences, Is.EqualTo(10));
        Assert.That(updated1.CodeReferences, Is.EqualTo(10));
        Assert.That(updated2.CodeReferences, Is.EqualTo(20));
        Assert.That(updated3.CodeReferences, Is.EqualTo(20));

        Assert.That(original.WordCount, Is.EqualTo(1500));
        Assert.That(updated1.WordCount, Is.EqualTo(1500));
        Assert.That(updated2.WordCount, Is.EqualTo(1500));
        Assert.That(updated3.WordCount, Is.EqualTo(3000));
    }
}