using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.Entities;

[TestFixture]
public class DocumentationTests
{
    private DocumentationMetadata _validMetadata;
    private Guid _repositoryId;

    [SetUp]
    public void SetUp()
    {
        _repositoryId = Guid.NewGuid();
        _validMetadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
    }

    [Test]
    public void Create_ValidInputs_CreatesDocumentation()
    {
        // Arrange
        var title = "Test Documentation";

        // Act
        var documentation = Documentation.Create(_repositoryId, title, _validMetadata);

        // Assert
        Assert.That(documentation.RepositoryId, Is.EqualTo(_repositoryId));
        Assert.That(documentation.Title, Is.EqualTo(title));
        Assert.That(documentation.Metadata, Is.EqualTo(_validMetadata));
        Assert.That(documentation.Status, Is.EqualTo(DocumentationStatus.NotStarted));
        Assert.That(documentation.Version, Is.EqualTo("1.0.0"));
        Assert.That(documentation.GeneratedAt <= DateTime.UtcNow, Is.True);
        Assert.That(documentation.LastUpdatedAt <= DateTime.UtcNow, Is.True);
        Assert.That(documentation.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(documentation.Sections.Count, Is.EqualTo(0));
    }

    [Test]
    public void Create_EmptyRepositoryId_ThrowsArgumentException()
    {
        // Arrange
        var title = "Test Documentation";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Documentation.Create(Guid.Empty, title, _validMetadata));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Create_InvalidTitle_ThrowsArgumentException(string invalidTitle)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Documentation.Create(_repositoryId, invalidTitle, _validMetadata));
    }

    [Test]
    public void Create_NullMetadata_ThrowsArgumentNullException()
    {
        // Arrange
        var title = "Test Documentation";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Documentation.Create(_repositoryId, title, null!));
    }

    [Test]
    public void UpdateStatus_ValidTransition_UpdatesStatus()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var originalUpdatedAt = documentation.LastUpdatedAt;

        // Act
        Thread.Sleep(1); // Ensure time difference
        documentation.UpdateStatus(DocumentationStatus.Analyzing);

        // Assert
        Assert.That(documentation.Status, Is.EqualTo(DocumentationStatus.Analyzing));
        Assert.That(documentation.LastUpdatedAt > originalUpdatedAt, Is.True);
    }

    [Test]
    public void UpdateStatus_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.UpdateStatus(DocumentationStatus.Completed);

        // Act & Assert - Cannot go from Completed back to Analyzing without going through UpdateRequired
        Assert.Throws<InvalidOperationException>(() => documentation.UpdateStatus(DocumentationStatus.Analyzing));
    }

    [Test]
    public void UpdateStatus_SameStatus_DoesNotUpdateTimestamp()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.UpdateStatus(DocumentationStatus.Analyzing);
        var timestampAfterFirstUpdate = documentation.LastUpdatedAt;

        // Act
        documentation.UpdateStatus(DocumentationStatus.Analyzing);

        // Assert
        Assert.That(documentation.LastUpdatedAt, Is.EqualTo(timestampAfterFirstUpdate));
    }

    [Test]
    public void AddSection_ValidSection_AddsSection()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var section = new DocumentationSection("Overview", "This is an overview", DocumentationSectionType.Overview, 1);

        // Act
        documentation.AddSection(section);

        // Assert
        Assert.That(documentation.Sections.Count, Is.EqualTo(1));
        Assert.That(documentation.Sections.First(), Is.EqualTo(section));
    }

    [Test]
    public void AddSection_NullSection_ThrowsArgumentNullException()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => documentation.AddSection(null!));
    }

    [Test]
    public void AddSection_DuplicateUniqueSection_ThrowsInvalidOperationException()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var section1 = new DocumentationSection("Overview", "First overview", DocumentationSectionType.Overview, 1);
        var section2 = new DocumentationSection("Overview 2", "Second overview", DocumentationSectionType.Overview, 2);
        documentation.AddSection(section1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => documentation.AddSection(section2));
    }

    [Test]
    public void UpdateSection_ExistingSection_UpdatesContent()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var section = new DocumentationSection("Overview", "Original content", DocumentationSectionType.Overview, 1);
        documentation.AddSection(section);
        var originalUpdatedAt = documentation.LastUpdatedAt;
        var newContent = "Updated content";

        // Act
        Thread.Sleep(1); // Ensure time difference
        documentation.UpdateSection(section.Id, newContent);

        // Assert
        var updatedSection = documentation.GetSection(section.Id);
        Assert.That(updatedSection!.Content, Is.EqualTo(newContent));
        Assert.That(documentation.LastUpdatedAt > originalUpdatedAt, Is.True);
    }

    [Test]
    public void UpdateSection_NonExistentSection_ThrowsInvalidOperationException()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var nonExistentSectionId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => documentation.UpdateSection(nonExistentSectionId, "New content"));
    }

    [Test]
    public void RemoveSection_ExistingSection_RemovesSection()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var section = new DocumentationSection("Overview", "Content", DocumentationSectionType.Overview, 1);
        documentation.AddSection(section);

        // Act
        documentation.RemoveSection(section.Id);

        // Assert
        Assert.That(documentation.Sections.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveSection_NonExistentSection_DoesNothing()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var section = new DocumentationSection("Overview", "Content", DocumentationSectionType.Overview, 1);
        documentation.AddSection(section);
        var nonExistentSectionId = Guid.NewGuid();

        // Act
        documentation.RemoveSection(nonExistentSectionId);

        // Assert
        Assert.That(documentation.Sections.Count, Is.EqualTo(1));
    }

    [Test]
    public void MarkAsCompleted_ValidDocumentation_UpdatesStatusAndVersion()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var originalVersion = documentation.Version;

        // Act
        documentation.MarkAsCompleted();

        // Assert
        Assert.That(documentation.Status, Is.EqualTo(DocumentationStatus.Completed));
        Assert.That(documentation.Version, Is.Not.EqualTo(originalVersion));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void MarkAsFailed_InvalidErrorMessage_ThrowsArgumentException(string invalidErrorMessage)
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => documentation.MarkAsFailed(invalidErrorMessage));
    }

    [Test]
    public void MarkAsFailed_ValidErrorMessage_UpdatesStatusAndErrorMessage()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var errorMessage = "Generation failed";

        // Act
        documentation.MarkAsFailed(errorMessage);

        // Assert
        Assert.That(documentation.Status, Is.EqualTo(DocumentationStatus.Error));
        Assert.That(documentation.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Test]
    public void RequiresRegeneration_RepositoryModifiedAfterDocumentation_ReturnsTrue()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var repositoryLastModified = documentation.LastUpdatedAt.AddHours(1);

        // Act
        var result = documentation.RequiresRegeneration(repositoryLastModified);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void RequiresRegeneration_RepositoryModifiedBeforeDocumentation_ReturnsFalse()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.MarkAsCompleted();
        var repositoryLastModified = documentation.LastUpdatedAt.AddHours(-1);

        // Act
        var result = documentation.RequiresRegeneration(repositoryLastModified);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void RequiresRegeneration_StatusUpdateRequired_ReturnsTrue()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.MarkAsCompleted();
        documentation.MarkForRegeneration();
        var repositoryLastModified = documentation.LastUpdatedAt.AddHours(-1);

        // Act
        var result = documentation.RequiresRegeneration(repositoryLastModified);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void RequiresRegeneration_StatusError_ReturnsTrue()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.MarkAsFailed("Test error");
        var repositoryLastModified = documentation.LastUpdatedAt.AddHours(-1);

        // Act
        var result = documentation.RequiresRegeneration(repositoryLastModified);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void GetSection_ExistingSectionByType_ReturnsSection()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var section = new DocumentationSection("Overview", "Content", DocumentationSectionType.Overview, 1);
        documentation.AddSection(section);

        // Act
        var result = documentation.GetSection(DocumentationSectionType.Overview);

        // Assert
        Assert.That(result, Is.EqualTo(section));
    }

    [Test]
    public void GetSection_NonExistentSectionByType_ReturnsNull()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);

        // Act
        var result = documentation.GetSection(DocumentationSectionType.Overview);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetSection_ExistingSectionById_ReturnsSection()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var section = new DocumentationSection("Overview", "Content", DocumentationSectionType.Overview, 1);
        documentation.AddSection(section);

        // Act
        var result = documentation.GetSection(section.Id);

        // Assert
        Assert.That(result, Is.EqualTo(section));
    }

    [Test]
    public void GetSectionsByTag_ExistingTag_ReturnsSectionsWithTag()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        var section1 = new DocumentationSection("Overview", "Content", DocumentationSectionType.Overview, 1);
        var section2 = new DocumentationSection("Usage", "Content", DocumentationSectionType.Usage, 2);
        section1.AddTag("csharp");
        section2.AddTag("csharp");
        section2.AddTag("api");
        documentation.AddSection(section1);
        documentation.AddSection(section2);

        // Act
        var result = documentation.GetSectionsByTag("csharp");

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(section1), Is.True);
        Assert.That(result.Contains(section2), Is.True);
    }

    [Test]
    public void IsInProgress_StatusGeneratingContent_ReturnsTrue()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.UpdateStatus(DocumentationStatus.GeneratingContent);

        // Act & Assert
        Assert.That(documentation.IsInProgress(), Is.True);
    }

    [Test]
    public void IsCompleted_StatusCompleted_ReturnsTrue()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.MarkAsCompleted();

        // Act & Assert
        Assert.That(documentation.IsCompleted(), Is.True);
    }

    [Test]
    public void HasError_StatusError_ReturnsTrue()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.MarkAsFailed("Test error");

        // Act & Assert
        Assert.That(documentation.HasError(), Is.True);
    }

    [Test]
    public void IsReady_StatusCompleted_ReturnsTrue()
    {
        // Arrange
        var documentation = Documentation.Create(_repositoryId, "Test Documentation", _validMetadata);
        documentation.MarkAsCompleted();

        // Act & Assert
        Assert.That(documentation.IsReady(), Is.True);
    }
}