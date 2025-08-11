using Archie.Application.DTOs;
using Archie.Domain.ValueObjects;
using HotChocolate.Types;

namespace Archie.Api.GraphQL.Types;

/// <summary>
/// GraphQL input type for starting a conversation
/// </summary>
public class StartConversationInputType : InputObjectType<StartConversationInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<StartConversationInput> descriptor)
    {
        descriptor.Field(x => x.RepositoryIds)
            .Type<NonNullType<ListType<NonNullType<IdType>>>>()
            .Description("List of repository IDs to include in the conversation context");

        descriptor.Field(x => x.Title)
            .Type<StringType>()
            .Description("Optional title for the conversation");

        descriptor.Field(x => x.Domain)
            .Type<StringType>()
            .Description("Domain or context for the conversation");

        descriptor.Field(x => x.Preferences)
            .Type<ConversationPreferencesInputType>()
            .Description("Conversation preferences");
    }
}

/// <summary>
/// GraphQL input type for processing a query within a conversation
/// </summary>
public class QueryInputType : InputObjectType<QueryInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<QueryInput> descriptor)
    {
        descriptor.Field(x => x.ConversationId)
            .Type<NonNullType<IdType>>()
            .Description("ID of the conversation to add the query to");

        descriptor.Field(x => x.Query)
            .Type<NonNullType<StringType>>()
            .Description("The user's query or question");

        descriptor.Field(x => x.IncludeContext)
            .Type<BooleanType>()
            .DefaultValue(true)
            .Description("Whether to include repository context in the response");

        descriptor.Field(x => x.MaxContextItems)
            .Type<IntType>()
            .DefaultValue(5)
            .Description("Maximum number of context items to include");

        descriptor.Field(x => x.ParentMessageId)
            .Type<IdType>()
            .Description("Optional parent message ID for threaded conversations");
    }
}

/// <summary>
/// GraphQL input type for conversation preferences
/// </summary>
public class ConversationPreferencesInputType : InputObjectType<ConversationPreferencesInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<ConversationPreferencesInput> descriptor)
    {
        descriptor.Field(x => x.ResponseStyle)
            .Type<ResponseStyleEnum>()
            .DefaultValue(ResponseStyle.Balanced)
            .Description("Preferred response style");

        descriptor.Field(x => x.IncludeCodeExamples)
            .Type<BooleanType>()
            .DefaultValue(true)
            .Description("Whether to include code examples in responses");

        descriptor.Field(x => x.IncludeReferences)
            .Type<BooleanType>()
            .DefaultValue(true)
            .Description("Whether to include references and links in responses");

        descriptor.Field(x => x.MaxResponseLength)
            .Type<IntType>()
            .DefaultValue(2000)
            .Description("Maximum response length in characters");

        descriptor.Field(x => x.PreferredLanguages)
            .Type<ListType<NonNullType<StringType>>>()
            .Description("Preferred programming languages for responses");
    }
}