using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;
using Archie.Api;
using Archie.Application.Interfaces;
using Archie.Infrastructure.AzureSearch.Services;
using Moq;

namespace Archie.Integration.Tests;

[TestFixture]
public class Features1To4IntegrationTests : IDisposable
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private GraphQLHttpClient _graphqlClient;
    private string _testRepositoryId;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["GitHub:DefaultAccessToken"] = "test-token",
                        ["AzureSearch:ServiceName"] = "test-search",
                        ["AzureSearch:AdminKey"] = "test-key",
                        ["AzureOpenAI:Endpoint"] = "https://test.openai.azure.com",
                        ["AzureOpenAI:ApiKey"] = "test-ai-key"
                    });
                });
                
                builder.ConfigureServices(services =>
                {
                    // Replace Azure services with test implementations
                    services.AddSingleton<IAzureSearchService, TestAzureSearchService>();
                    services.AddSingleton<IAzureOpenAIEmbeddingService, TestEmbeddingService>();
                });
            });

        _client = _factory.CreateClient();
        _graphqlClient = new GraphQLHttpClient(_client.BaseAddress + "graphql", new NewtonsoftJsonSerializer(), _client);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Dispose();
    }

    public void Dispose()
    {
        _graphqlClient?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Test, Order(1)]
    public async Task F01_RepositoryConnection_AddRepository_Success()
    {
        // Arrange
        var addRepositoryMutation = @"
            mutation AddRepository($input: AddRepositoryInput!) {
                addRepository(input: $input) {
                    id
                    name
                    url
                    owner
                    language
                    description
                    status
                    createdAt
                }
            }";

        var variables = new
        {
            input = new
            {
                name = "integration-test-repo",
                url = "https://github.com/microsoft/TypeScript",
                owner = "microsoft",
                language = "TypeScript",
                description = "Integration test repository"
            }
        };

        var request = new GraphQLRequest
        {
            Query = addRepositoryMutation,
            Variables = variables
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        Assert.That(response.Errors, Is.Null, "GraphQL query should not have errors");
        Assert.That(response.Data.addRepository, Is.Not.Null);
        Assert.That(response.Data.addRepository.name.ToString(), Is.EqualTo("integration-test-repo"));
        Assert.That(response.Data.addRepository.status.ToString(), Is.EqualTo("Connected"));

        _testRepositoryId = response.Data.addRepository.id.ToString();
        Assert.That(_testRepositoryId, Is.Not.Empty);

        Console.WriteLine($"Repository created with ID: {_testRepositoryId}");
    }

    [Test, Order(2)]
    public async Task F01_RepositoryConnection_GetRepository_Success()
    {
        // Arrange
        if (string.IsNullOrEmpty(_testRepositoryId))
        {
            Assert.Ignore("Repository ID not available from previous test");
        }

        var getRepositoryQuery = @"
            query GetRepository($id: ID!) {
                repository(id: $id) {
                    id
                    name
                    url
                    owner
                    language
                    description
                    status
                    indexedAt
                    documentCount
                }
            }";

        var request = new GraphQLRequest
        {
            Query = getRepositoryQuery,
            Variables = new { id = _testRepositoryId }
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        Assert.That(response.Errors, Is.Null);
        Assert.That(response.Data.repository, Is.Not.Null);
        Assert.That(response.Data.repository.id.ToString(), Is.EqualTo(_testRepositoryId));
        Assert.That(response.Data.repository.name.ToString(), Is.EqualTo("integration-test-repo"));
    }

    [Test, Order(3)]
    public async Task F02_AzureSearch_StartIndexing_Success()
    {
        // Arrange
        if (string.IsNullOrEmpty(_testRepositoryId))
        {
            Assert.Ignore("Repository ID not available from previous test");
        }

        var startIndexingMutation = @"
            mutation StartIndexing($repositoryId: ID!) {
                startRepositoryIndexing(repositoryId: $repositoryId) {
                    success
                    message
                    indexingId
                }
            }";

        var request = new GraphQLRequest
        {
            Query = startIndexingMutation,
            Variables = new { repositoryId = _testRepositoryId }
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        Assert.That(response.Errors, Is.Null);
        Assert.That(response.Data.startRepositoryIndexing, Is.Not.Null);
        Assert.That(response.Data.startRepositoryIndexing.success.ToString(), Is.EqualTo("True"));
        Assert.That(response.Data.startRepositoryIndexing.message.ToString(), Does.Contain("started"));

        Console.WriteLine($"Indexing started: {response.Data.startRepositoryIndexing.message}");
    }

    [Test, Order(4)]
    public async Task F02_AzureSearch_MonitorIndexing_CompletesSuccessfully()
    {
        // Arrange
        if (string.IsNullOrEmpty(_testRepositoryId))
        {
            Assert.Ignore("Repository ID not available from previous test");
        }

        var getIndexingStatusQuery = @"
            query GetIndexingStatus($repositoryId: ID!) {
                repository(id: $repositoryId) {
                    id
                    status
                    documentCount
                    indexedAt
                    lastIndexingError
                }
            }";

        // Act & Assert - Monitor indexing progress
        var maxAttempts = 20; // 2 minutes with 6-second intervals
        var indexingCompleted = false;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var request = new GraphQLRequest
            {
                Query = getIndexingStatusQuery,
                Variables = new { repositoryId = _testRepositoryId }
            };

            var response = await _graphqlClient.SendQueryAsync<dynamic>(request);
            Assert.That(response.Errors, Is.Null);

            var repository = response.Data.repository;
            var status = repository.status.ToString();
            var docCount = int.Parse(repository.documentCount?.ToString() ?? "0");

            Console.WriteLine($"Indexing attempt {attempt}: Status = {status}, Documents = {docCount}");

            if (status == "Indexed" && docCount > 0)
            {
                indexingCompleted = true;
                Assert.That(repository.indexedAt, Is.Not.Null);
                Console.WriteLine($"Indexing completed successfully with {docCount} documents");
                break;
            }

            if (status == "IndexingFailed")
            {
                var error = repository.lastIndexingError?.ToString() ?? "Unknown error";
                Assert.Fail($"Indexing failed: {error}");
            }

            await Task.Delay(6000); // Wait 6 seconds before next check
        }

        Assert.That(indexingCompleted, Is.True, "Indexing did not complete within the timeout period");
    }

    [Test, Order(5)]
    public async Task F02_AzureSearch_VectorSearch_ReturnsResults()
    {
        // Arrange
        var searchQuery = @"
            query SearchDocuments($query: String!, $repositoryIds: [ID!]) {
                searchDocuments(query: $query, repositoryIds: $repositoryIds) {
                    totalCount
                    results {
                        id
                        title
                        content
                        filePath
                        repositoryId
                        score
                        highlights {
                            field
                            fragments
                        }
                    }
                }
            }";

        var request = new GraphQLRequest
        {
            Query = searchQuery,
            Variables = new
            {
                query = "typescript interface definition",
                repositoryIds = new[] { _testRepositoryId }
            }
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        Assert.That(response.Errors, Is.Null);
        Assert.That(response.Data.searchDocuments, Is.Not.Null);

        var totalCount = int.Parse(response.Data.searchDocuments.totalCount.ToString());
        Assert.That(totalCount, Is.GreaterThan(0), "Search should return results");

        var results = response.Data.searchDocuments.results;
        Assert.That(results, Is.Not.Empty);

        // Verify first result has required fields
        var firstResult = results[0];
        Assert.That(firstResult.id, Is.Not.Null);
        Assert.That(firstResult.repositoryId.ToString(), Is.EqualTo(_testRepositoryId));
        Assert.That(firstResult.score, Is.Not.Null);

        Console.WriteLine($"Search returned {totalCount} results");
    }

    [Test, Order(6)]
    public async Task F03_DocumentationGeneration_Generate_Success()
    {
        // Arrange
        if (string.IsNullOrEmpty(_testRepositoryId))
        {
            Assert.Ignore("Repository ID not available from previous test");
        }

        var generateDocumentationMutation = @"
            mutation GenerateDocumentation($repositoryId: ID!) {
                generateDocumentation(repositoryId: $repositoryId) {
                    id
                    repositoryId
                    status
                    title
                    sections {
                        id
                        title
                        content
                        type
                        order
                    }
                    totalSections
                    estimatedReadingTime
                    generationDuration
                    sectionsGenerated
                }
            }";

        var request = new GraphQLRequest
        {
            Query = generateDocumentationMutation,
            Variables = new { repositoryId = _testRepositoryId }
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        Assert.That(response.Errors, Is.Null);
        Assert.That(response.Data.generateDocumentation, Is.Not.Null);

        var documentation = response.Data.generateDocumentation;
        Assert.That(documentation.repositoryId.ToString(), Is.EqualTo(_testRepositoryId));
        Assert.That(documentation.status.ToString(), Is.EqualTo("Generated"));
        Assert.That(documentation.title, Is.Not.Null);

        var sections = documentation.sections;
        Assert.That(sections, Is.Not.Empty);

        // Verify frontend UX fields are populated
        var totalSections = int.Parse(documentation.totalSections.ToString());
        var sectionsGenerated = int.Parse(documentation.sectionsGenerated.ToString());
        var readingTime = double.Parse(documentation.estimatedReadingTime.ToString());

        Assert.That(totalSections, Is.GreaterThan(0));
        Assert.That(sectionsGenerated, Is.GreaterThan(0));
        Assert.That(readingTime, Is.GreaterThan(0));

        Console.WriteLine($"Documentation generated: {totalSections} sections, {readingTime:F1} min read");
    }

    [Test, Order(7)]
    public async Task F03_DocumentationGeneration_GetDocumentation_Success()
    {
        // Arrange
        var getDocumentationQuery = @"
            query GetDocumentation($repositoryId: ID!) {
                repository(id: $repositoryId) {
                    id
                    documentation {
                        id
                        title
                        status
                        sections {
                            id
                            title
                            content
                            type
                            order
                        }
                        totalSections
                        estimatedReadingTime
                        lastGenerated
                        generationDuration
                        sectionsGenerated
                    }
                }
            }";

        var request = new GraphQLRequest
        {
            Query = getDocumentationQuery,
            Variables = new { repositoryId = _testRepositoryId }
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        Assert.That(response.Errors, Is.Null);
        Assert.That(response.Data.repository.documentation, Is.Not.Null);

        var documentation = response.Data.repository.documentation;
        Assert.That(documentation.status.ToString(), Is.EqualTo("Generated"));
        Assert.That(documentation.sections, Is.Not.Empty);

        // Verify sections are ordered correctly
        var sections = documentation.sections;
        for (var i = 1; i < sections.Count; i++)
        {
            var currentOrder = int.Parse(sections[i].order.ToString());
            var previousOrder = int.Parse(sections[i - 1].order.ToString());
            Assert.That(currentOrder, Is.GreaterThanOrEqualTo(previousOrder));
        }

        Console.WriteLine($"Retrieved documentation with {sections.Count} sections");
    }

    [Test, Order(8)]
    public async Task F04_ConversationalQuery_SendQuery_Success()
    {
        // Arrange
        var sendMessageMutation = @"
            mutation SendMessage($input: SendMessageInput!) {
                sendMessage(input: $input) {
                    id
                    content
                    role
                    timestamp
                    metadata {
                        sources {
                            title
                            filePath
                            repositoryId
                        }
                        confidence
                        responseTime
                    }
                }
            }";

        var variables = new
        {
            input = new
            {
                conversationId = Guid.NewGuid().ToString(),
                message = "What is the main purpose of this TypeScript repository?",
                repositoryIds = new[] { _testRepositoryId }
            }
        };

        var request = new GraphQLRequest
        {
            Query = sendMessageMutation,
            Variables = variables
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        Assert.That(response.Errors, Is.Null);
        Assert.That(response.Data.sendMessage, Is.Not.Null);

        var message = response.Data.sendMessage;
        Assert.That(message.content, Is.Not.Null);
        Assert.That(message.role.ToString(), Is.EqualTo("Assistant"));
        Assert.That(message.metadata, Is.Not.Null);

        // Verify sources are included
        var sources = message.metadata.sources;
        if (sources?.Count > 0)
        {
            var firstSource = sources[0];
            Assert.That(firstSource.repositoryId.ToString(), Is.EqualTo(_testRepositoryId));
            Assert.That(firstSource.filePath, Is.Not.Null);
        }

        Console.WriteLine($"Conversational query response: {message.content.ToString().Substring(0, Math.Min(100, message.content.ToString().Length))}...");
    }

    [Test, Order(9)]
    public async Task F04_ConversationalQuery_GetConversationHistory_Success()
    {
        // Arrange
        var conversationId = Guid.NewGuid().ToString();

        // First, send a message
        var sendMessageMutation = @"
            mutation SendMessage($input: SendMessageInput!) {
                sendMessage(input: $input) {
                    id
                    conversationId
                }
            }";

        await _graphqlClient.SendQueryAsync<dynamic>(new GraphQLRequest
        {
            Query = sendMessageMutation,
            Variables = new
            {
                input = new
                {
                    conversationId = conversationId,
                    message = "Tell me about the repository structure.",
                    repositoryIds = new[] { _testRepositoryId }
                }
            }
        });

        // Now get conversation history
        var getConversationQuery = @"
            query GetConversation($conversationId: ID!) {
                conversation(id: $conversationId) {
                    id
                    title
                    createdAt
                    updatedAt
                    messages {
                        id
                        content
                        role
                        timestamp
                    }
                }
            }";

        var request = new GraphQLRequest
        {
            Query = getConversationQuery,
            Variables = new { conversationId = conversationId }
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        Assert.That(response.Errors, Is.Null);
        Assert.That(response.Data.conversation, Is.Not.Null);

        var conversation = response.Data.conversation;
        Assert.That(conversation.id.ToString(), Is.EqualTo(conversationId));
        Assert.That(conversation.messages, Is.Not.Empty);

        // Should have at least user message and AI response
        Assert.That(conversation.messages.Count, Is.GreaterThanOrEqualTo(2));

        Console.WriteLine($"Conversation history retrieved with {conversation.messages.Count} messages");
    }

    [Test, Order(10)]
    public async Task Integration_ErrorHandling_InvalidRepository_Returns404()
    {
        // Arrange
        var invalidRepositoryId = Guid.NewGuid().ToString();
        var getRepositoryQuery = @"
            query GetRepository($id: ID!) {
                repository(id: $id) {
                    id
                    name
                }
            }";

        var request = new GraphQLRequest
        {
            Query = getRepositoryQuery,
            Variables = new { id = invalidRepositoryId }
        };

        // Act
        var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

        // Assert
        // GraphQL typically returns null for missing resources rather than HTTP errors
        Assert.That(response.Data.repository, Is.Null);
    }

    [Test, Order(11)]
    public async Task Integration_Performance_MultipleQueries_WithinThresholds()
    {
        // Arrange
        var queries = new[]
        {
            "query { repositories { totalCount } }",
            $"query {{ repository(id: \"{_testRepositoryId}\") {{ id name }} }}",
            "query { searchDocuments(query: \"test\") { totalCount } }"
        };

        // Act & Assert
        foreach (var query in queries)
        {
            var startTime = DateTime.UtcNow;

            var request = new GraphQLRequest { Query = query };
            var response = await _graphqlClient.SendQueryAsync<dynamic>(request);

            var duration = DateTime.UtcNow - startTime;

            Assert.That(response.Errors, Is.Null);
            Assert.That(duration.TotalSeconds, Is.LessThan(5.0), $"Query took too long: {duration.TotalSeconds}s");

            Console.WriteLine($"Query completed in {duration.TotalMilliseconds}ms");
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        // Clean up test repository after each test if needed
        if (!string.IsNullOrEmpty(_testRepositoryId))
        {
            try
            {
                var deleteRepositoryMutation = @"
                    mutation DeleteRepository($id: ID!) {
                        deleteRepository(id: $id) {
                            success
                            message
                        }
                    }";

                var request = new GraphQLRequest
                {
                    Query = deleteRepositoryMutation,
                    Variables = new { id = _testRepositoryId }
                };

                await _graphqlClient.SendQueryAsync<dynamic>(request);
                Console.WriteLine($"Test repository {_testRepositoryId} cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup failed: {ex.Message}");
            }
        }
    }
}

// Test service implementations for mocking Azure dependencies
public class TestAzureSearchService : IAzureSearchService
{
    public Task<bool> IndexExistsAsync(string indexName) => Task.FromResult(true);
    public Task CreateIndexAsync(string indexName, int embeddingDimensions) => Task.CompletedTask;
    public Task<bool> IndexDocumentAsync<T>(string indexName, T document) => Task.FromResult(true);
    public Task<bool> IndexDocumentsBatchAsync<T>(string indexName, IEnumerable<T> documents) => Task.FromResult(true);
    public Task<int> GetDocumentCountAsync(string indexName, string filter = null) => Task.FromResult(100);
    public Task<IEnumerable<T>> SearchAsync<T>(string indexName, string searchText, string filter = null, int top = 10) 
        => Task.FromResult(Enumerable.Empty<T>());
    public Task<bool> DeleteDocumentAsync(string indexName, string documentId) => Task.FromResult(true);
    public Task<bool> DeleteDocumentsByFilterAsync(string indexName, string filter) => Task.FromResult(true);
    public Task<bool> RecreateIndexAsync(string indexName, int embeddingDimensions) => Task.FromResult(true);
}

public class TestEmbeddingService : IAzureOpenAIEmbeddingService
{
    public Task<float[]> GenerateEmbeddingAsync(string text) 
        => Task.FromResult(new float[3072]); // Return dummy 3072-dimensional embedding
}