using Archie.Application.DTOs;
using Archie.Application.UseCases;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;

namespace Archie.Api.GraphQL.Resolvers;

/// <summary>
/// GraphQL mutation resolver for conversation functionality
/// </summary>
[ExtendObjectType(typeof(Mutation))]
public class ConversationMutationResolver
{
    private readonly ILogger<ConversationMutationResolver> _logger;

    public ConversationMutationResolver(ILogger<ConversationMutationResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Start a new conversation
    /// </summary>
    /// <param name="input">Conversation start input</param>
    /// <param name="useCase">Start conversation use case</param>
    /// <param name="userId">Optional user ID (for future authentication)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created conversation</returns>
    [GraphQLDescription("Start a new conversation")]
    public async Task<ConversationDto> StartConversationAsync(
        StartConversationInput input,
        StartConversationUseCase useCase,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, use a default user ID since authentication is not implemented
            // In a real implementation, this would come from the authenticated user context
            var effectiveUserId = userId ?? Guid.NewGuid(); // Default to new GUID to avoid errors

            _logger.LogInformation("Starting conversation for user: {UserId} with repositories: {RepositoryIds}", 
                effectiveUserId, string.Join(", ", input.RepositoryIds));

            var result = await useCase.ExecuteAsync(input, effectiveUserId, cancellationToken);
            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to start conversation: {Error}", result.Error);
                throw new GraphQLException(result.Error ?? "Failed to start conversation");
            }

            _logger.LogInformation("Successfully started conversation: {ConversationId}", result.Value!.Id);
            return result.Value!;
        }
        catch (GraphQLException)
        {
            throw; // Re-throw GraphQL exceptions as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting conversation");
            throw new GraphQLException($"Failed to start conversation: {ex.Message}");
        }
    }

    /// <summary>
    /// Process a query within a conversation
    /// </summary>
    /// <param name="input">Query input</param>
    /// <param name="useCase">Process query use case</param>
    /// <param name="userId">Optional user ID (for future authentication)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query response</returns>
    [GraphQLDescription("Process a query within a conversation")]
    public async Task<QueryResponseDto> ProcessQueryAsync(
        QueryInput input,
        ProcessQueryUseCase useCase,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, use a default user ID since authentication is not implemented
            var effectiveUserId = userId ?? Guid.NewGuid();

            _logger.LogInformation("Processing query in conversation: {ConversationId} for user: {UserId}", 
                input.ConversationId, effectiveUserId);

            var result = await useCase.ExecuteAsync(input, effectiveUserId, cancellationToken);
            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to process query: {Error}", result.Error);
                throw new GraphQLException(result.Error ?? "Failed to process query");
            }

            _logger.LogInformation("Successfully processed query in conversation: {ConversationId}", input.ConversationId);
            return result.Value!;
        }
        catch (GraphQLException)
        {
            throw; // Re-throw GraphQL exceptions as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query in conversation: {ConversationId}", input.ConversationId);
            throw new GraphQLException($"Failed to process query: {ex.Message}");
        }
    }

}