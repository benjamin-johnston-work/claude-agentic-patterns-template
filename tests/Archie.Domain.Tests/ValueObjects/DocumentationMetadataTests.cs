using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.ValueObjects;

[TestFixture]
public class DocumentationMetadataTests
{
    [Test]
    public void Constructor_ValidInputs_CreatesMetadata()
    {
        // Arrange
        var repositoryName = "TestRepo";
        var repositoryUrl = "https://github.com/user/repo";
        var primaryLanguage = "C#";
        var projectType = "Application";

        // Act
        var metadata = new DocumentationMetadata(repositoryName, repositoryUrl, primaryLanguage, projectType);

        // Assert
        Assert.That(metadata.RepositoryName, Is.EqualTo(repositoryName));
        Assert.That(metadata.RepositoryUrl, Is.EqualTo(repositoryUrl));
        Assert.That(metadata.PrimaryLanguage, Is.EqualTo(primaryLanguage));
        Assert.That(metadata.ProjectType, Is.EqualTo(projectType));
        Assert.That(metadata.Languages.Count, Is.EqualTo(1));
        Assert.That(metadata.Languages.First(), Is.EqualTo(primaryLanguage));
        Assert.That(metadata.Frameworks.Count, Is.EqualTo(0));
        Assert.That(metadata.Dependencies.Count, Is.EqualTo(0));
        Assert.That(metadata.CustomProperties.Count, Is.EqualTo(0));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidRepositoryName_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var repositoryUrl = "https://github.com/user/repo";
        var primaryLanguage = "C#";
        var projectType = "Application";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DocumentationMetadata(invalidName, repositoryUrl, primaryLanguage, projectType));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidRepositoryUrl_ThrowsArgumentException(string invalidUrl)
    {
        // Arrange
        var repositoryName = "TestRepo";
        var primaryLanguage = "C#";
        var projectType = "Application";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DocumentationMetadata(repositoryName, invalidUrl, primaryLanguage, projectType));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidPrimaryLanguage_ThrowsArgumentException(string invalidLanguage)
    {
        // Arrange
        var repositoryName = "TestRepo";
        var repositoryUrl = "https://github.com/user/repo";
        var projectType = "Application";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DocumentationMetadata(repositoryName, repositoryUrl, invalidLanguage, projectType));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidProjectType_ThrowsArgumentException(string invalidProjectType)
    {
        // Arrange
        var repositoryName = "TestRepo";
        var repositoryUrl = "https://github.com/user/repo";
        var primaryLanguage = "C#";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DocumentationMetadata(repositoryName, repositoryUrl, primaryLanguage, invalidProjectType));
    }

    [Test]
    public void AddLanguage_ValidLanguage_AddsLanguage()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
        var newLanguage = "JavaScript";

        // Act
        metadata.AddLanguage(newLanguage);

        // Assert
        Assert.That(metadata.Languages.Count, Is.EqualTo(2));
        Assert.That(metadata.Languages.Contains(newLanguage), Is.True);
    }

    [Test]
    public void AddLanguage_DuplicateLanguage_DoesNotAddDuplicate()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
        var duplicateLanguage = "c#"; // Different case

        // Act
        metadata.AddLanguage(duplicateLanguage);

        // Assert
        Assert.That(metadata.Languages.Count, Is.EqualTo(1)); // Should not add duplicate
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void AddLanguage_InvalidLanguage_DoesNotAdd(string invalidLanguage)
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");

        // Act
        metadata.AddLanguage(invalidLanguage);

        // Assert
        Assert.That(metadata.Languages.Count, Is.EqualTo(1)); // Only primary language
    }

    [Test]
    public void AddFramework_ValidFramework_AddsFramework()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
        var framework = "ASP.NET Core";

        // Act
        metadata.AddFramework(framework);

        // Assert
        Assert.That(metadata.Frameworks.Count, Is.EqualTo(1));
        Assert.That(metadata.Frameworks.Contains(framework), Is.True);
    }

    [Test]
    public void AddFramework_DuplicateFramework_DoesNotAddDuplicate()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
        var framework = "ASP.NET Core";
        metadata.AddFramework(framework);

        // Act
        metadata.AddFramework(framework.ToLowerInvariant()); // Different case

        // Assert
        Assert.That(metadata.Frameworks.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddDependency_ValidDependency_AddsDependency()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
        var dependency = "Microsoft.EntityFrameworkCore";

        // Act
        metadata.AddDependency(dependency);

        // Assert
        Assert.That(metadata.Dependencies.Count, Is.EqualTo(1));
        Assert.That(metadata.Dependencies.Contains(dependency), Is.True);
    }

    [Test]
    public void AddDependency_DuplicateDependency_DoesNotAddDuplicate()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
        var dependency = "Microsoft.EntityFrameworkCore";
        metadata.AddDependency(dependency);

        // Act
        metadata.AddDependency(dependency.ToUpperInvariant()); // Different case

        // Assert
        Assert.That(metadata.Dependencies.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddCustomProperty_ValidProperty_AddsProperty()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
        var key = "TargetFramework";
        var value = "net6.0";

        // Act
        metadata.AddCustomProperty(key, value);

        // Assert
        Assert.That(metadata.CustomProperties.Count, Is.EqualTo(1));
        Assert.That(metadata.CustomProperties[key], Is.EqualTo(value));
    }

    [Test]
    public void AddCustomProperty_DuplicateKey_UpdatesValue()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");
        var key = "TargetFramework";
        var originalValue = "net5.0";
        var newValue = "net6.0";
        metadata.AddCustomProperty(key, originalValue);

        // Act
        metadata.AddCustomProperty(key, newValue);

        // Assert
        Assert.That(metadata.CustomProperties.Count, Is.EqualTo(1));
        Assert.That(metadata.CustomProperties[key], Is.EqualTo(newValue));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void AddCustomProperty_InvalidKey_DoesNotAdd(string invalidKey)
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");

        // Act
        metadata.AddCustomProperty(invalidKey, "value");

        // Assert
        Assert.That(metadata.CustomProperties.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddCustomProperty_NullValue_DoesNotAdd()
    {
        // Arrange
        var metadata = new DocumentationMetadata("TestRepo", "https://github.com/user/repo", "C#", "Application");

        // Act
        metadata.AddCustomProperty("key", null!);

        // Assert
        Assert.That(metadata.CustomProperties.Count, Is.EqualTo(0));
    }
}