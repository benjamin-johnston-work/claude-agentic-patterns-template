using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Archie.Infrastructure.AzureSearch.Interfaces;

namespace Archie.Infrastructure.AzureSearch.Services;

/// <summary>
/// Implementation of code symbol extraction using regex patterns for various programming languages.
/// Extracts functions, classes, interfaces, methods, and other important code constructs.
/// </summary>
public class CodeSymbolExtractor : ICodeSymbolExtractor
{
    private readonly ILogger<CodeSymbolExtractor> _logger;
    
    // Language-specific regex patterns for extracting code symbols
    private static readonly Dictionary<string, List<SymbolPattern>> LanguagePatterns = new()
    {
        ["csharp"] = new List<SymbolPattern>
        {
            new("class", @"(?:public|private|internal|protected)?\s*(?:static|abstract|sealed)?\s*class\s+(\w+)", RegexOptions.Multiline),
            new("interface", @"(?:public|private|internal|protected)?\s*interface\s+(\w+)", RegexOptions.Multiline),
            new("method", @"(?:public|private|internal|protected)?\s*(?:static|virtual|override|abstract)?\s*(?:\w+\s+)*(\w+)\s*\([^)]*\)\s*(?:\{|;)", RegexOptions.Multiline),
            new("property", @"(?:public|private|internal|protected)?\s*(?:static)?\s*\w+\s+(\w+)\s*\{\s*get", RegexOptions.Multiline),
            new("enum", @"(?:public|private|internal|protected)?\s*enum\s+(\w+)", RegexOptions.Multiline),
            new("struct", @"(?:public|private|internal|protected)?\s*struct\s+(\w+)", RegexOptions.Multiline),
            new("namespace", @"namespace\s+([\w\.]+)", RegexOptions.Multiline)
        },
        
        ["javascript"] = new List<SymbolPattern>
        {
            new("function", @"function\s+(\w+)\s*\(", RegexOptions.Multiline),
            new("arrow_function", @"(?:const|let|var)\s+(\w+)\s*=\s*\([^)]*\)\s*=>", RegexOptions.Multiline),
            new("class", @"class\s+(\w+)", RegexOptions.Multiline),
            new("method", @"(\w+)\s*\([^)]*\)\s*\{", RegexOptions.Multiline),
            new("export_function", @"export\s+(?:function\s+)?(\w+)", RegexOptions.Multiline),
            new("async_function", @"async\s+function\s+(\w+)", RegexOptions.Multiline)
        },
        
        ["typescript"] = new List<SymbolPattern>
        {
            new("function", @"function\s+(\w+)\s*\(", RegexOptions.Multiline),
            new("arrow_function", @"(?:const|let|var)\s+(\w+)\s*=\s*\([^)]*\)\s*=>", RegexOptions.Multiline),
            new("class", @"class\s+(\w+)", RegexOptions.Multiline),
            new("interface", @"interface\s+(\w+)", RegexOptions.Multiline),
            new("type", @"type\s+(\w+)", RegexOptions.Multiline),
            new("method", @"(\w+)\s*\([^)]*\)\s*:\s*\w+\s*\{", RegexOptions.Multiline),
            new("export_function", @"export\s+(?:function\s+)?(\w+)", RegexOptions.Multiline),
            new("enum", @"enum\s+(\w+)", RegexOptions.Multiline)
        },
        
        ["python"] = new List<SymbolPattern>
        {
            new("function", @"def\s+(\w+)\s*\(", RegexOptions.Multiline),
            new("class", @"class\s+(\w+)", RegexOptions.Multiline),
            new("async_function", @"async\s+def\s+(\w+)", RegexOptions.Multiline),
            new("method", @"^\s+def\s+(\w+)\s*\(", RegexOptions.Multiline),
            new("staticmethod", @"@staticmethod\s+def\s+(\w+)", RegexOptions.Multiline | RegexOptions.Singleline),
            new("classmethod", @"@classmethod\s+def\s+(\w+)", RegexOptions.Multiline | RegexOptions.Singleline)
        },
        
        ["java"] = new List<SymbolPattern>
        {
            new("class", @"(?:public|private|protected)?\s*class\s+(\w+)", RegexOptions.Multiline),
            new("interface", @"(?:public|private|protected)?\s*interface\s+(\w+)", RegexOptions.Multiline),
            new("method", @"(?:public|private|protected)?\s*(?:static)?\s*\w+\s+(\w+)\s*\([^)]*\)\s*\{", RegexOptions.Multiline),
            new("enum", @"(?:public|private|protected)?\s*enum\s+(\w+)", RegexOptions.Multiline),
            new("package", @"package\s+([\w\.]+)", RegexOptions.Multiline)
        },
        
        ["go"] = new List<SymbolPattern>
        {
            new("function", @"func\s+(\w+)\s*\(", RegexOptions.Multiline),
            new("method", @"func\s+\([^)]+\)\s+(\w+)\s*\(", RegexOptions.Multiline),
            new("type", @"type\s+(\w+)\s+(?:struct|interface)", RegexOptions.Multiline),
            new("package", @"package\s+(\w+)", RegexOptions.Multiline),
            new("const", @"const\s+(\w+)", RegexOptions.Multiline),
            new("var", @"var\s+(\w+)", RegexOptions.Multiline)
        },
        
        ["rust"] = new List<SymbolPattern>
        {
            new("function", @"fn\s+(\w+)\s*\(", RegexOptions.Multiline),
            new("struct", @"struct\s+(\w+)", RegexOptions.Multiline),
            new("enum", @"enum\s+(\w+)", RegexOptions.Multiline),
            new("trait", @"trait\s+(\w+)", RegexOptions.Multiline),
            new("impl", @"impl.*?for\s+(\w+)", RegexOptions.Multiline),
            new("mod", @"mod\s+(\w+)", RegexOptions.Multiline)
        },
        
        ["cpp"] = new List<SymbolPattern>
        {
            new("function", @"(?:\w+\s+)*(\w+)\s*\([^)]*\)\s*\{", RegexOptions.Multiline),
            new("class", @"class\s+(\w+)", RegexOptions.Multiline),
            new("struct", @"struct\s+(\w+)", RegexOptions.Multiline),
            new("namespace", @"namespace\s+(\w+)", RegexOptions.Multiline),
            new("enum", @"enum\s+(?:class\s+)?(\w+)", RegexOptions.Multiline)
        }
    };

    // Common patterns that apply to multiple languages
    private static readonly List<SymbolPattern> CommonPatterns = new()
    {
        new("comment_todo", @"(?://|#)\s*(?:TODO|FIXME|HACK|NOTE):\s*(.+)", RegexOptions.Multiline),
        new("import", @"(?:import|from|#include|using)\s+([\w\.\/""\<\>]+)", RegexOptions.Multiline)
    };

    public CodeSymbolExtractor(ILogger<CodeSymbolExtractor> logger)
    {
        _logger = logger;
    }

    public async Task<List<string>> ExtractSymbolsAsync(string content, string language, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new List<string>();
        }

        var normalizedLanguage = language.ToLowerInvariant();
        var symbols = new HashSet<string>(); // Use HashSet to avoid duplicates

        try
        {
            // Extract language-specific symbols
            if (LanguagePatterns.TryGetValue(normalizedLanguage, out var patterns))
            {
                foreach (var pattern in patterns)
                {
                    await ExtractSymbolsWithPattern(content, pattern, symbols, cancellationToken);
                }
            }

            // Extract common symbols (comments, imports, etc.)
            foreach (var pattern in CommonPatterns)
            {
                await ExtractSymbolsWithPattern(content, pattern, symbols, cancellationToken);
            }

            // For unknown languages, try to extract basic identifiers
            if (!LanguagePatterns.ContainsKey(normalizedLanguage))
            {
                await ExtractBasicIdentifiers(content, symbols, cancellationToken);
            }

            var symbolList = symbols.Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Where(s => s.Length > 1 && s.Length <= 100) // Filter reasonable symbol names
                .OrderBy(s => s)
                .ToList();

            _logger.LogDebug("Extracted {SymbolCount} symbols from {Language} code ({ContentLength} characters)",
                symbolList.Count, language, content.Length);

            return symbolList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting symbols from {Language} code", language);
            return new List<string>();
        }
    }

    public bool IsLanguageSupported(string language)
    {
        return LanguagePatterns.ContainsKey(language.ToLowerInvariant());
    }

    public IEnumerable<string> GetSupportedLanguages()
    {
        return LanguagePatterns.Keys;
    }

    #region Private Helper Methods

    private async Task ExtractSymbolsWithPattern(string content, SymbolPattern pattern, HashSet<string> symbols, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            try
            {
                var regex = new Regex(pattern.Pattern, pattern.Options | RegexOptions.Compiled, TimeSpan.FromSeconds(5));
                var matches = regex.Matches(content);

                foreach (Match match in matches)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (match.Groups.Count > 1)
                    {
                        var symbolName = match.Groups[1].Value.Trim();
                        
                        // Filter out common non-symbol matches
                        if (IsValidSymbolName(symbolName))
                        {
                            symbols.Add($"{pattern.SymbolType}:{symbolName}");
                        }
                    }
                }
            }
            catch (RegexMatchTimeoutException)
            {
                _logger.LogWarning("Regex timeout extracting symbols with pattern: {Pattern}", pattern.Pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting symbols with pattern: {Pattern}", pattern.Pattern);
            }
        }, cancellationToken);
    }

    private async Task ExtractBasicIdentifiers(string content, HashSet<string> symbols, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            try
            {
                // Extract basic word-like identifiers that might be function or variable names
                var identifierPattern = new Regex(@"\b([A-Za-z_][A-Za-z0-9_]{2,})\b", 
                    RegexOptions.Compiled, TimeSpan.FromSeconds(5));
                
                var matches = identifierPattern.Matches(content);
                var identifierCounts = new Dictionary<string, int>();

                foreach (Match match in matches)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var identifier = match.Groups[1].Value;
                    
                    if (IsValidSymbolName(identifier))
                    {
                        identifierCounts[identifier] = identifierCounts.GetValueOrDefault(identifier, 0) + 1;
                    }
                }

                // Add identifiers that appear multiple times (likely to be important)
                foreach (var kvp in identifierCounts.Where(x => x.Value >= 2))
                {
                    symbols.Add($"identifier:{kvp.Key}");
                }
            }
            catch (RegexMatchTimeoutException)
            {
                _logger.LogWarning("Regex timeout extracting basic identifiers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting basic identifiers");
            }
        }, cancellationToken);
    }

    private static bool IsValidSymbolName(string symbolName)
    {
        if (string.IsNullOrWhiteSpace(symbolName) || symbolName.Length < 2 || symbolName.Length > 100)
            return false;

        // Filter out common keywords and noise
        var commonKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "if", "else", "for", "while", "do", "switch", "case", "break", "continue", "return",
            "public", "private", "protected", "static", "final", "abstract", "virtual", "override",
            "class", "interface", "struct", "enum", "namespace", "using", "import", "from",
            "function", "var", "let", "const", "def", "async", "await", "try", "catch", "finally",
            "true", "false", "null", "undefined", "void", "int", "string", "bool", "double", "float",
            "this", "self", "super", "new", "delete", "typeof", "instanceof", "in", "of"
        };

        if (commonKeywords.Contains(symbolName))
            return false;

        // Check if it looks like a valid identifier
        return Regex.IsMatch(symbolName, @"^[A-Za-z_][A-Za-z0-9_]*$");
    }

    #endregion

    /// <summary>
    /// Represents a regex pattern for extracting a specific type of symbol
    /// </summary>
    private record SymbolPattern(string SymbolType, string Pattern, RegexOptions Options);
}