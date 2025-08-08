using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.ValueObjects;

public class RepositoryStatisticsTests
{
    [Fact]
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
        Assert.Equal(fileCount, statistics.FileCount);
        Assert.Equal(lineCount, statistics.LineCount);
        Assert.Equal(languageBreakdown, statistics.LanguageBreakdown);
    }

    [Fact]
    public void Constructor_NegativeFileCount_ThrowsArgumentException()
    {
        // Arrange
        var lineCount = 5000;
        var languageBreakdown = new Dictionary<string, LanguageStats>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RepositoryStatistics(-1, lineCount, languageBreakdown));
    }

    [Fact]
    public void Constructor_NegativeLineCount_ThrowsArgumentException()
    {
        // Arrange
        var fileCount = 100;
        var languageBreakdown = new Dictionary<string, LanguageStats>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RepositoryStatistics(fileCount, -1, languageBreakdown));
    }

    [Fact]
    public void Constructor_NullLanguageBreakdown_CreatesEmptyBreakdown()
    {
        // Act
        var statistics = new RepositoryStatistics(100, 5000, null!);

        // Assert
        Assert.NotNull(statistics.LanguageBreakdown);
        Assert.Empty(statistics.LanguageBreakdown);
    }

    [Fact]
    public void Empty_ReturnsEmptyStatistics()
    {
        // Act
        var statistics = RepositoryStatistics.Empty;

        // Assert
        Assert.Equal(0, statistics.FileCount);
        Assert.Equal(0, statistics.LineCount);
        Assert.Empty(statistics.LanguageBreakdown);
    }
}

public class LanguageStatsTests
{
    [Fact]
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
        Assert.Equal(language, stats.Language);
        Assert.Equal(fileCount, stats.FileCount);
        Assert.Equal(lineCount, stats.LineCount);
        Assert.Equal(percentage, stats.Percentage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_InvalidLanguage_ThrowsArgumentException(string invalidLanguage)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LanguageStats(invalidLanguage, 10, 100, 50.0));
    }

    [Fact]
    public void Constructor_NegativeFileCount_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LanguageStats("C#", -1, 100, 50.0));
    }

    [Fact]
    public void Constructor_NegativeLineCount_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LanguageStats("C#", 10, -1, 50.0));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(101.0)]
    public void Constructor_InvalidPercentage_ThrowsArgumentException(double invalidPercentage)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LanguageStats("C#", 10, 100, invalidPercentage));
    }
}