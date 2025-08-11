using Archie.Domain.ValueObjects;

namespace Archie.Domain.Tests.ValueObjects;

[TestFixture]
public class CodeReferenceTests
{
    [Test]
    public void Constructor_ValidInputs_CreatesCodeReference()
    {
        // Arrange
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";
        var startLine = 10;
        var endLine = 12;

        // Act
        var codeReference = new CodeReference(filePath, codeSnippet, description, referenceType, startLine, endLine);

        // Assert
        Assert.That(codeReference.FilePath, Is.EqualTo(filePath));
        Assert.That(codeReference.CodeSnippet, Is.EqualTo(codeSnippet));
        Assert.That(codeReference.Description, Is.EqualTo(description));
        Assert.That(codeReference.ReferenceType, Is.EqualTo(referenceType));
        Assert.That(codeReference.StartLine, Is.EqualTo(startLine));
        Assert.That(codeReference.EndLine, Is.EqualTo(endLine));
    }

    [Test]
    public void Constructor_WithoutLineNumbers_CreatesCodeReferenceWithNullLines()
    {
        // Arrange
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";

        // Act
        var codeReference = new CodeReference(filePath, codeSnippet, description, referenceType);

        // Assert
        Assert.That(codeReference.FilePath, Is.EqualTo(filePath));
        Assert.That(codeReference.CodeSnippet, Is.EqualTo(codeSnippet));
        Assert.That(codeReference.Description, Is.EqualTo(description));
        Assert.That(codeReference.ReferenceType, Is.EqualTo(referenceType));
        Assert.That(codeReference.StartLine, Is.Null);
        Assert.That(codeReference.EndLine, Is.Null);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidFilePath_ThrowsArgumentException(string invalidFilePath)
    {
        // Arrange
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CodeReference(invalidFilePath, codeSnippet, description, referenceType));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidCodeSnippet_ThrowsArgumentException(string invalidCodeSnippet)
    {
        // Arrange
        var filePath = "Program.cs";
        var description = "Hello world example";
        var referenceType = "Method";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CodeReference(filePath, invalidCodeSnippet, description, referenceType));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidDescription_ThrowsArgumentException(string invalidDescription)
    {
        // Arrange
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var referenceType = "Method";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CodeReference(filePath, codeSnippet, invalidDescription, referenceType));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Constructor_InvalidReferenceType_ThrowsArgumentException(string invalidReferenceType)
    {
        // Arrange
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CodeReference(filePath, codeSnippet, description, invalidReferenceType));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-10)]
    public void Constructor_InvalidStartLine_ThrowsArgumentOutOfRangeException(int invalidStartLine)
    {
        // Arrange
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new CodeReference(filePath, codeSnippet, description, referenceType, invalidStartLine));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-10)]
    public void Constructor_InvalidEndLine_ThrowsArgumentOutOfRangeException(int invalidEndLine)
    {
        // Arrange
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new CodeReference(filePath, codeSnippet, description, referenceType, 10, invalidEndLine));
    }

    [Test]
    public void Constructor_EndLineBeforeStartLine_ThrowsArgumentException()
    {
        // Arrange
        var filePath = "Program.cs";
        var codeSnippet = "Console.WriteLine(\"Hello World\");";
        var description = "Hello world example";
        var referenceType = "Method";
        var startLine = 15;
        var endLine = 10; // Before start line

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new CodeReference(filePath, codeSnippet, description, referenceType, startLine, endLine));
    }

    [Test]
    public void HasLineNumbers_WithStartLine_ReturnsTrue()
    {
        // Arrange
        var codeReference = new CodeReference("Program.cs", "code", "description", "type", 10);

        // Act & Assert
        Assert.That(codeReference.HasLineNumbers, Is.True);
    }

    [Test]
    public void HasLineNumbers_WithoutStartLine_ReturnsFalse()
    {
        // Arrange
        var codeReference = new CodeReference("Program.cs", "code", "description", "type");

        // Act & Assert
        Assert.That(codeReference.HasLineNumbers, Is.False);
    }

    [Test]
    public void HasRange_WithStartAndEndLine_ReturnsTrue()
    {
        // Arrange
        var codeReference = new CodeReference("Program.cs", "code", "description", "type", 10, 15);

        // Act & Assert
        Assert.That(codeReference.HasRange, Is.True);
    }

    [Test]
    public void HasRange_WithOnlyStartLine_ReturnsFalse()
    {
        // Arrange
        var codeReference = new CodeReference("Program.cs", "code", "description", "type", 10);

        // Act & Assert
        Assert.That(codeReference.HasRange, Is.False);
    }

    [Test]
    public void HasRange_WithoutLineNumbers_ReturnsFalse()
    {
        // Arrange
        var codeReference = new CodeReference("Program.cs", "code", "description", "type");

        // Act & Assert
        Assert.That(codeReference.HasRange, Is.False);
    }

    [Test]
    public void GetDisplayLocation_WithRange_ReturnsFilePathWithRange()
    {
        // Arrange
        var filePath = "Program.cs";
        var startLine = 10;
        var endLine = 15;
        var codeReference = new CodeReference(filePath, "code", "description", "type", startLine, endLine);
        var expectedLocation = $"{filePath}:{startLine}-{endLine}";

        // Act
        var location = codeReference.GetDisplayLocation();

        // Assert
        Assert.That(location, Is.EqualTo(expectedLocation));
    }

    [Test]
    public void GetDisplayLocation_WithStartLineOnly_ReturnsFilePathWithLine()
    {
        // Arrange
        var filePath = "Program.cs";
        var startLine = 10;
        var codeReference = new CodeReference(filePath, "code", "description", "type", startLine);
        var expectedLocation = $"{filePath}:{startLine}";

        // Act
        var location = codeReference.GetDisplayLocation();

        // Assert
        Assert.That(location, Is.EqualTo(expectedLocation));
    }

    [Test]
    public void GetDisplayLocation_WithoutLineNumbers_ReturnsFilePathOnly()
    {
        // Arrange
        var filePath = "Program.cs";
        var codeReference = new CodeReference(filePath, "code", "description", "type");

        // Act
        var location = codeReference.GetDisplayLocation();

        // Assert
        Assert.That(location, Is.EqualTo(filePath));
    }

    [Test]
    public void Constructor_ValidRangeWithSameStartAndEnd_CreatesValidReference()
    {
        // Arrange
        var filePath = "Program.cs";
        var codeSnippet = "var x = 1;";
        var description = "Variable declaration";
        var referenceType = "Variable";
        var line = 10;

        // Act
        var codeReference = new CodeReference(filePath, codeSnippet, description, referenceType, line, line);

        // Assert
        Assert.That(codeReference.StartLine, Is.EqualTo(line));
        Assert.That(codeReference.EndLine, Is.EqualTo(line));
        Assert.That(codeReference.HasRange, Is.True);
        Assert.That(codeReference.GetDisplayLocation(), Is.EqualTo($"{filePath}:{line}-{line}"));
    }
}