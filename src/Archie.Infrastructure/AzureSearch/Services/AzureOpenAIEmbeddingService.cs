using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Sockets;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.Configuration;

namespace Archie.Infrastructure.AzureSearch.Services;

/// <summary>
/// Azure OpenAI service implementation for generating text embeddings for semantic search.
/// Includes rate limiting, retry logic, and batch processing capabilities.
/// </summary>
public class AzureOpenAIEmbeddingService : IAzureOpenAIEmbeddingService
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly AzureOpenAIOptions _options;
    private readonly ILogger<AzureOpenAIEmbeddingService> _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly ConcurrentQueue<DateTime> _requestHistory;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    public AzureOpenAIEmbeddingService(
        IOptions<AzureOpenAIOptions> options,
        ILogger<AzureOpenAIEmbeddingService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var endpoint = new Uri(_options.Endpoint);
        var credential = new AzureKeyCredential(_options.ApiKey);
        
        // Use default client options - let SDK handle retries without NetworkTimeout override
        var clientOptions = new AzureOpenAIClientOptions();
        
        _openAIClient = new AzureOpenAIClient(endpoint, credential, clientOptions);
        
        // Initialize rate limiting
        _rateLimitSemaphore = new SemaphoreSlim(_options.MaxBatchSize, _options.MaxBatchSize);
        _requestHistory = new ConcurrentQueue<DateTime>();
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("AZURE OPENAI: Empty text provided for embedding generation");
            return Array.Empty<float>();
        }

        try
        {
            _logger.LogDebug("AZURE OPENAI: Starting embedding generation for text of length {TextLength}", text.Length);

            // Apply rate limiting if enabled
            if (_options.EnableRateLimitProtection)
            {
                _logger.LogDebug("AZURE OPENAI: Applying rate limiting protection");
                await ApplyRateLimitingAsync(cancellationToken);
            }

            // Preprocess text to stay within token limits
            var processedText = PreprocessTextForEmbedding(text);
            _logger.LogDebug("AZURE OPENAI: Text preprocessed from {OriginalLength} to {ProcessedLength} characters", 
                text.Length, processedText.Length);
            
            var embeddingClient = _openAIClient.GetEmbeddingClient(_options.EmbeddingDeploymentName);
            _logger.LogDebug("AZURE OPENAI: Created embedding client for deployment {DeploymentName}", _options.EmbeddingDeploymentName);
            
            _logger.LogInformation("AZURE OPENAI: Calling Azure OpenAI API for embedding generation");
            var response = await ExecuteWithRetryAsync(
                async () => await embeddingClient.GenerateEmbeddingAsync(processedText),
                cancellationToken);

            if (response?.Value != null)
            {
                var embedding = response.Value.ToFloats().ToArray();
                
                _logger.LogInformation("AZURE OPENAI: Successfully generated embedding with {Dimensions} dimensions for text of length {TextLength}",
                    embedding.Length, processedText.Length);
                    
                return embedding;
            }

            _logger.LogError("AZURE OPENAI: No embedding data returned from Azure OpenAI service");
            return Array.Empty<float>();
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "AZURE OPENAI: API error generating embedding - {ErrorCode}: {Message} | Status: {Status} | Details: {Details}", 
                ex.ErrorCode, ex.Message, ex.Status, ex.GetRawResponse()?.Content?.ToString() ?? "No details");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "AZURE OPENAI: Request timed out generating embedding for text of length {TextLength}", text.Length);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AZURE OPENAI: HTTP error generating embedding - {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AZURE OPENAI: Unexpected error generating embedding for text of length {TextLength} - {ExceptionType}: {Message}", 
                text.Length, ex.GetType().Name, ex.Message);
            throw;
        }
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var textList = texts.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        
        if (!textList.Any())
        {
            _logger.LogWarning("No valid texts provided for batch embedding generation");
            return new List<float[]>();
        }

        _logger.LogInformation("Starting batch embedding generation for {TextCount} texts", textList.Count);

        try
        {
            var results = new List<float[]>();
            var batches = textList.Chunk(_options.MaxBatchSize).ToList();

            foreach (var batch in batches)
            {
                var batchResults = await ProcessBatchEmbeddingsAsync(batch, cancellationToken);
                results.AddRange(batchResults);
                
                _logger.LogDebug("Processed batch of {BatchSize} embeddings, total processed: {TotalProcessed}", 
                    batch.Length, results.Count);
            }

            _logger.LogInformation("Successfully generated {EmbeddingCount} embeddings from {TextCount} texts", 
                results.Count, textList.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch embedding generation for {TextCount} texts", textList.Count);
            throw;
        }
    }

    public async Task<bool> ValidateServiceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating Azure OpenAI service connection");

            // Try to generate a simple embedding to verify the service is accessible
            const string testText = "Azure OpenAI service validation test";
            var embedding = await GenerateEmbeddingAsync(testText, cancellationToken);

            var isValid = embedding.Length > 0;
            
            if (isValid)
            {
                _logger.LogInformation("Azure OpenAI service validation successful. Embedding dimensions: {Dimensions}", 
                    embedding.Length);
            }
            else
            {
                _logger.LogError("Azure OpenAI service validation failed: No embedding returned");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI service validation failed with exception");
            return false;
        }
    }

    #region Private Helper Methods

    private async Task<List<float[]>> ProcessBatchEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken)
    {
        var textList = texts.ToList();
        
        try
        {
            // Apply rate limiting if enabled
            if (_options.EnableRateLimitProtection)
            {
                await ApplyRateLimitingAsync(cancellationToken);
            }

            // Preprocess all texts
            var processedTexts = textList.Select(PreprocessTextForEmbedding).ToList();
            
            var embeddingClient = _openAIClient.GetEmbeddingClient(_options.EmbeddingDeploymentName);
            
            var response = await ExecuteWithRetryAsync(
                async () => await embeddingClient.GenerateEmbeddingsAsync(processedTexts),
                cancellationToken);

            if (response?.Value?.Count > 0)
            {
                var embeddings = response.Value
                    .Select(e => e.ToFloats().ToArray())
                    .ToList();

                if (embeddings.Count != textList.Count)
                {
                    _logger.LogWarning("Mismatch between input texts {InputCount} and returned embeddings {EmbeddingCount}",
                        textList.Count, embeddings.Count);
                }

                return embeddings;
            }

            _logger.LogError("No embedding data returned from Azure OpenAI service for batch of {TextCount} texts", 
                textList.Count);
            return new List<float[]>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch of {TextCount} texts for embeddings", textList.Count);
            throw;
        }
    }

    private async Task ApplyRateLimitingAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);

        try
        {
            // Clean up old requests from history
            var cutoffTime = DateTime.UtcNow - RateLimitWindow;
            while (_requestHistory.TryPeek(out var oldestRequest) && oldestRequest < cutoffTime)
            {
                _requestHistory.TryDequeue(out _);
            }

            // Add current request to history
            _requestHistory.Enqueue(DateTime.UtcNow);

            // If we're at the limit, add a small delay
            if (_requestHistory.Count >= _options.MaxBatchSize)
            {
                var delayMs = Math.Min(1000, 100 * _requestHistory.Count); // Scale delay with queue size
                await Task.Delay(delayMs, cancellationToken);
            }
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    private async Task<T?> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken) where T : class
    {
        for (int attempt = 1; attempt <= _options.RetryAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsRetryableException(ex) && attempt < _options.RetryAttempts)
            {
                var delayMs = CalculateExponentialBackoffDelay(attempt);
                
                var errorDetails = ex is RequestFailedException reqEx ? $"{reqEx.ErrorCode} - " : "";
                _logger.LogWarning("Azure OpenAI request failed (attempt {Attempt}/{MaxAttempts}), retrying in {DelayMs}ms: {ErrorDetails}{Message}",
                    attempt, _options.RetryAttempts, delayMs, errorDetails, ex.Message);
                
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        return null;
    }

    private static bool IsRetryableException(Exception ex)
    {
        return ex switch
        {
            RequestFailedException reqEx => reqEx.Status is 429 or 408 or >= 500,
            TaskCanceledException => false, // Don't retry cancellations
            OperationCanceledException => false, // Don't retry cancellations
            HttpRequestException => true, // Retry HTTP issues
            SocketException => true, // Retry network issues
            IOException => true, // Retry I/O issues
            TimeoutException => true, // Retry timeouts
            _ => false
        };
    }

    private static int CalculateExponentialBackoffDelay(int attempt)
    {
        // Exponential backoff with jitter: base delay * 2^attempt + random jitter
        var baseDelay = 1000; // 1 second
        var exponentialDelay = baseDelay * Math.Pow(2, attempt - 1);
        var jitter = Random.Shared.Next(0, 500); // Add up to 500ms jitter
        
        return (int)Math.Min(exponentialDelay + jitter, 30000); // Cap at 30 seconds
    }

    private static string PreprocessTextForEmbedding(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Remove excessive whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

        // Truncate if too long (approximate token limit - 1 token ≈ 4 characters for English)
        // Azure OpenAI text-embedding-ada-002 has a limit of ~8191 tokens
        const int maxChars = 30000; // Conservative limit allowing for token overhead
        
        if (text.Length > maxChars)
        {
            text = text.Substring(0, maxChars);
            
            // Try to cut at a word boundary to avoid cutting words in half
            var lastSpaceIndex = text.LastIndexOf(' ');
            if (lastSpaceIndex > maxChars * 0.9) // Only if we don't lose more than 10%
            {
                text = text.Substring(0, lastSpaceIndex);
            }
        }

        return text;
    }

    #endregion

    public void Dispose()
    {
        _rateLimitSemaphore?.Dispose();
        // AzureOpenAIClient doesn't implement IDisposable in Azure.AI.OpenAI 2.1.0
    }
}