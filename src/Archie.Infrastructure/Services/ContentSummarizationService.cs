using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Archie.Domain.ValueObjects;

namespace Archie.Infrastructure.Services;

/// <summary>
/// Service for analyzing and summarizing file content to extract functionality and purpose
/// </summary>
public class ContentSummarizationService
{
    private readonly ILogger<ContentSummarizationService> _logger;

    public ContentSummarizationService(ILogger<ContentSummarizationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Summarize source code file to extract key functionality
    /// </summary>
    public async Task<ContentSummary> SummarizeCodeFileAsync(string filePath, string content)
    {
        try
        {
            var language = DetermineLanguageFromPath(filePath);
            var summary = new ContentSummary(filePath, language, "");
            
            summary.LinesOfCode = content.Split('\n').Length;
            summary.ContentType = DetermineContentType(filePath, content);
            
            // Extract functionality based on language
            var functionality = await ExtractCodeFunctionalityAsync(content, language);
            summary.FunctionalityDescription = functionality;
            
            // Extract key functions and components
            ExtractCodeElements(content, language, summary);
            
            // Generate a meaningful code snippet
            summary.CodeSnippet = ExtractRepresentativeCodeSnippet(content, language, 150);
            
            _logger.LogDebug("Summarized code file {FilePath}: {Functionality}", filePath, functionality);
            
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize code file {FilePath}", filePath);
            return new ContentSummary(filePath, "unknown", "Error analyzing file content");
        }
    }

    /// <summary>
    /// Extract project purpose and key features from README content
    /// </summary>
    public async Task<ProjectPurpose> ExtractProjectPurposeFromReadmeAsync(string readmeContent)
    {
        try
        {
            var purpose = new ProjectPurpose();
            
            // Extract title/heading as description
            var titleMatch = Regex.Match(readmeContent, @"^#\s+(.+)", RegexOptions.Multiline);
            if (titleMatch.Success)
            {
                purpose.Description = titleMatch.Groups[1].Value.Trim();
            }

            // Look for description section
            var descriptionMatch = Regex.Match(readmeContent, 
                @"(?:## Description|## About|## Overview)\s*\n((?:.|\n)*?)(?=##|\z)", 
                RegexOptions.IgnoreCase);
            
            if (descriptionMatch.Success)
            {
                var description = CleanTextContent(descriptionMatch.Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    purpose.Description = description;
                }
            }

            // Extract features
            var featuresMatch = Regex.Match(readmeContent, 
                @"(?:## Features|## Key Features|## What|## Highlights)\s*\n((?:.|\n)*?)(?=##|\z)", 
                RegexOptions.IgnoreCase);
            
            if (featuresMatch.Success)
            {
                var featureText = featuresMatch.Groups[1].Value;
                var features = ExtractListItems(featureText);
                foreach (var feature in features.Take(8)) // Limit features
                {
                    purpose.AddFeature(feature);
                }
            }

            // Determine business domain from content analysis
            purpose.BusinessDomain = DetermineBusinessDomain(readmeContent);
            
            // Extract user value
            purpose.UserValue = ExtractUserValue(readmeContent);
            
            _logger.LogDebug("Extracted project purpose: {Description} in domain {Domain}", 
                purpose.Description, purpose.BusinessDomain);
                
            return purpose;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract project purpose from README");
            return new ProjectPurpose();
        }
    }

    /// <summary>
    /// Summarize configuration file content
    /// </summary>
    public async Task<string> SummarizeConfigurationAsync(string filePath, string configContent)
    {
        try
        {
            var fileName = Path.GetFileName(filePath).ToLowerInvariant();
            
            return fileName switch
            {
                "package.json" => SummarizePackageJson(configContent),
                "pom.xml" => SummarizePomXml(configContent),
                "requirements.txt" => SummarizeRequirements(configContent),
                "dockerfile" => SummarizeDockerfile(configContent),
                _ when fileName.EndsWith(".json") => SummarizeJsonConfig(configContent),
                _ when fileName.EndsWith(".xml") => SummarizeXmlConfig(configContent),
                _ when fileName.EndsWith(".yml") || fileName.EndsWith(".yaml") => SummarizeYamlConfig(configContent),
                _ => $"Configuration file with {configContent.Split('\n').Length} lines"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize configuration {FilePath}", filePath);
            return "Configuration file";
        }
    }

    #region Private Helper Methods

    private async Task<string> ExtractCodeFunctionalityAsync(string content, string language)
    {
        return language.ToLowerInvariant() switch
        {
            "javascript" => ExtractJavaScriptFunctionality(content),
            "html" => ExtractHtmlFunctionality(content),
            "css" => ExtractCssFunctionality(content),
            "python" => ExtractPythonFunctionality(content),
            "csharp" => ExtractCSharpFunctionality(content),
            "java" => ExtractJavaFunctionality(content),
            _ => ExtractGenericFunctionality(content, language)
        };
    }

    private string ExtractJavaScriptFunctionality(string content)
    {
        var functionality = new List<string>();

        // Look for game-specific patterns
        if (Regex.IsMatch(content, @"canvas|getContext|drawImage|requestAnimationFrame", RegexOptions.IgnoreCase))
        {
            if (Regex.IsMatch(content, @"ball|paddle|brick|collision|game", RegexOptions.IgnoreCase))
            {
                functionality.Add("Game graphics and animation using HTML5 Canvas");
                functionality.Add("Ball physics and collision detection");
            }
            else
            {
                functionality.Add("Canvas-based graphics and animation");
            }
        }

        // Look for class definitions
        var classMatches = Regex.Matches(content, @"class\s+(\w+)", RegexOptions.IgnoreCase);
        if (classMatches.Count > 0)
        {
            var classes = classMatches.Cast<Match>().Select(m => m.Groups[1].Value).Take(3);
            functionality.Add($"Defines classes: {string.Join(", ", classes)}");
        }

        // Look for function definitions
        var functionMatches = Regex.Matches(content, @"(?:function\s+(\w+)|(\w+)\s*[:=]\s*(?:function|\([^)]*\)\s*=>))", RegexOptions.IgnoreCase);
        if (functionMatches.Count > 0)
        {
            var functions = functionMatches.Cast<Match>()
                .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Take(3);
            functionality.Add($"Key functions: {string.Join(", ", functions)}");
        }

        // Look for event handling
        if (Regex.IsMatch(content, @"addEventListener|onClick|onLoad|keydown|mousedown", RegexOptions.IgnoreCase))
        {
            functionality.Add("Handles user input events");
        }

        // Look for API calls
        if (Regex.IsMatch(content, @"fetch|axios|XMLHttpRequest|ajax", RegexOptions.IgnoreCase))
        {
            functionality.Add("Makes HTTP requests to external APIs");
        }

        return functionality.Any() 
            ? string.Join(". ", functionality)
            : "JavaScript functionality not clearly identified";
    }

    private string ExtractHtmlFunctionality(string content)
    {
        var functionality = new List<string>();

        // Look for canvas element
        if (Regex.IsMatch(content, @"<canvas", RegexOptions.IgnoreCase))
        {
            functionality.Add("Provides HTML5 Canvas element for graphics");
        }

        // Look for form elements
        if (Regex.IsMatch(content, @"<form|<input|<button", RegexOptions.IgnoreCase))
        {
            functionality.Add("Contains interactive forms and input elements");
        }

        // Look for game-related content
        if (Regex.IsMatch(content, @"game|play|start|score|level", RegexOptions.IgnoreCase))
        {
            functionality.Add("Game interface with controls and display elements");
        }

        // Count major sections
        var headingCount = Regex.Matches(content, @"<h[1-6]", RegexOptions.IgnoreCase).Count;
        if (headingCount > 0)
        {
            functionality.Add($"Structured content with {headingCount} sections");
        }

        return functionality.Any() 
            ? string.Join(". ", functionality) 
            : "HTML document structure";
    }

    private string ExtractCssFunctionality(string content)
    {
        var functionality = new List<string>();

        // Count selectors
        var selectorCount = Regex.Matches(content, @"[.#]?\w+\s*{", RegexOptions.IgnoreCase).Count;
        if (selectorCount > 0)
        {
            functionality.Add($"Styles {selectorCount} elements");
        }

        // Look for animations
        if (Regex.IsMatch(content, @"@keyframes|animation|transition", RegexOptions.IgnoreCase))
        {
            functionality.Add("Includes CSS animations and transitions");
        }

        // Look for responsive design
        if (Regex.IsMatch(content, @"@media|flex|grid", RegexOptions.IgnoreCase))
        {
            functionality.Add("Responsive design with modern CSS features");
        }

        return functionality.Any() 
            ? string.Join(". ", functionality) 
            : "CSS styling definitions";
    }

    private string ExtractGenericFunctionality(string content, string language)
    {
        var lineCount = content.Split('\n').Length;
        var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        return $"{language} file with {lineCount} lines containing implementation logic";
    }

    private void ExtractCodeElements(string content, string language, ContentSummary summary)
    {
        if (language.ToLowerInvariant() == "javascript")
        {
            // Extract function names
            var functionMatches = Regex.Matches(content, @"(?:function\s+(\w+)|(\w+)\s*[:=]\s*(?:function|\([^)]*\)\s*=>))", RegexOptions.IgnoreCase);
            foreach (Match match in functionMatches.Take(5))
            {
                var functionName = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(functionName))
                {
                    summary.AddKeyFunction(functionName);
                }
            }

            // Extract imports
            var importMatches = Regex.Matches(content, @"import\s+.*?\s+from\s+['""]([^'""]+)['""]", RegexOptions.IgnoreCase);
            foreach (Match match in importMatches.Take(5))
            {
                summary.AddImportedModule(match.Groups[1].Value);
            }
        }
    }

    private string ExtractRepresentativeCodeSnippet(string content, string language, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        if (maxLength <= 0)
        {
            maxLength = 150; // Default safe value
        }

        try
        {
            // Find the most interesting part of the code
            var lines = content.Split('\n');
            
            // Look for class definitions first
            var classLineIndex = Array.FindIndex(lines, line => 
                !string.IsNullOrWhiteSpace(line) && 
                Regex.IsMatch(line, @"class\s+\w+", RegexOptions.IgnoreCase));
                
            if (classLineIndex >= 0)
            {
                var snippet = string.Join("\n", lines.Skip(classLineIndex).Take(8));
                return snippet.Length <= maxLength ? snippet : snippet.Substring(0, maxLength);
            }

            // Look for function definitions
            var functionLineIndex = Array.FindIndex(lines, line => 
                !string.IsNullOrWhiteSpace(line) && 
                Regex.IsMatch(line, @"function\s+\w+", RegexOptions.IgnoreCase));
                
            if (functionLineIndex >= 0)
            {
                var snippet = string.Join("\n", lines.Skip(functionLineIndex).Take(6));
                return snippet.Length <= maxLength ? snippet : snippet.Substring(0, maxLength);
            }

            // Return first meaningful lines
            var meaningfulLines = lines.Where(line => 
                !string.IsNullOrWhiteSpace(line) && 
                !line.Trim().StartsWith("//") && 
                !line.Trim().StartsWith("/*")).Take(5);
                
            var finalSnippet = string.Join("\n", meaningfulLines);
            
            if (string.IsNullOrEmpty(finalSnippet))
            {
                // Fallback to first few lines if no meaningful content found
                finalSnippet = string.Join("\n", lines.Take(3).Where(l => !string.IsNullOrWhiteSpace(l)));
            }
            
            return finalSnippet.Length <= maxLength ? finalSnippet : finalSnippet.Substring(0, maxLength);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting code snippet from content of length {ContentLength}", content.Length);
            return content.Length <= maxLength ? content : content.Substring(0, maxLength);
        }
    }

    private string DetermineBusinessDomain(string readmeContent)
    {
        var content = readmeContent.ToLowerInvariant();

        if (Regex.IsMatch(content, @"game|play|player|score|level|brick|ball|paddle|breakout|arkanoid"))
            return "Game";
        if (Regex.IsMatch(content, @"api|server|backend|endpoint|database|rest|graphql"))
            return "Web API";
        if (Regex.IsMatch(content, @"library|package|utility|tool|framework|sdk"))
            return "Library";
        if (Regex.IsMatch(content, @"website|webapp|frontend|react|vue|angular|dashboard"))
            return "Web Application";
        if (Regex.IsMatch(content, @"cli|command|terminal|console|script"))
            return "CLI Tool";
        if (Regex.IsMatch(content, @"documentation|docs|guide|tutorial|wiki"))
            return "Documentation";

        return "Software";
    }

    private string ExtractUserValue(string readmeContent)
    {
        // Look for sections that explain user benefits
        var valuePatterns = new[]
        {
            @"(?:## Why|## Benefits|## Value)\s*\n((?:.|\n)*?)(?=##|\z)",
            @"(?:helps|allows|enables|provides|offers)\s+([^.]+)",
            @"(?:for|to)\s+([^.]+(?:developers|users|players))"
        };

        foreach (var pattern in valuePatterns)
        {
            var match = Regex.Match(readmeContent, pattern, RegexOptions.IgnoreCase);
            if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
            {
                return CleanTextContent(match.Groups[1].Value);
            }
        }

        return "";
    }

    private List<string> ExtractListItems(string text)
    {
        var items = new List<string>();
        
        // Extract bulleted items
        var bulletMatches = Regex.Matches(text, @"(?:^|\n)\s*[-*+]\s+(.+)", RegexOptions.Multiline);
        foreach (Match match in bulletMatches)
        {
            var item = CleanTextContent(match.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(item))
            {
                items.Add(item);
            }
        }
        
        // Extract numbered items
        var numberMatches = Regex.Matches(text, @"(?:^|\n)\s*\d+\.\s+(.+)", RegexOptions.Multiline);
        foreach (Match match in numberMatches)
        {
            var item = CleanTextContent(match.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(item))
            {
                items.Add(item);
            }
        }

        return items.Distinct().ToList();
    }

    private string CleanTextContent(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        try
        {
            // Remove markdown formatting and clean up text
            var cleaned = Regex.Replace(text, @"[*_`#\[\]()]+", "")
                .Replace("\n", " ")
                .Trim();

            // Limit length based on the cleaned string, not original
            return cleaned.Length <= 200 ? cleaned : cleaned.Substring(0, 200);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning text content of length {TextLength}", text.Length);
            return text.Length <= 200 ? text : text.Substring(0, 200);
        }
    }

    private string DetermineLanguageFromPath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".js" => "javascript",
            ".html" => "html",
            ".css" => "css",
            ".py" => "python",
            ".cs" => "csharp",
            ".java" => "java",
            ".cpp" => "cpp",
            ".c" => "c",
            ".php" => "php",
            ".rb" => "ruby",
            ".go" => "go",
            ".rs" => "rust",
            ".ts" => "typescript",
            ".jsx" => "jsx",
            ".vue" => "vue",
            _ => Path.GetExtension(filePath).TrimStart('.')
        };
    }

    private ContentType DetermineContentType(string filePath, string content)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();

        if (fileName.Contains("test") || fileName.Contains("spec"))
            return ContentType.Test;
        if (fileName.Contains("config") || fileName.Contains("setting"))
            return ContentType.Configuration;
        if (extension == ".md" || extension == ".txt")
            return ContentType.Documentation;
        if (extension == ".html")
            return ContentType.Markup;
        if (extension == ".css")
            return ContentType.Style;
        if (extension == ".json" || extension == ".xml" || extension == ".yml")
            return ContentType.Data;

        return ContentType.SourceCode;
    }

    // Configuration file summarization methods
    private string SummarizePackageJson(string content)
    {
        try
        {
            // Extract key info from package.json structure
            var dependencyCount = Regex.Matches(content, @"""[^""]+"":\s*""[^""]+""").Count;
            var scripts = Regex.Matches(content, @"""scripts""").Count > 0;
            
            return $"Node.js project configuration with {dependencyCount} dependencies" + 
                   (scripts ? " and build scripts" : "");
        }
        catch
        {
            return "Node.js project configuration";
        }
    }

    private string SummarizePomXml(string content) => "Maven project configuration with Java dependencies";
    private string SummarizeRequirements(string content) => $"Python dependencies ({content.Split('\n').Length} packages)";
    private string SummarizeDockerfile(string content) => "Docker container configuration";
    private string SummarizeJsonConfig(string content) => "JSON configuration settings";
    private string SummarizeXmlConfig(string content) => "XML configuration settings";
    private string SummarizeYamlConfig(string content) => "YAML configuration settings";

    // Placeholder methods for other languages
    private string ExtractPythonFunctionality(string content) => ExtractGenericFunctionality(content, "Python");
    private string ExtractCSharpFunctionality(string content) => ExtractGenericFunctionality(content, "C#");
    private string ExtractJavaFunctionality(string content) => ExtractGenericFunctionality(content, "Java");

    #endregion
}