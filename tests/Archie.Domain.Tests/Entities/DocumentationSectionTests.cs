using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.Entities;

[TestFixture]
public class DocumentationSectionTests
{
    [Test]
    public void Create_ValidInputs_CreatesDocumentationSection()
    {
        // Arrange
        var title = "Overview";
        var content = "This is an overview section";
        var type = DocumentationSectionType.Overview;
        var order = 1;

        // Act
        var section = DocumentationSection.Create(title, content, type, order);

        // Assert
        Assert.That(section.Title, Is.EqualTo(title));
        Assert.That(section.Content, Is.EqualTo(content));
        Assert.That(section.Type, Is.EqualTo(type));
        Assert.That(section.Order, Is.EqualTo(order));
        Assert.That(section.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(section.CodeReferences.Count, Is.EqualTo(0));
        Assert.That(section.Tags.Count, Is.EqualTo(0));
        Assert.That(section.Metadata, Is.Not.Null);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Create_InvalidTitle_ThrowsArgumentException(string invalidTitle)
    {
        // Arrange
        var content = "Valid content";
        var type = DocumentationSectionType.Overview;
        var order = 1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DocumentationSection.Create(invalidTitle, content, type, order));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Create_InvalidContent_ThrowsArgumentException(string invalidContent)
    {
        // Arrange
        var title = "Valid Title";
        var type = DocumentationSectionType.Overview;
        var order = 1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DocumentationSection.Create(title, invalidContent, type, order));
    }

    [TestCase(-1)]
    [TestCase(-10)]
    public void Create_NegativeOrder_ThrowsArgumentOutOfRangeException(int negativeOrder)
    {
        // Arrange
        var title = "Valid Title";
        var content = "Valid content";
        var type = DocumentationSectionType.Overview;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => DocumentationSection.Create(title, content, type, negativeOrder));
    }

    [Test]
    public void UpdateContent_ValidContent_UpdatesContentAndModifiedTime()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Original content", DocumentationSectionType.Overview, 1);
        var originalModifiedTime = section.Metadata.LastModifiedAt;
        var newContent = "Updated content";

        // Act
        Thread.Sleep(10); // Ensure time difference (10ms for better reliability)
        section.UpdateContent(newContent);

        // Assert
        Assert.That(section.Content, Is.EqualTo(newContent));
        Assert.That(section.Metadata.LastModifiedAt > originalModifiedTime, Is.True);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void UpdateContent_InvalidContent_ThrowsArgumentException(string invalidContent)
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Original content", DocumentationSectionType.Overview, 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => section.UpdateContent(invalidContent));
    }

    [Test]
    public void UpdateTitle_ValidTitle_UpdatesTitleAndModifiedTime()
    {
        // Arrange
        var section = DocumentationSection.Create("Original Title", "Content", DocumentationSectionType.Overview, 1);
        var originalModifiedTime = section.Metadata.LastModifiedAt;
        var newTitle = "Updated Title";

        // Act
        Thread.Sleep(10); // Ensure time difference (10ms for better reliability)
        section.UpdateTitle(newTitle);

        // Assert
        Assert.That(section.Title, Is.EqualTo(newTitle));
        Assert.That(section.Metadata.LastModifiedAt > originalModifiedTime, Is.True);
    }

    [Test]
    public void UpdateOrder_ValidOrder_UpdatesOrderAndModifiedTime()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var originalModifiedTime = section.Metadata.LastModifiedAt;
        var newOrder = 5;

        // Act
        Thread.Sleep(10); // Ensure time difference (10ms for better reliability)
        section.UpdateOrder(newOrder);

        // Assert
        Assert.That(section.Order, Is.EqualTo(newOrder));
        Assert.That(section.Metadata.LastModifiedAt > originalModifiedTime, Is.True);
    }

    [Test]
    public void UpdateOrder_NegativeOrder_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => section.UpdateOrder(-1));
    }

    [Test]
    public void AddCodeReference_ValidReference_AddsReference()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";

        // Act
        section.AddCodeReference(filePath, codeSnippet, description, referenceType);

        // Assert
        Assert.That(section.CodeReferences.Count, Is.EqualTo(1));
        var addedReference = section.CodeReferences.First();
        Assert.That(addedReference.FilePath, Is.EqualTo(filePath));
        Assert.That(addedReference.CodeSnippet, Is.EqualTo(codeSnippet));
        Assert.That(addedReference.Description, Is.EqualTo(description));
        Assert.That(addedReference.ReferenceType, Is.EqualTo(referenceType));
    }

    [Test]
    public void AddCodeReference_WithLineNumbers_AddsReferenceWithLineNumbers()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";
        var startLine = 10;
        var endLine = 12;

        // Act
        section.AddCodeReference(filePath, codeSnippet, description, referenceType, startLine, endLine);

        // Assert
        Assert.That(section.CodeReferences.Count, Is.EqualTo(1));
        var addedReference = section.CodeReferences.First();
        Assert.That(addedReference.StartLine, Is.EqualTo(startLine));
        Assert.That(addedReference.EndLine, Is.EqualTo(endLine));
    }

    [Test]
    public void AddCodeReference_DuplicateReference_DoesNotAddDuplicate()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";
        var startLine = 10;

        // Act
        section.AddCodeReference(filePath, codeSnippet, description, referenceType, startLine);
        section.AddCodeReference(filePath, "Different snippet", "Different description", "Different type", startLine); // Same file and line

        // Assert
        Assert.That(section.CodeReferences.Count, Is.EqualTo(1)); // Should not add duplicate
    }

    [Test]
    public void RemoveCodeReference_ExistingReference_RemovesReference()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var filePath = "Program.cs";
        section.AddCodeReference(filePath, "code", "description", "type", 10);

        // Act
        section.RemoveCodeReference(filePath, 10);

        // Assert
        Assert.That(section.CodeReferences.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveCodeReference_NonExistentReference_DoesNothing()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        section.AddCodeReference("Program.cs", "code", "description", "type", 10);

        // Act
        section.RemoveCodeReference("NonExistent.cs", 10);

        // Assert
        Assert.That(section.CodeReferences.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddTag_ValidTag_AddsTag()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var tag = "csharp";

        // Act
        section.AddTag(tag);

        // Assert
        Assert.That(section.Tags.Count, Is.EqualTo(1));
        Assert.That(section.Tags.First(), Is.EqualTo(tag.ToLowerInvariant()));
    }

    [Test]
    public void AddTag_DuplicateTag_DoesNotAddDuplicate()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var tag = "csharp";

        // Act
        section.AddTag(tag);
        section.AddTag(tag.ToUpperInvariant()); // Same tag, different case

        // Assert
        Assert.That(section.Tags.Count, Is.EqualTo(1));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void AddTag_InvalidTag_DoesNotAdd(string invalidTag)
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);

        // Act
        section.AddTag(invalidTag);

        // Assert
        Assert.That(section.Tags.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveTag_ExistingTag_RemovesTag()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var tag = "csharp";
        section.AddTag(tag);

        // Act
        section.RemoveTag(tag.ToUpperInvariant()); // Different case

        // Assert
        Assert.That(section.Tags.Count, Is.EqualTo(0));
    }

    [Test]
    public void ClearTags_WithTags_RemovesAllTags()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        section.AddTag("tag1");
        section.AddTag("tag2");
        section.AddTag("tag3");

        // Act
        section.ClearTags();

        // Assert
        Assert.That(section.Tags.Count, Is.EqualTo(0));
    }

    [Test]
    public void HasTag_ExistingTag_ReturnsTrue()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        var tag = "csharp";
        section.AddTag(tag);

        // Act & Assert
        Assert.That(section.HasTag(tag.ToUpperInvariant()), Is.True); // Case insensitive
    }

    [Test]
    public void HasTag_NonExistentTag_ReturnsFalse()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        section.AddTag("csharp");

        // Act & Assert
        Assert.That(section.HasTag("javascript"), Is.False);
    }

    [Test]
    public void HasCodeReferences_WithReferences_ReturnsTrue()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);
        section.AddCodeReference("file.cs", "code", "desc", "type");

        // Act & Assert
        Assert.That(section.HasCodeReferences, Is.True);
    }

    [Test]
    public void HasCodeReferences_WithoutReferences_ReturnsFalse()
    {
        // Arrange
        var section = DocumentationSection.Create("Title", "Content", DocumentationSectionType.Overview, 1);

        // Act & Assert
        Assert.That(section.HasCodeReferences, Is.False);
    }

    [Test]
    public void GetWordCount_ValidContent_ReturnsCorrectCount()
    {
        // Arrange
        var content = "This is a test content with ten words exactly.";
        var section = DocumentationSection.Create("Title", content, DocumentationSectionType.Overview, 1);

        // Act
        var wordCount = section.GetWordCount();

        // Assert
        Assert.That(wordCount, Is.EqualTo(9)); // "This", "is", "a", "test", "content", "with", "ten", "words", "exactly."
    }

    [Test]
    public void GetWordCount_SingleWordContent_ReturnsOne()
    {
        // Arrange - Test with minimal valid content
        var section = DocumentationSection.Create("Title", "Word", DocumentationSectionType.Overview, 1);

        // Act
        var wordCount = section.GetWordCount();

        // Assert
        Assert.That(wordCount, Is.EqualTo(1));
    }

    [Test]
    public void GetWordCount_ContentWithMultipleSpaces_ReturnsCorrectCount()
    {
        // Arrange
        var content = "Word1   Word2\t\tWord3\n\nWord4";
        var section = DocumentationSection.Create("Title", content, DocumentationSectionType.Overview, 1);

        // Act
        var wordCount = section.GetWordCount();

        // Assert
        Assert.That(wordCount, Is.EqualTo(4));
    }
}