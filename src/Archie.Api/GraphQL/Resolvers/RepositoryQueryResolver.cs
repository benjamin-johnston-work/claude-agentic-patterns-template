using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Application.UseCases;
using HotChocolate.Types;

namespace Archie.Api.GraphQL.Resolvers;

[ExtendObjectType<Query>]
public class RepositoryQueryResolver
{
    public async Task<RepositoryDto?> GetRepositoryAsync(
        [ID] Guid id,
        GetRepositoryUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<IEnumerable<RepositoryDto>> GetRepositoriesAsync(
        RepositoryFilter? filter,
        GetRepositoriesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(filter, cancellationToken);
        return result.IsSuccess ? result.Value! : Enumerable.Empty<RepositoryDto>();
    }
}

public class Query
{
    // This class serves as a placeholder for the root Query type
    // Individual resolvers extend this using ExtendObjectType
}