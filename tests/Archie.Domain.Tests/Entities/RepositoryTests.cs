using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.Entities;

public class RepositoryTests
{
    [Fact]
    public void Constructor_ValidInputs_CreatesRepository()
    {
        // Arrange
        var name = "TestRepo";
        var url = "https://github.com/user/repo.git";
        var language = "C#";
        var description = "Test repository";

        // Act
        var repository = new Repository(name, url, language, description);

        // Assert
        Assert.Equal(name, repository.Name);
        Assert.Equal(url, repository.Url);
        Assert.Equal(language, repository.Language);
        Assert.Equal(description, repository.Description);
        Assert.Equal(RepositoryStatus.Connecting, repository.Status);
        Assert.True(repository.CreatedAt <= DateTime.UtcNow);
        Assert.True(repository.UpdatedAt <= DateTime.UtcNow);
        Assert.NotEqual(Guid.Empty, repository.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var url = "https://github.com/user/repo.git";
        var language = "C#";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Repository(invalidName, url, language));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("invalid-url")]
    public void Constructor_InvalidUrl_ThrowsArgumentException(string invalidUrl)
    {
        // Arrange
        var name = "TestRepo";
        var language = "C#";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Repository(name, invalidUrl, language));
    }

    [Fact]
    public void UpdateStatus_ValidTransition_UpdatesStatus()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        var originalUpdatedAt = repository.UpdatedAt;

        // Act
        Thread.Sleep(1); // Ensure time difference
        repository.UpdateStatus(RepositoryStatus.Connected);

        // Assert
        Assert.Equal(RepositoryStatus.Connected, repository.Status);
        Assert.True(repository.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void UpdateStatus_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        repository.UpdateStatus(RepositoryStatus.Connected);
        repository.UpdateStatus(RepositoryStatus.Analyzing);
        repository.UpdateStatus(RepositoryStatus.Ready);

        // Act & Assert - Cannot go from Ready back to Connecting
        Assert.Throws<InvalidOperationException>(() => repository.UpdateStatus(RepositoryStatus.Connecting));
    }

    [Fact]
    public void AddBranch_ValidBranch_AddsBranch()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        var branch = new Branch("main", true, repository.Id);

        // Act
        repository.AddBranch(branch);

        // Assert
        Assert.Single(repository.Branches);
        Assert.Equal(branch, repository.Branches.First());
    }

    [Fact]
    public void AddBranch_DuplicateBranchName_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        var branch1 = new Branch("main", true, repository.Id);
        var branch2 = new Branch("main", false, repository.Id);
        repository.AddBranch(branch1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => repository.AddBranch(branch2));
    }

    [Fact]
    public void AddBranch_WrongRepositoryId_ThrowsArgumentException()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        var branch = new Branch("main", true, Guid.NewGuid()); // Wrong repository ID

        // Act & Assert
        Assert.Throws<ArgumentException>(() => repository.AddBranch(branch));
    }

    [Fact]
    public void UpdateStatistics_ValidStatistics_UpdatesStatistics()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        var languageBreakdown = new Dictionary<string, LanguageStats>
        {
            { "C#", new LanguageStats("C#", 10, 1000, 80.0) },
            { "JavaScript", new LanguageStats("JavaScript", 5, 250, 20.0) }
        };
        var statistics = new RepositoryStatistics(15, 1250, languageBreakdown);
        var originalUpdatedAt = repository.UpdatedAt;

        // Act
        Thread.Sleep(1); // Ensure time difference
        repository.UpdateStatistics(statistics);

        // Assert
        Assert.Equal(statistics, repository.Statistics);
        Assert.True(repository.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void GetDefaultBranch_HasDefaultBranch_ReturnsDefaultBranch()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        var defaultBranch = new Branch("main", true, repository.Id);
        var regularBranch = new Branch("feature", false, repository.Id);
        repository.AddBranch(defaultBranch);
        repository.AddBranch(regularBranch);

        // Act
        var result = repository.GetDefaultBranch();

        // Assert
        Assert.Equal(defaultBranch, result);
    }

    [Fact]
    public void IsReady_StatusReady_ReturnsTrue()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        repository.UpdateStatus(RepositoryStatus.Connected);
        repository.UpdateStatus(RepositoryStatus.Analyzing);
        repository.UpdateStatus(RepositoryStatus.Ready);

        // Act & Assert
        Assert.True(repository.IsReady());
    }

    [Fact]
    public void IsConnected_ConnectedStatus_ReturnsTrue()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        repository.UpdateStatus(RepositoryStatus.Connected);

        // Act & Assert
        Assert.True(repository.IsConnected());
    }

    [Fact]
    public void HasError_ErrorStatus_ReturnsTrue()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "C#");
        repository.UpdateStatus(RepositoryStatus.Error);

        // Act & Assert
        Assert.True(repository.HasError());
    }
}