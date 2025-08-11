namespace Archie.Infrastructure.AzureSearch.Interfaces;

/// <summary>
/// Interface for extracting code symbols (functions, classes, methods, interfaces, etc.)
/// from source code files to enhance search capabilities.
/// </summary>
public interface ICodeSymbolExtractor
{
    /// <summary>
    /// Extract code symbols from source code content based on the programming language.
    /// </summary>
    /// <param name="content">The source code content</param>
    /// <param name="language">The programming language (e.g., "csharp", "javascript", "python")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of extracted code symbols (function names, class names, etc.)</returns>
    Task<List<string>> ExtractSymbolsAsync(string content, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if symbol extraction is supported for the given programming language.
    /// </summary>
    /// <param name="language">The programming language</param>
    /// <returns>True if the language is supported for symbol extraction</returns>
    bool IsLanguageSupported(string language);

    /// <summary>
    /// Get all supported programming languages for symbol extraction.
    /// </summary>
    /// <returns>List of supported language identifiers</returns>
    IEnumerable<string> GetSupportedLanguages();
}