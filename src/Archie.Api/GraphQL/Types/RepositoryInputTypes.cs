using HotChocolate.Types;
using Archie.Application.Interfaces;

namespace Archie.Api.GraphQL.Types;

public class RepositoryFilterInputType : InputObjectType<RepositoryFilter>
{
    protected override void Configure(IInputObjectTypeDescriptor<RepositoryFilter> descriptor)
    {
        descriptor.Description("Filter criteria for repositories");

        descriptor.Field(f => f.Language).Description("Filter by primary programming language");
        descriptor.Field(f => f.Status).Description("Filter by repository status");
        descriptor.Field(f => f.SearchTerm).Description("Search term to match repository name or description");
        descriptor.Field(f => f.HasDocumentation).Description("Filter by whether repository has documentation");
        descriptor.Field(f => f.Skip).Description("Number of records to skip (for pagination)");
        descriptor.Field(f => f.Take).Description("Number of records to take (for pagination)");
    }
}