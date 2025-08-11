namespace Archie.Infrastructure.AzureSearch.Interfaces;

public interface IAzureOpenAIEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
    Task<bool> ValidateServiceAsync(CancellationToken cancellationToken = default);
}