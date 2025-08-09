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

// Add documentation use cases
builder.Services.AddScoped<GenerateDocumentationUseCase>();
builder.Services.AddScoped<GetDocumentationUseCase>();
builder.Services.AddScoped<UpdateDocumentationSectionUseCase>();
builder.Services.AddScoped<SearchDocumentationUseCase>();
builder.Services.AddScoped<DeleteDocumentationUseCase>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Archie.Api.GraphQL.Resolvers.Query>()
    .AddMutationType<Mutation>()
    .AddType<RepositoryType>()
    .AddType<RepositoryStatusType>()
    .AddType<BranchType>()
    .AddType<CommitType>()
    .AddType<RepositoryStatisticsType>()
    .AddType<LanguageStatsType>()
    .AddType<AddRepositoryInputType>()
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
    
    .AddTypeExtension<RepositoryQueryResolver>()
    .AddTypeExtension<RepositoryMutationResolver>()
    .AddTypeExtension<SearchQueryResolver>()
    .AddTypeExtension<SearchMutationResolver>()
    .AddTypeExtension<RepositorySearchExtensions>()
    
    // Add documentation GraphQL resolvers
    .AddTypeExtension<DocumentationQueryResolver>()
    .AddTypeExtension<DocumentationMutationResolver>()
    
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapGraphQL("/graphql");

// Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));


app.Run();
