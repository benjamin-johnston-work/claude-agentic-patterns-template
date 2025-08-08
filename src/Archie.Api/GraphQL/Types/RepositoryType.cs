using Archie.Application.DTOs;
using HotChocolate.Types;

namespace Archie.Api.GraphQL.Types;

public class RepositoryType : ObjectType<RepositoryDto>
{
    protected override void Configure(IObjectTypeDescriptor<RepositoryDto> descriptor)
    {
        descriptor.Field(r => r.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(r => r.Name)
            .Type<NonNullType<StringType>>();

        descriptor.Field(r => r.Url)
            .Type<NonNullType<StringType>>();

        descriptor.Field(r => r.Language)
            .Type<NonNullType<StringType>>();

        descriptor.Field(r => r.Description)
            .Type<StringType>();

        descriptor.Field(r => r.Status)
            .Type<NonNullType<RepositoryStatusType>>();

        descriptor.Field(r => r.Branches)
            .Type<NonNullType<ListType<NonNullType<BranchType>>>>();

        descriptor.Field(r => r.Statistics)
            .Type<RepositoryStatisticsType>();

        descriptor.Field(r => r.CreatedAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(r => r.UpdatedAt)
            .Type<NonNullType<DateTimeType>>();
    }
}

public class RepositoryStatusType : EnumType<RepositoryStatusEnum>
{
    protected override void Configure(IEnumTypeDescriptor<RepositoryStatusEnum> descriptor)
    {
        descriptor.Value(RepositoryStatusEnum.Connecting);
        descriptor.Value(RepositoryStatusEnum.Connected);
        descriptor.Value(RepositoryStatusEnum.Analyzing);
        descriptor.Value(RepositoryStatusEnum.Ready);
        descriptor.Value(RepositoryStatusEnum.Error);
        descriptor.Value(RepositoryStatusEnum.Disconnected);
    }
}

public enum RepositoryStatusEnum
{
    Connecting,
    Connected,
    Analyzing,
    Ready,
    Error,
    Disconnected
}

public class BranchType : ObjectType<BranchDto>
{
    protected override void Configure(IObjectTypeDescriptor<BranchDto> descriptor)
    {
        descriptor.Field(b => b.Name)
            .Type<NonNullType<StringType>>();

        descriptor.Field(b => b.IsDefault)
            .Type<NonNullType<BooleanType>>();

        descriptor.Field(b => b.LastCommit)
            .Type<CommitType>();

        descriptor.Field(b => b.CreatedAt)
            .Type<NonNullType<DateTimeType>>();
    }
}

public class CommitType : ObjectType<CommitDto>
{
    protected override void Configure(IObjectTypeDescriptor<CommitDto> descriptor)
    {
        descriptor.Field(c => c.Hash)
            .Type<NonNullType<StringType>>();

        descriptor.Field(c => c.Message)
            .Type<NonNullType<StringType>>();

        descriptor.Field(c => c.Author)
            .Type<NonNullType<StringType>>();

        descriptor.Field(c => c.Timestamp)
            .Type<NonNullType<DateTimeType>>();
    }
}

public class RepositoryStatisticsType : ObjectType<RepositoryStatisticsDto>
{
    protected override void Configure(IObjectTypeDescriptor<RepositoryStatisticsDto> descriptor)
    {
        descriptor.Field(s => s.FileCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(s => s.LineCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(s => s.LanguageBreakdown)
            .Type<NonNullType<ListType<NonNullType<LanguageStatsType>>>>();
    }
}

public class LanguageStatsType : ObjectType<LanguageStatsDto>
{
    protected override void Configure(IObjectTypeDescriptor<LanguageStatsDto> descriptor)
    {
        descriptor.Field(l => l.Language)
            .Type<NonNullType<StringType>>();

        descriptor.Field(l => l.FileCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(l => l.LineCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(l => l.Percentage)
            .Type<NonNullType<FloatType>>();
    }
}

public class AddRepositoryInputType : InputObjectType<AddRepositoryInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<AddRepositoryInput> descriptor)
    {
        descriptor.Field(i => i.Url)
            .Type<NonNullType<StringType>>();

        descriptor.Field(i => i.AccessToken)
            .Type<StringType>();

        descriptor.Field(i => i.Branch)
            .Type<StringType>();
    }
}