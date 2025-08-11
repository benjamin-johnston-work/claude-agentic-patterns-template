using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.ValueObjects;

[TestFixture]
public class RepositoryStatisticsTests
{
    [Test]
    public void Constructor_ValidInputs_CreatesRepositoryStatistics()
    {
        // Arrange
        var fileCount = 100;
        var lineCount = 5000;
        var languageBreakdown = new Dictionary<string, LanguageStats>
        {
            { "C#", new LanguageStats("C#", 80, 4000, 80.0) },
            { "JavaScript", new LanguageStats("JavaScript", 20, 1000, 20.0) }
        };

        // Act
        var statistics = new RepositoryStatistics(fileCount, lineCount, languageBreakdown);

        // Assert
        Assert.That(statistics.FileCount, Is.EqualTo(fileCount));
        Assert.That(statistics.LineCount, Is.EqualTo(lineCount));
        Assert.That(statistics.LanguageBreakdown, Is.EqualTo(languageBreakdown));
    }

    [Test]
    public void Constructor_NegativeFileCount_ThrowsArgumentException()
    {
        // Arrange
        var lineCount = 5000;
        var languageBreakdown = new Dictionary<string, LanguageStats>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RepositoryStatistics(-1, lineCount, languageBreakdown));
    }

    [Test]
    public void Constructor_NegativeLineCount_ThrowsArgumentException()
    {
        // Arrange
        var fileCount = 100;
        var languageBreakdown = new Dictionary<string, LanguageStats>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RepositoryStatistics(fileCount, -1, languageBreakdown));
    }

    [Test]
    public void Constructor_NullLanguageBreakdown_CreatesEmptyBreakdown()
    {
        // Act
        var statistics = new RepositoryStatistics(100, 5000, null!);

        // Assert
        Assert.That(statistics.LanguageBreakdown, Is.Not.Null);
        Assert.That(statistics.LanguageBreakdown, Is.Empty);
    }

    [Test]
    public void Empty_ReturnsEmptyStatistics()
    {
        // Act
        var statistics = RepositoryStatistics.Empty;

        // Assert
        Assert.That(statistics.FileCount, Is.EqualTo(0));
        Assert.That(statistics.LineCount, Is.EqualTo(0));
        Assert.That(statistics.LanguageBreakdown, Is.Empty);
    }
}

[TestFixture]
public class LanguageStatsTests
{
    [Test]
    public void Constructor_ValidInputs_CreatesLanguageStats()
    {
        // Arrange
        var language = "C#";
        var fileCount = 50;
        var lineCount = 2500;
        var percentage = 75.5;

        // Act
        var stats = new LanguageStats(language, fileCount, lineCount, percentage);

        // Assert
        Assert.That(stats.Language, Is.EqualTo(language));
        Assert.That(stats.FileCount, Is.EqualTo(fileCount));
        Assert.That(stats.LineCount, Is.EqualTo(lineCount));
        Assert.That(stats.Percentage, Is.EqualTo(percentage));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidLanguage_ThrowsArgumentException(string invalidLanguage)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LanguageStats(invalidLanguage, 10, 100, 50.0));
    }

    [Test]
    public void Constructor_NegativeFileCount_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LanguageStats("C#", -1, 100, 50.0));
    }

    [Test]
    public void Constructor_NegativeLineCount_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LanguageStats("C#", 10, -1, 50.0));
    }

    [TestCase(-1.0)]
    [TestCase(101.0)]
    public void Constructor_InvalidPercentage_ThrowsArgumentException(double invalidPercentage)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LanguageStats("C#", 10, 100, invalidPercentage));
    }
}