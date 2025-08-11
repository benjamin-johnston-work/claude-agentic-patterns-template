using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.Entities;

[TestFixture]
public class RepositoryTests
{
    [Test]
    public void Constructor_ValidInputs_CreatesRepository()
    {
        // Arrange
        var name = "TestRepo";
        var url = "https://github.com/user/repo.git";
        var owner = "user";
        var language = "C#";
        var description = "Test repository";

        // Act
        var repository = new Repository(name, url, owner, language, description);

        // Assert
        Assert.That(repository.Name, Is.EqualTo(name));
        Assert.That(repository.Url, Is.EqualTo(url));
        Assert.That(repository.Owner, Is.EqualTo(owner));
        Assert.That(repository.FullName, Is.EqualTo($"{owner}/{name}"));
        Assert.That(repository.CloneUrl, Is.EqualTo(url)); // Since URL already ends with .git
        Assert.That(repository.Language, Is.EqualTo(language));
        Assert.That(repository.Description, Is.EqualTo(description));
        Assert.That(repository.DefaultBranch, Is.EqualTo("main"));
        Assert.That(repository.Status, Is.EqualTo(RepositoryStatus.Connecting));
        Assert.That(repository.CreatedAt <= DateTime.UtcNow, Is.True);
        Assert.That(repository.UpdatedAt <= DateTime.UtcNow, Is.True);
        Assert.That(repository.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var url = "https://github.com/user/repo.git";
        var owner = "user";
        var language = "C#";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Repository(invalidName, url, owner, language));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    [TestCase("invalid-url")]
    public void Constructor_InvalidUrl_ThrowsArgumentException(string invalidUrl)
    {
        // Arrange
        var name = "TestRepo";
        var owner = "user";
        var language = "C#";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Repository(name, invalidUrl, owner, language));
    }

    [Test]
    public void UpdateStatus_ValidTransition_UpdatesStatus()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        var originalUpdatedAt = repository.UpdatedAt;

        // Act
        Thread.Sleep(1); // Ensure time difference
        repository.UpdateStatus(RepositoryStatus.Connected);

        // Assert
        Assert.That(repository.Status, Is.EqualTo(RepositoryStatus.Connected));
        Assert.That(repository.UpdatedAt > originalUpdatedAt, Is.True);
    }

    [Test]
    public void UpdateStatus_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        repository.UpdateStatus(RepositoryStatus.Connected);
        repository.UpdateStatus(RepositoryStatus.Analyzing);
        repository.UpdateStatus(RepositoryStatus.Ready);

        // Act & Assert - Cannot go from Ready back to Connecting
        Assert.Throws<InvalidOperationException>(() => repository.UpdateStatus(RepositoryStatus.Connecting));
    }

    [Test]
    public void AddBranch_ValidBranch_AddsBranch()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        var branch = new Branch("main", true, repository.Id);

        // Act
        repository.AddBranch(branch);

        // Assert
        Assert.That(repository.Branches.Count(), Is.EqualTo(1));
        Assert.That(repository.Branches.First(), Is.EqualTo(branch));
    }

    [Test]
    public void AddBranch_DuplicateBranchName_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        var branch1 = new Branch("main", true, repository.Id);
        var branch2 = new Branch("main", false, repository.Id);
        repository.AddBranch(branch1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => repository.AddBranch(branch2));
    }

    [Test]
    public void AddBranch_WrongRepositoryId_ThrowsArgumentException()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        var branch = new Branch("main", true, Guid.NewGuid()); // Wrong repository ID

        // Act & Assert
        Assert.Throws<ArgumentException>(() => repository.AddBranch(branch));
    }

    [Test]
    public void UpdateStatistics_ValidStatistics_UpdatesStatistics()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
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
        Assert.That(repository.Statistics, Is.EqualTo(statistics));
        Assert.That(repository.UpdatedAt > originalUpdatedAt, Is.True);
    }

    [Test]
    public void GetDefaultBranch_HasDefaultBranch_ReturnsDefaultBranch()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        var defaultBranch = new Branch("main", true, repository.Id);
        var regularBranch = new Branch("feature", false, repository.Id);
        repository.AddBranch(defaultBranch);
        repository.AddBranch(regularBranch);

        // Act
        var result = repository.GetDefaultBranch();

        // Assert
        Assert.That(result, Is.EqualTo(defaultBranch));
    }

    [Test]
    public void IsReady_StatusReady_ReturnsTrue()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        repository.UpdateStatus(RepositoryStatus.Connected);
        repository.UpdateStatus(RepositoryStatus.Analyzing);
        repository.UpdateStatus(RepositoryStatus.Ready);

        // Act & Assert
        Assert.That(repository.IsReady(), Is.True);
    }

    [Test]
    public void IsConnected_ConnectedStatus_ReturnsTrue()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        repository.UpdateStatus(RepositoryStatus.Connected);

        // Act & Assert
        Assert.That(repository.IsConnected(), Is.True);
    }

    [Test]
    public void HasError_ErrorStatus_ReturnsTrue()
    {
        // Arrange
        var repository = new Repository("TestRepo", "https://github.com/user/repo.git", "user", "C#");
        repository.UpdateStatus(RepositoryStatus.Error);

        // Act & Assert
        Assert.That(repository.HasError(), Is.True);
    }
}