using Archie.Application.DTOs;
using Archie.Application.UseCases;
using HotChocolate;
using HotChocolate.Types;

namespace Archie.Api.GraphQL.Resolvers;

[ExtendObjectType<Mutation>]
public class RepositoryMutationResolver
{
    public async Task<RepositoryDto> AddRepositoryAsync(
        AddRepositoryInput input,
        AddRepositoryUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(input, cancellationToken);
        if (result.IsFailure)
        {
            throw new GraphQLException(result.Error ?? "Failed to add repository");
        }
        return result.Value!;
    }

    public async Task<RepositoryDto> RefreshRepositoryAsync(
        [ID] Guid id,
        RefreshRepositoryUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            throw new GraphQLException(result.Error ?? "Failed to refresh repository");
        }
        return result.Value!;
    }

    public async Task<bool> RemoveRepositoryAsync(
        [ID] Guid id,
        RemoveRepositoryUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            throw new GraphQLException(result.Error ?? "Failed to remove repository");
        }
        return result.Value;
    }
}