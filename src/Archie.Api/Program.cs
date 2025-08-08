using Archie.Api.GraphQL.Resolvers;
using Archie.Api.GraphQL.Types;
using Archie.Application.Interfaces;
using Archie.Application.UseCases;
using Archie.Infrastructure.Configuration;
using Archie.Infrastructure.Events;
using Archie.Infrastructure.Repositories;
using Archie.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<GitOptions>(builder.Configuration.GetSection(GitOptions.SectionName));
builder.Services.Configure<Neo4jOptions>(builder.Configuration.GetSection(Neo4jOptions.SectionName));

// Add logging
builder.Services.AddLogging();

// Add infrastructure services
builder.Services.AddScoped<IGitRepositoryService, GitRepositoryService>();
builder.Services.AddScoped<IRepositoryRepository, Neo4jRepositoryRepository>();
builder.Services.AddScoped<IEventPublisher, InMemoryEventPublisher>();

// Add use cases
builder.Services.AddScoped<AddRepositoryUseCase>();
builder.Services.AddScoped<GetRepositoryUseCase>();
builder.Services.AddScoped<GetRepositoriesUseCase>();
builder.Services.AddScoped<RefreshRepositoryUseCase>();
builder.Services.AddScoped<RemoveRepositoryUseCase>();

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
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<RepositoryType>()
    .AddType<RepositoryStatusType>()
    .AddType<BranchType>()
    .AddType<CommitType>()
    .AddType<RepositoryStatisticsType>()
    .AddType<LanguageStatsType>()
    .AddType<AddRepositoryInputType>()
    .AddTypeExtension<RepositoryQueryResolver>()
    .AddTypeExtension<RepositoryMutationResolver>();

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
