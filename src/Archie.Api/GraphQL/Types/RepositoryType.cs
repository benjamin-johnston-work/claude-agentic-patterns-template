using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Application.UseCases;
using Archie.Domain.ValueObjects;
using Archie.Domain.Entities;
using Archie.Infrastructure.GitHub;
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

        // Documentation fields
        descriptor.Field("documentation")
            .Type<DocumentationType>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var documentationService = context.Service<GetDocumentationUseCase>();
                var result = await documentationService.ExecuteAsync(repository.Id, context.RequestAborted);
                return result.IsSuccess ? result.Value : null;
            });

        descriptor.Field("hasDocumentation")
            .Type<NonNullType<BooleanType>>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var documentationRepo = context.Service<IDocumentationRepository>();
                return await documentationRepo.ExistsForRepositoryAsync(repository.Id, context.RequestAborted);
            });

        descriptor.Field("documentationStatus")
            .Type<DocumentationStatusType>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var documentationService = context.Service<GetDocumentationUseCase>();
                var result = await documentationService.ExecuteAsync(repository.Id, context.RequestAborted);
                return result.IsSuccess && result.Value != null ? result.Value.Status : DocumentationStatus.NotStarted;
            });

        descriptor.Field("documentationLastGenerated")
            .Type<DateTimeType>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var documentationService = context.Service<GetDocumentationUseCase>();
                var result = await documentationService.ExecuteAsync(repository.Id, context.RequestAborted);
                return result.IsSuccess && result.Value != null ? result.Value.GeneratedAt : (DateTime?)null;
            });

        // Knowledge Graph fields
        descriptor.Field("knowledgeGraph")
            .Type<KnowledgeGraphType>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var graphStorage = context.Service<IGraphStorageService>();
                
                try
                {
                    return await graphStorage.GetKnowledgeGraphByRepositoriesAsync(
                        new List<Guid> { repository.Id }, 
                        context.RequestAborted);
                }
                catch
                {
                    return null;
                }
            });

        descriptor.Field("codeEntities")
            .Argument("type", a => a.Type<EntityTypeEnumType>())
            .Argument("limit", a => a.Type<IntType>().DefaultValue(100))
            .Type<ListType<CodeEntityType>>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var graphStorage = context.Service<IGraphStorageService>();
                var entityType = context.ArgumentValue<EntityType?>("type");
                var limit = context.ArgumentValue<int>("limit");
                
                try
                {
                    var entities = await graphStorage.GetEntitiesByRepositoryAsync(repository.Id, context.RequestAborted);
                    
                    if (entityType.HasValue)
                    {
                        entities = entities.Where(e => e.Type == entityType.Value).ToList();
                    }
                    
                    return entities.Take(limit).ToList();
                }
                catch
                {
                    return new List<CodeEntity>();
                }
            });

        descriptor.Field("architecturalPatterns")
            .Type<ListType<ArchitecturalPatternType>>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var graphStorage = context.Service<IGraphStorageService>();
                
                try
                {
                    return await graphStorage.GetRepositoryPatternsAsync(repository.Id, context.RequestAborted);
                }
                catch
                {
                    return new List<ArchitecturalPattern>();
                }
            });

        descriptor.Field("complexityScore")
            .Type<FloatType>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var graphStorage = context.Service<IGraphStorageService>();
                
                try
                {
                    var entities = await graphStorage.GetEntitiesByRepositoryAsync(repository.Id, context.RequestAborted);
                    if (entities.Count == 0) return 0.0;
                    
                    return entities.Average(e => (double)e.ComplexityScore);
                }
                catch
                {
                    return 0.0;
                }
            });

        descriptor.Field("architecturalHealth")
            .Type<FloatType>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var graphStorage = context.Service<IGraphStorageService>();
                
                try
                {
                    var patterns = await graphStorage.GetRepositoryPatternsAsync(repository.Id, context.RequestAborted);
                    if (patterns.Count == 0) return 1.0; // Perfect health if no patterns (or no analysis done)
                    
                    var totalPatterns = patterns.Count;
                    var healthyPatterns = patterns.Count(p => !p.HasViolations || !p.HasCriticalViolations);
                    
                    return totalPatterns > 0 ? (double)healthyPatterns / totalPatterns : 1.0;
                }
                catch
                {
                    return 1.0; // Default to healthy
                }
            });

        descriptor.Field("hasKnowledgeGraph")
            .Type<NonNullType<BooleanType>>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var graphStorage = context.Service<IGraphStorageService>();
                
                try
                {
                    var graph = await graphStorage.GetKnowledgeGraphByRepositoriesAsync(
                        new List<Guid> { repository.Id }, 
                        context.RequestAborted);
                    return graph != null && graph.IsReady();
                }
                catch
                {
                    return false;
                }
            });

        descriptor.Field("fileStructure")
            .Type<FileStructureType>()
            .Resolve(async context =>
            {
                var repository = context.Parent<RepositoryDto>();
                var gitHubService = context.Service<IGitHubService>();
                
                try
                {
                    var (owner, repo) = gitHubService.ParseRepositoryUrl(repository.Url);
                    var fileTree = await gitHubService.GetRepositoryTreeAsync(owner, repo, "main", true, null, context.RequestAborted);
                    
                    // Group files into folders and root files
                    var folders = new List<FolderDto>();
                    var rootFiles = new List<FileDto>();
                    
                    foreach (var filePath in fileTree)
                    {
                        var pathParts = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        
                        if (pathParts.Length == 1)
                        {
                            // Root level file
                            rootFiles.Add(new FileDto
                            {
                                Name = pathParts[0],
                                Path = filePath,
                                Size = 0, // Size not available from tree API
                                Language = GetLanguageFromPath(filePath),
                                LastModified = DateTime.UtcNow // Not available from tree API
                            });
                        }
                        else
                        {
                            // File in a folder - we'll organize by first level folder for now
                            var folderName = pathParts[0];
                            var folder = folders.FirstOrDefault(f => f.Name == folderName);
                            
                            if (folder == null)
                            {
                                folder = new FolderDto
                                {
                                    Name = folderName,
                                    Path = folderName,
                                    Files = new List<FileDto>(),
                                    Subfolders = new List<SubfolderDto>()
                                };
                                folders.Add(folder);
                            }
                            
                            folder.Files.Add(new FileDto
                            {
                                Name = System.IO.Path.GetFileName(filePath),
                                Path = filePath,
                                Size = 0,
                                Language = GetLanguageFromPath(filePath),
                                LastModified = DateTime.UtcNow
                            });
                        }
                    }
                    
                    return new FileStructureDto
                    {
                        Folders = folders,
                        Files = rootFiles
                    };
                }
                catch
                {
                    return null;
                }
            });
    }

    private static string GetLanguageFromPath(string filePath)
    {
        var extension = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();
        return extension switch
        {
            ".cs" => "C#",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".py" => "Python",
            ".java" => "Java",
            ".cpp" or ".cc" or ".cxx" => "C++",
            ".c" => "C",
            ".go" => "Go",
            ".rs" => "Rust",
            ".php" => "PHP",
            ".rb" => "Ruby",
            ".swift" => "Swift",
            ".kt" => "Kotlin",
            ".scala" => "Scala",
            ".html" => "HTML",
            ".css" => "CSS",
            ".sql" => "SQL",
            ".sh" => "Shell",
            ".ps1" => "PowerShell",
            ".yml" or ".yaml" => "YAML",
            ".json" => "JSON",
            ".xml" => "XML",
            ".md" => "Markdown",
            _ => "text"
        };
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

public class ValidateRepositoryInputType : InputObjectType<ValidateRepositoryInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<ValidateRepositoryInput> descriptor)
    {
        descriptor.Field(i => i.Url)
            .Type<NonNullType<StringType>>();

        descriptor.Field(i => i.AccessToken)
            .Type<StringType>();
    }
}

public class ValidateRepositoryResultType : ObjectType<ValidateRepositoryResult>
{
    protected override void Configure(IObjectTypeDescriptor<ValidateRepositoryResult> descriptor)
    {
        descriptor.Field(r => r.IsValid)
            .Type<NonNullType<BooleanType>>();
            
        descriptor.Field(r => r.Repository)
            .Type<RepositoryInfoType>();
            
        descriptor.Field(r => r.ErrorMessage)
            .Type<StringType>();
    }
}

public class RepositoryInfoType : ObjectType<RepositoryInfo>
{
    protected override void Configure(IObjectTypeDescriptor<RepositoryInfo> descriptor)
    {
        descriptor.Field(r => r.Name)
            .Type<NonNullType<StringType>>();
            
        descriptor.Field(r => r.Description)
            .Type<StringType>();
            
        descriptor.Field(r => r.Language)
            .Type<NonNullType<StringType>>();
            
        descriptor.Field(r => r.IsPrivate)
            .Type<NonNullType<BooleanType>>();
            
        descriptor.Field(r => r.Branches)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();
    }
}

public class FileStructureType : ObjectType<FileStructureDto>
{
    protected override void Configure(IObjectTypeDescriptor<FileStructureDto> descriptor)
    {
        descriptor.Field(f => f.Folders)
            .Type<NonNullType<ListType<NonNullType<FolderType>>>>();
            
        descriptor.Field(f => f.Files)
            .Type<NonNullType<ListType<NonNullType<FileType>>>>();
    }
}

public class FolderType : ObjectType<FolderDto>
{
    protected override void Configure(IObjectTypeDescriptor<FolderDto> descriptor)
    {
        descriptor.Field(f => f.Name)
            .Type<NonNullType<StringType>>();
            
        descriptor.Field(f => f.Path)
            .Type<NonNullType<StringType>>();
            
        descriptor.Field(f => f.Files)
            .Type<NonNullType<ListType<NonNullType<FileType>>>>();
            
        descriptor.Field(f => f.Subfolders)
            .Type<NonNullType<ListType<NonNullType<SubfolderType>>>>();
    }
}

public class FileType : ObjectType<FileDto>
{
    protected override void Configure(IObjectTypeDescriptor<FileDto> descriptor)
    {
        descriptor.Field(f => f.Name)
            .Type<NonNullType<StringType>>();
            
        descriptor.Field(f => f.Path)
            .Type<NonNullType<StringType>>();
            
        descriptor.Field(f => f.Size)
            .Type<NonNullType<LongType>>();
            
        descriptor.Field(f => f.Language)
            .Type<NonNullType<StringType>>();
            
        descriptor.Field(f => f.LastModified)
            .Type<NonNullType<DateTimeType>>();
    }
}

public class SubfolderType : ObjectType<SubfolderDto>
{
    protected override void Configure(IObjectTypeDescriptor<SubfolderDto> descriptor)
    {
        descriptor.Field(f => f.Name)
            .Type<NonNullType<StringType>>();
            
        descriptor.Field(f => f.Path)
            .Type<NonNullType<StringType>>();
    }
}