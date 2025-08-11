using Archie.Api.GraphQL.Resolvers;
using Archie.Api.GraphQL.Types;
using Archie.Application.Interfaces;
using Archie.Application.UseCases;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Services;
using Archie.Infrastructure.Configuration;
using Archie.Infrastructure.Events;
using Archie.Infrastructure.GitHub;
using Archie.Infrastructure.Repositories;
using Archie.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add local secrets configuration (not tracked by git)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add configuration
builder.Services.Configure<GitOptions>(builder.Configuration.GetSection(GitOptions.SectionName));
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection(GitHubOptions.SectionName));
builder.Services.Configure<AzureSearchOptions>(builder.Configuration.GetSection(AzureSearchOptions.SectionName));
builder.Services.Configure<AzureOpenAIOptions>(builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));
builder.Services.Configure<IndexingOptions>(builder.Configuration.GetSection(IndexingOptions.SectionName));
builder.Services.Configure<DocumentationGenerationSettings>(builder.Configuration.GetSection(DocumentationGenerationSettings.SectionName));

// Add logging
builder.Services.AddLogging();

// Add infrastructure services
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IGitRepositoryService, GitRepositoryService>();
builder.Services.AddScoped<IRepositoryRepository, AzureSearchRepositoryRepository>();
builder.Services.AddScoped<IEventPublisher, InMemoryEventPublisher>();

// Add documentation services
builder.Services.AddScoped<IDocumentationRepository, AzureSearchDocumentationRepository>();
builder.Services.AddScoped<IAIDocumentationGeneratorService, AIDocumentationGeneratorService>();
builder.Services.AddScoped<IRepositoryAnalysisService, RepositoryAnalysisService>();
builder.Services.AddScoped<ContentSummarizationService>();

// Add Azure Search services
builder.Services.AddScoped<IAzureSearchService, AzureSearchService>();
builder.Services.AddScoped<IAzureOpenAIEmbeddingService, AzureOpenAIEmbeddingService>();
builder.Services.AddScoped<ICodeSymbolExtractor, CodeSymbolExtractor>();
builder.Services.AddScoped<FileContentProcessor>();
builder.Services.AddScoped<IRepositoryIndexingService, RepositoryIndexingService>();

// Add use cases
builder.Services.AddScoped<AddRepositoryUseCase>();
builder.Services.AddScoped<GetRepositoryUseCase>();
builder.Services.AddScoped<GetRepositoriesUseCase>();
builder.Services.AddScoped<RefreshRepositoryUseCase>();
builder.Services.AddScoped<RemoveRepositoryUseCase>();
builder.Services.AddScoped<ValidateRepositoryUseCase>();

// Add documentation use cases
builder.Services.AddScoped<GenerateDocumentationUseCase>();
builder.Services.AddScoped<GetDocumentationUseCase>();
builder.Services.AddScoped<UpdateDocumentationSectionUseCase>();
builder.Services.AddScoped<SearchDocumentationUseCase>();
builder.Services.AddScoped<DeleteDocumentationUseCase>();

// Add conversation services and use cases
builder.Services.AddScoped<IConversationRepository, AzureSearchConversationRepository>();
builder.Services.AddScoped<IConversationalAIService, ConversationalAIService>();
builder.Services.AddScoped<IConversationContextService, ConversationContextService>();
builder.Services.AddScoped<StartConversationUseCase>();
builder.Services.AddScoped<ProcessQueryUseCase>();
builder.Services.AddScoped<GetConversationUseCase>();

// Add knowledge graph services
builder.Services.AddScoped<IGraphStorageService, StubGraphStorageService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Archie.Api.GraphQL.Resolvers.Query>()
    .AddMutationType<Mutation>()
    // Configure Dictionary serialization for GraphQL
    .BindRuntimeType<Dictionary<string, object>, AnyType>()
    .BindRuntimeType<IDictionary<string, object>, AnyType>()
    .AddType<RepositoryType>()
    .AddType<RepositoryStatusType>()
    .AddType<BranchType>()
    .AddType<CommitType>()
    .AddType<RepositoryStatisticsType>()
    .AddType<LanguageStatsType>()
    .AddType<AddRepositoryInputType>()
    .AddType<RepositoryFilterInputType>()
    .AddType<ValidateRepositoryInputType>()
    .AddType<ValidateRepositoryResultType>()
    .AddType<RepositoryInfoType>()
    .AddType<SearchableDocumentType>()
    .AddType<DocumentMetadataType>()
    .AddType<SearchResultType>()
    .AddType<SearchResultsType>()
    .AddType<FacetResultType>()
    .AddType<IndexStatusType>()
    .AddType<IndexingStatusType>()
    .AddType<SearchTypeEnum>()
    
    // Add documentation GraphQL types
    .AddType<DocumentationType>()
    .AddType<DocumentationSectionType>()
    .AddType<DocumentationMetadataType>()
    .AddType<DocumentationStatisticsType>()
    .AddType<CodeReferenceType>()
    .AddType<SectionMetadataType>()
    .AddType<DocumentationStatusType>()
    .AddType<DocumentationSectionTypeEnum>()
    .AddType<GenerateDocumentationInputType>()
    .AddType<UpdateDocumentationSectionInputType>()
    .AddType<DocumentationGenerationProgressType>()
    
    // Add conversation GraphQL types
    .AddType<ConversationType>()
    .AddType<ConversationSummaryType>()
    .AddType<ConversationMessageType>()
    .AddType<MessageAttachmentType>()
    .AddType<ConversationContextType>()
    .AddType<ConversationPreferencesType>()
    .AddType<ConversationMetadataType>()
    .AddType<MessageMetadataType>()
    .AddType<ConversationStatusType>()
    .AddType<MessageTypeEnum>()
    .AddType<AttachmentTypeEnum>()
    .AddType<ResponseStyleEnum>()
    
    // Add conversation input types
    .AddType<StartConversationInputType>()
    .AddType<QueryInputType>()
    .AddType<ConversationPreferencesInputType>()
    
    // Add knowledge graph GraphQL types
    .AddType<KnowledgeGraphType>()
    .AddType<GraphStatisticsType>()
    .AddType<GraphMetadataType>()
    .AddType<CodeEntityType>()
    .AddType<EntityLocationType>()
    .AddType<EntityMetadataType>()
    .AddType<EntityAttributeType>()
    .AddType<CodeRelationshipType>()
    .AddType<RelationshipMetadataType>()
    .AddType<ArchitecturalPatternType>()
    .AddType<PatternMetadataType>()
    .AddType<PatternViolationType>()
    .AddType<AntiPatternType>()
    .AddType<GraphStatusType>()
    .AddType<AnalysisDepthType>()
    .AddType<EntityTypeEnumType>()
    .AddType<AccessModifierType>()
    .AddType<AttributeTypeEnumType>()
    .AddType<RelationshipTypeEnumType>()
    .AddType<PatternTypeEnumType>()
    .AddType<ViolationSeverityType>()
    
    .AddTypeExtension<RepositoryQueryResolver>()
    .AddTypeExtension<RepositoryMutationResolver>()
    .AddTypeExtension<SearchQueryResolver>()
    .AddTypeExtension<SearchMutationResolver>()
    .AddTypeExtension<RepositorySearchExtensions>()
    
    // Add documentation GraphQL resolvers
    .AddTypeExtension<DocumentationQueryResolver>()
    .AddTypeExtension<DocumentationMutationResolver>()
    
    // Add knowledge graph GraphQL resolvers  
    .AddTypeExtension<KnowledgeGraphQueryResolver>()
    .AddTypeExtension<KnowledgeGraphMutationResolver>()
    
    // Add conversation GraphQL resolvers
    .AddTypeExtension<ConversationQueryResolver>()
    .AddTypeExtension<ConversationMutationResolver>()
    
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
// Disable HTTPS redirection in development to avoid CORS issues
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGraphQL("/graphql");

// Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));


app.Run();
