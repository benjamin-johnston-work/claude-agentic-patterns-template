using Archie.Application.DTOs;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using HotChocolate.Types;

namespace Archie.Api.GraphQL.Types;

/// <summary>
/// GraphQL type for Conversation entity
/// </summary>
public class ConversationType : ObjectType<ConversationDto>
{
    protected override void Configure(IObjectTypeDescriptor<ConversationDto> descriptor)
    {
        descriptor.Field(c => c.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(c => c.UserId)
            .Type<NonNullType<IdType>>();

        descriptor.Field(c => c.Title)
            .Type<NonNullType<StringType>>();

        descriptor.Field(c => c.Status)
            .Type<NonNullType<ConversationStatusType>>();

        descriptor.Field(c => c.Messages)
            .Type<NonNullType<ListType<NonNullType<ConversationMessageType>>>>();

        descriptor.Field(c => c.Context)
            .Type<NonNullType<ConversationContextType>>();

        descriptor.Field(c => c.CreatedAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(c => c.LastActivityAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(c => c.Metadata)
            .Type<NonNullType<ConversationMetadataType>>();
    }
}

/// <summary>
/// GraphQL type for ConversationSummary
/// </summary>
public class ConversationSummaryType : ObjectType<ConversationSummaryDto>
{
    protected override void Configure(IObjectTypeDescriptor<ConversationSummaryDto> descriptor)
    {
        descriptor.Field(c => c.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(c => c.Title)
            .Type<NonNullType<StringType>>();

        descriptor.Field(c => c.Status)
            .Type<NonNullType<ConversationStatusType>>();

        descriptor.Field(c => c.MessageCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(c => c.CreatedAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(c => c.LastActivityAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(c => c.LastMessageAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(c => c.RepositoryId)
            .Type<IdType>();

        descriptor.Field(c => c.RepositoryName)
            .Type<StringType>();

        descriptor.Field(c => c.RepositoryNames)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(c => c.Domain)
            .Type<NonNullType<StringType>>();

        descriptor.Field(c => c.Duration)
            .Type<NonNullType<TimeSpanType>>();

        descriptor.Field(c => c.Metadata)
            .Type<NonNullType<ConversationMetadataType>>();
    }
}

/// <summary>
/// GraphQL type for ConversationMessage
/// </summary>
public class ConversationMessageType : ObjectType<ConversationMessageDto>
{
    protected override void Configure(IObjectTypeDescriptor<ConversationMessageDto> descriptor)
    {
        descriptor.Field(m => m.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(m => m.Type)
            .Type<NonNullType<MessageTypeEnum>>();

        descriptor.Field(m => m.Content)
            .Type<NonNullType<StringType>>();

        descriptor.Field(m => m.Timestamp)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(m => m.Attachments)
            .Type<NonNullType<ListType<NonNullType<MessageAttachmentType>>>>();

        descriptor.Field(m => m.Metadata)
            .Type<NonNullType<MessageMetadataType>>();

        descriptor.Field(m => m.ParentMessageId)
            .Type<IdType>();
    }
}

/// <summary>
/// GraphQL type for MessageAttachment
/// </summary>
public class MessageAttachmentType : ObjectType<MessageAttachmentDto>
{
    protected override void Configure(IObjectTypeDescriptor<MessageAttachmentDto> descriptor)
    {
        descriptor.Field(a => a.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(a => a.Type)
            .Type<NonNullType<AttachmentTypeEnum>>();

        descriptor.Field(a => a.Content)
            .Type<NonNullType<StringType>>();

        descriptor.Field(a => a.Title)
            .Type<NonNullType<StringType>>();

        descriptor.Field(a => a.Properties)
            .Type<NonNullType<AnyType>>();
    }
}

/// <summary>
/// GraphQL type for ConversationContext
/// </summary>
public class ConversationContextType : ObjectType<ConversationContextDto>
{
    protected override void Configure(IObjectTypeDescriptor<ConversationContextDto> descriptor)
    {
        descriptor.Field(c => c.RepositoryIds)
            .Type<NonNullType<ListType<NonNullType<IdType>>>>();

        descriptor.Field(c => c.RepositoryNames)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(c => c.Domain)
            .Type<NonNullType<StringType>>();

        descriptor.Field(c => c.TechnicalTags)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(c => c.SessionData)
            .Type<NonNullType<AnyType>>();

        descriptor.Field(c => c.Preferences)
            .Type<NonNullType<ConversationPreferencesType>>();
    }
}

/// <summary>
/// GraphQL type for ConversationPreferences
/// </summary>
public class ConversationPreferencesType : ObjectType<ConversationPreferencesDto>
{
    protected override void Configure(IObjectTypeDescriptor<ConversationPreferencesDto> descriptor)
    {
        descriptor.Field(p => p.ResponseStyle)
            .Type<NonNullType<ResponseStyleEnum>>();

        descriptor.Field(p => p.IncludeCodeExamples)
            .Type<NonNullType<BooleanType>>();

        descriptor.Field(p => p.IncludeReferences)
            .Type<NonNullType<BooleanType>>();

        descriptor.Field(p => p.MaxResponseLength)
            .Type<NonNullType<IntType>>();

        descriptor.Field(p => p.PreferredLanguages)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();
    }
}

/// <summary>
/// GraphQL type for ConversationMetadata
/// </summary>
public class ConversationMetadataType : ObjectType<ConversationMetadataDto>
{
    protected override void Configure(IObjectTypeDescriptor<ConversationMetadataDto> descriptor)
    {
        descriptor.Field(m => m.MessageCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.UserMessageCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.AIMessageCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.SystemMessageCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.TotalAttachmentCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.TotalWordCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.AverageResponseTimeSeconds)
            .Type<NonNullType<FloatType>>();

        descriptor.Field(m => m.LastUserActivity)
            .Type<DateTimeType>();

        descriptor.Field(m => m.LastAIActivity)
            .Type<DateTimeType>();

        descriptor.Field(m => m.TopicFrequency)
            .Type<NonNullType<AnyType>>();

        descriptor.Field(m => m.Tags)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(m => m.Summary)
            .Type<NonNullType<StringType>>();

        descriptor.Field(m => m.Participants)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();
    }
}

/// <summary>
/// GraphQL type for MessageMetadata
/// </summary>
public class MessageMetadataType : ObjectType<MessageMetadataDto>
{
    protected override void Configure(IObjectTypeDescriptor<MessageMetadataDto> descriptor)
    {
        descriptor.Field(m => m.MessageType)
            .Type<NonNullType<MessageTypeEnum>>();

        descriptor.Field(m => m.WordCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.AttachmentCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.ResponseTimeSeconds)
            .Type<FloatType>();

        descriptor.Field(m => m.ConfidenceScore)
            .Type<FloatType>();

        descriptor.Field(m => m.IsEdited)
            .Type<NonNullType<BooleanType>>();

        descriptor.Field(m => m.EditedAt)
            .Type<DateTimeType>();

        descriptor.Field(m => m.Topics)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(m => m.ExtractedEntities)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();
    }
}

// Enum types
/// <summary>
/// GraphQL enum type for ConversationStatus
/// </summary>
public class ConversationStatusType : EnumType<ConversationStatus>
{
    protected override void Configure(IEnumTypeDescriptor<ConversationStatus> descriptor)
    {
        descriptor.Value(ConversationStatus.Active);
        descriptor.Value(ConversationStatus.Archived);
        descriptor.Value(ConversationStatus.Deleted);
    }
}

/// <summary>
/// GraphQL enum type for MessageType
/// </summary>
public class MessageTypeEnum : EnumType<MessageType>
{
    protected override void Configure(IEnumTypeDescriptor<MessageType> descriptor)
    {
        descriptor.Value(MessageType.UserQuery);
        descriptor.Value(MessageType.AIResponse);
        descriptor.Value(MessageType.SystemMessage);
        descriptor.Value(MessageType.CodeReference);
        descriptor.Value(MessageType.SearchResult);
    }
}

/// <summary>
/// GraphQL enum type for AttachmentType
/// </summary>
public class AttachmentTypeEnum : EnumType<AttachmentType>
{
    protected override void Configure(IEnumTypeDescriptor<AttachmentType> descriptor)
    {
        descriptor.Value(AttachmentType.CodeReference);
        descriptor.Value(AttachmentType.DocumentationReference);
        descriptor.Value(AttachmentType.SearchResult);
        descriptor.Value(AttachmentType.DiagramReference);
        descriptor.Value(AttachmentType.FileReference);
    }
}

/// <summary>
/// GraphQL enum type for ResponseStyle
/// </summary>
public class ResponseStyleEnum : EnumType<ResponseStyle>
{
    protected override void Configure(IEnumTypeDescriptor<ResponseStyle> descriptor)
    {
        descriptor.Value(ResponseStyle.Concise);
        descriptor.Value(ResponseStyle.Balanced);
        descriptor.Value(ResponseStyle.Detailed);
        descriptor.Value(ResponseStyle.Tutorial);
    }
}