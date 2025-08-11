# Feature 12: GraphRAG Knowledge Construction & Query Engine

## Feature Overview

**Feature ID**: F12  
**Feature Name**: Microsoft GraphRAG Knowledge Construction & Query Engine  
**Phase**: Phase 6 (GraphRAG Core Engine - Backend)  
**Dependencies**: Feature 05 (Semantic Kernel Code Analysis Foundation)
**Type**: Core GraphRAG Engine - Delivers 3x-90x Accuracy Improvement

### Business Value Proposition
Implements Microsoft's official GraphRAG methodology for knowledge graph construction and querying, delivering 3x to 90x accuracy improvement over vanilla RAG approaches for complex relationship queries. This feature transforms the code entity and relationship data from Feature 05 into a comprehensive knowledge graph using **Azure Databricks Delta Lake** for enterprise-grade storage with built-in versioning, ACID transactions, and community detection algorithms, enabling DeepWiki-like conversational code exploration with enterprise-grade accuracy and Australian data sovereignty.

### User Impact
- **Development Teams**: Access conversational AI that understands complex code relationships with unprecedented accuracy (3x-90x better than traditional search)
- **Technical Architects**: Query architectural patterns and dependencies with global/local GraphRAG search capabilities
- **Code Reviewers**: Understand change impacts through multi-hop relationship queries and community-based analysis
- **New Team Members**: Explore codebases through GraphRAG-powered questions like "How does authentication work?" or "Show me all security patterns"
- **Engineering Managers**: Gain architectural insights through community detection and pattern analysis across repositories
- **Enterprise Users**: Benefit from Australian data sovereignty and compliance while accessing superior AI accuracy

### Success Criteria
- **GraphRAG Accuracy**: Achieve 3x-90x accuracy improvement over vanilla RAG for complex relationship queries
- **Knowledge Graph Construction**: Build comprehensive graphs with 95% entity coverage and 85% relationship accuracy
- **Query Performance**: Global search <3 seconds, Local search <1 second for typical repositories
- **Community Detection**: Identify 90% of architectural patterns and component groupings automatically
- **Australian Sovereignty**: All GraphRAG processing within Australia East region with full compliance
- **Delta Lake Storage**: Enterprise-grade ACID storage with built-in versioning and time travel capabilities
- **Integration Success**: Seamless integration with Feature 05 analysis data and Feature 14 visualization

### Dependencies
- **Feature 05**: Requires completed Semantic Kernel code analysis with extracted entities and relationships
- **Microsoft GraphRAG Library**: Official Microsoft GraphRAG implementation for .NET 9
- **Azure Databricks**: Scalable compute platform with Delta Lake integration for GraphRAG processing
- **Azure OpenAI**: Community detection and global/local search query processing
- **Azure AI Search**: Vector similarity search integration (NOT for graph relationships)
- **Australian Infrastructure**: All processing within Australia East region

## Technical Specification

### Architecture Overview

#### Microsoft GraphRAG Architecture Implementation
This feature implements Microsoft's official GraphRAG patterns with **Azure Databricks Delta Lake** storage and in-memory processing:

```mermaid
graph TB
    A[Feature 05: Code Entities & Relationships] --> B[GraphRAG Knowledge Constructor]
    
    B --> C[Community Detection]
    B --> D[Entity Processing]
    B --> E[Relationship Processing]
    
    C --> F[communities (Delta Table)]
    D --> G[entities (Delta Table)]
    E --> H[relationships (Delta Table)]
    
    F --> I[In-Memory Graph Engine]
    G --> I
    H --> I
    
    I --> J[Global GraphRAG Search]
    I --> K[Local GraphRAG Search]
    
    J --> L[Community-Based Insights]
    K --> M[Entity-Specific Queries]
    
    N[Azure AI Search Vector Store] --> I
    I --> O[GraphRAG Query Results]
    
    P[Australian Processing Zone] --> B
    P --> I
```

#### GraphRAG Processing Pipeline
1. **Entity Aggregation**: Collect code entities and relationships from Feature 05 analysis
2. **Community Detection**: Use Microsoft GraphRAG algorithms to identify architectural patterns and component groups
3. **Delta Lake Storage**: Store entities, relationships, and communities in ACID-compliant tables with versioning
4. **Graph Construction**: Build in-memory graph structures for traversal and query processing
5. **Global Search Setup**: Prepare community summaries for high-level architectural queries
6. **Local Search Setup**: Index entity networks for specific code exploration queries
7. **Vector Integration**: Connect with Azure AI Search for semantic similarity augmentation

#### Australian Data Sovereignty GraphRAG Architecture
- **Delta Lake Storage Australia East**: All knowledge graph data stored within Australian region with ACID compliance
- **In-Memory Processing Australia East**: Graph construction and queries processed locally
- **Azure OpenAI Australia East**: Community detection and query processing via Australian endpoints
- **Compliance**: Full ACSC Essential 8 and Australian Privacy Principles adherence

### Domain Model - Microsoft GraphRAG Implementation

```csharp
namespace Archie.Domain.GraphRAG
{
    // Main aggregate for GraphRAG knowledge graphs
    public class KnowledgeGraph
    {
        public Guid Id { get; private set; }
        public Guid RepositoryId { get; private set; }
        public GraphStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }
        public GraphRAGMetrics Metrics { get; private set; }
        public GraphConfiguration Configuration { get; private set; }
        public List<GraphError> Errors { get; private set; } = new();

        public static KnowledgeGraph Create(Guid repositoryId, GraphConfiguration configuration);
        public void UpdateStatus(GraphStatus status);
        public void UpdateMetrics(GraphRAGMetrics metrics);
        public void AddError(GraphError error);
        public bool IsQueryReady() => Status == GraphStatus.Ready && Metrics.CommunityCount > 0;
    }

    public enum GraphStatus
    {
        NotBuilt,
        BuildingCommunities,
        StoringDeltaTables,
        LoadingInMemory,
        Ready,
        Failed,
        RequiresUpdate
    }

    // GraphRAG Community (Microsoft pattern)
    public class GraphCommunity
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Summary { get; private set; }
        public List<string> EntityIds { get; private set; } = new();
        public int Level { get; private set; }
        public double Rank { get; private set; }
        public CommunityMetadata Metadata { get; private set; }
        public DateTime DetectedAt { get; private set; }

        public static GraphCommunity Create(string title, string summary, int level);
        public void AddEntity(string entityId);
        public void UpdateRank(double rank);
    }

    // GraphRAG Entity (extends Feature 05 CodeEntity for GraphRAG)
    public class GraphRAGEntity
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public EntityType Type { get; private set; }
        public List<string> TextUnits { get; private set; } = new();
        public Dictionary<string, object> Attributes { get; private set; } = new();
        public float[] Embeddings { get; private set; }
        public double Rank { get; private set; }
        public List<string> CommunityIds { get; private set; } = new();

        public static GraphRAGEntity FromCodeEntity(CodeEntity codeEntity);
        public void UpdateDescription(string description);
        public void AddToCommunity(string communityId);
    }

    // GraphRAG Relationship (extends Feature 05 CodeRelationship for GraphRAG)
    public class GraphRAGRelationship
    {
        public string Id { get; private set; }
        public string Source { get; private set; }
        public string Target { get; private set; }
        public string Description { get; private set; }
        public double Weight { get; private set; }
        public RelationshipType Type { get; private set; }
        public List<string> TextUnits { get; private set; } = new();
        public Dictionary<string, object> Attributes { get; private set; } = new();
        public double Rank { get; private set; }

        public static GraphRAGRelationship FromCodeRelationship(CodeRelationship codeRelationship);
        public void UpdateDescription(string description);
        public void UpdateWeight(double weight);
    }

    // GraphRAG Query Types
    public class GraphRAGQuery
    {
        public string QueryId { get; private set; }
        public string Query { get; private set; }
        public GraphRAGQueryType Type { get; private set; }
        public SearchScope Scope { get; private set; }
        public QueryContext Context { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public static GraphRAGQuery CreateGlobalQuery(string query, QueryContext context);
        public static GraphRAGQuery CreateLocalQuery(string query, string entityId, QueryContext context);
    }

    public enum GraphRAGQueryType
    {
        GlobalSearch,    // Community-based high-level queries
        LocalSearch,     // Entity-specific detailed queries
        HybridSearch     // Combination of global and local
    }

    public class GraphRAGResult
    {
        public string QueryId { get; private set; }
        public string Response { get; private set; }
        public List<GraphRAGContext> Contexts { get; private set; } = new();
        public List<GraphRAGSource> Sources { get; private set; } = new();
        public double Confidence { get; private set; }
        public TimeSpan ProcessingTime { get; private set; }
        public GraphRAGQueryType QueryType { get; private set; }

        public void AddContext(GraphRAGContext context);
        public void AddSource(GraphRAGSource source);
    }

    public class GraphRAGContext
    {
        public string CommunityId { get; set; }
        public string EntityId { get; set; }
        public string Content { get; set; }
        public double Relevance { get; set; }
        public ContextType Type { get; set; }
    }

    public enum ContextType
    {
        CommunityReport,
        EntityDescription,
        RelationshipDescription,
        SourceCode,
        Documentation
    }

    public class GraphRAGSource
    {
        public string EntityId { get; set; }
        public string FilePath { get; set; }
        public int? StartLine { get; set; }
        public int? EndLine { get; set; }
        public string CodeSnippet { get; set; }
        public double Relevance { get; set; }
    }

    // GraphRAG Metrics
    public class GraphRAGMetrics
    {
        public int EntityCount { get; set; }
        public int RelationshipCount { get; set; }
        public int CommunityCount { get; set; }
        public int MaxCommunityLevel { get; set; }
        public Dictionary<int, int> CommunityDistribution { get; set; } = new(); // Level -> Count
        public TimeSpan ConstructionTime { get; set; }
        public long DeltaTableStorageSize { get; set; }
        public DateTime LastUpdated { get; set; }
        public double AverageEntityRank { get; set; }
        public double AverageRelationshipWeight { get; set; }
    }

    // Configuration for GraphRAG construction
    public class GraphConfiguration
    {
        public int MaxCommunityLevels { get; set; } = 3;
        public double MinCommunitySize { get; set; } = 5;
        public double EntityRankThreshold { get; set; } = 0.1;
        public int MaxTextUnitsPerEntity { get; set; } = 10;
        public bool EnableHierarchicalCommunities { get; set; } = true;
        public bool EnableGlobalSearch { get; set; } = true;
        public bool EnableLocalSearch { get; set; } = true;
        public string DeltaTableCatalog { get; set; } = "archie_graphrag";
    }

    public class GraphError
    {
        public string Message { get; set; }
        public string Component { get; set; }
        public DateTime Timestamp { get; set; }
        public ErrorSeverity Severity { get; set; }
        public Dictionary<string, object> Context { get; set; } = new();
    }
}
```

### Application Layer - Microsoft GraphRAG Services

#### GraphRAG Knowledge Construction Service
```csharp
namespace Archie.Application.GraphRAG
{
    public interface IGraphRAGKnowledgeConstructionService
    {
        Task<KnowledgeGraph> BuildKnowledgeGraphAsync(
            Guid repositoryId,
            GraphConfiguration configuration = null,
            CancellationToken cancellationToken = default);
            
        Task<KnowledgeGraph> UpdateKnowledgeGraphAsync(
            Guid knowledgeGraphId,
            CancellationToken cancellationToken = default);
            
        Task<List<GraphCommunity>> DetectCommunitiesAsync(
            List<GraphRAGEntity> entities,
            List<GraphRAGRelationship> relationships,
            CancellationToken cancellationToken = default);
            
        Task<bool> StoreDeltaTablesAsync(
            KnowledgeGraph knowledgeGraph,
            List<GraphRAGEntity> entities,
            List<GraphRAGRelationship> relationships,
            List<GraphCommunity> communities,
            CancellationToken cancellationToken = default);
    }

    public class GraphRAGKnowledgeConstructionService : IGraphRAGKnowledgeConstructionService
    {
        private readonly IGraphRAGEngine _graphRAGEngine;
        private readonly ICommunityDetectionService _communityDetection;
        private readonly IDatabricksService _databricks;
        private readonly ISemanticKernelCodeAnalysisService _codeAnalysis;
        private readonly IAzureOpenAIService _azureOpenAI;
        private readonly ILogger<GraphRAGKnowledgeConstructionService> _logger;

        public async Task<KnowledgeGraph> BuildKnowledgeGraphAsync(
            Guid repositoryId,
            GraphConfiguration configuration = null,
            CancellationToken cancellationToken = default)
        {
            configuration ??= new GraphConfiguration();
            var knowledgeGraph = KnowledgeGraph.Create(repositoryId, configuration);
            
            try
            {
                _logger.LogInformation("Starting GraphRAG knowledge graph construction for repository {RepositoryId}", repositoryId);
                
                // Phase 1: Load Feature 05 analysis results
                var codeEntities = await _codeAnalysis.GetExtractedEntitiesAsync(repositoryId, cancellationToken: cancellationToken);
                var codeRelationships = await _codeAnalysis.GetDetectedRelationshipsAsync(repositoryId, cancellationToken: cancellationToken);
                
                if (!codeEntities.Any())
                {
                    throw new InvalidOperationException($"No code entities found for repository {repositoryId}. Feature 05 analysis must be completed first.");
                }

                // Phase 2: Transform to GraphRAG entities
                var graphEntities = codeEntities.Select(GraphRAGEntity.FromCodeEntity).ToList();
                var graphRelationships = codeRelationships.Select(GraphRAGRelationship.FromCodeRelationship).ToList();

                // Phase 3: Community detection using Microsoft GraphRAG algorithms
                knowledgeGraph.UpdateStatus(GraphStatus.BuildingCommunities);
                var communities = await DetectCommunitiesAsync(graphEntities, graphRelationships, cancellationToken);
                
                // Phase 4: Store in Delta Lake tables (Enterprise GraphRAG pattern)
                knowledgeGraph.UpdateStatus(GraphStatus.StoringDeltaTables);
                await StoreDeltaTablesAsync(knowledgeGraph, graphEntities, graphRelationships, communities, cancellationToken);
                
                // Phase 5: Load into in-memory graph engine
                knowledgeGraph.UpdateStatus(GraphStatus.LoadingInMemory);
                await _graphRAGEngine.LoadKnowledgeGraphAsync(knowledgeGraph.Id, cancellationToken);
                
                // Phase 6: Calculate metrics and complete
                var metrics = CalculateGraphRAGMetrics(graphEntities, graphRelationships, communities);
                knowledgeGraph.UpdateMetrics(metrics);
                knowledgeGraph.UpdateStatus(GraphStatus.Ready);
                
                _logger.LogInformation("GraphRAG knowledge graph construction completed for repository {RepositoryId}. " +
                    "Entities: {EntityCount}, Relationships: {RelationshipCount}, Communities: {CommunityCount}",
                    repositoryId, metrics.EntityCount, metrics.RelationshipCount, metrics.CommunityCount);
                
                return knowledgeGraph;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GraphRAG knowledge graph construction failed for repository {RepositoryId}", repositoryId);
                knowledgeGraph.UpdateStatus(GraphStatus.Failed);
                knowledgeGraph.AddError(new GraphError
                {
                    Message = ex.Message,
                    Component = "KnowledgeGraphConstruction",
                    Timestamp = DateTime.UtcNow,
                    Severity = ErrorSeverity.Critical
                });
                return knowledgeGraph;
            }
        }

        public async Task<List<GraphCommunity>> DetectCommunitiesAsync(
            List<GraphRAGEntity> entities,
            List<GraphRAGRelationship> relationships,
            CancellationToken cancellationToken = default)
        {
            // Use Microsoft GraphRAG community detection algorithms
            var communities = new List<GraphCommunity>();
            
            // Build adjacency matrix for community detection
            var entityGraph = BuildEntityGraph(entities, relationships);
            
            // Hierarchical community detection (Microsoft pattern)
            for (int level = 0; level < 3; level++) // Max 3 levels as per Microsoft recommendations
            {
                var levelCommunities = await _communityDetection.DetectCommunitiesAtLevelAsync(
                    entityGraph, 
                    level, 
                    cancellationToken);
                    
                communities.AddRange(levelCommunities);
            }
            
            // Generate community reports using Azure OpenAI
            foreach (var community in communities)
            {
                var communityEntities = entities.Where(e => community.EntityIds.Contains(e.Id)).ToList();
                var summary = await GenerateCommunitySummaryAsync(community, communityEntities, cancellationToken);
                community.UpdateSummary(summary);
            }
            
            return communities;
        }

        public async Task<bool> StoreDeltaTablesAsync(
            KnowledgeGraph knowledgeGraph,
            List<GraphRAGEntity> entities,
            List<GraphRAGRelationship> relationships,
            List<GraphCommunity> communities,
            CancellationToken cancellationToken = default)
        {
            var catalogName = knowledgeGraph.Configuration.DeltaTableCatalog;
            var repositorySchema = $"repository_{knowledgeGraph.RepositoryId.ToString().Replace("-", "_")}";
            
            // Create Delta Lake tables with ACID transactions
            await _databricks.ExecuteSqlAsync($"CREATE SCHEMA IF NOT EXISTS {catalogName}.{repositorySchema}");
            
            // Store entities in Delta Lake (with automatic versioning)
            await _databricks.MergeIntoTableAsync(
                tableName: $"{catalogName}.{repositorySchema}.entities",
                sourceData: entities,
                mergeKeys: new[] { "id", "repository_id" },
                cancellationToken);
            
            // Store relationships in Delta Lake (with ACID compliance)
            await _databricks.MergeIntoTableAsync(
                tableName: $"{catalogName}.{repositorySchema}.relationships", 
                sourceData: relationships,
                mergeKeys: new[] { "id", "repository_id" },
                cancellationToken);
            
            // Store communities in Delta Lake (with built-in versioning)
            await _databricks.MergeIntoTableAsync(
                tableName: $"{catalogName}.{repositorySchema}.communities",
                sourceData: communities,
                mergeKeys: new[] { "id", "repository_id" },
                cancellationToken);
            
            // Store text units for GraphRAG context
            var textUnits = GenerateTextUnitsFromEntities(entities);
            await _databricks.MergeIntoTableAsync(
                tableName: $"{catalogName}.{repositorySchema}.text_units",
                sourceData: textUnits,
                mergeKeys: new[] { "id", "repository_id" },
                cancellationToken);
            
            // Store community reports for global search
            var reports = GenerateReportsFromCommunities(communities);
            await _databricks.MergeIntoTableAsync(
                tableName: $"{catalogName}.{repositorySchema}.community_reports",
                sourceData: reports, 
                mergeKeys: new[] { "id", "repository_id" },
                cancellationToken);
            
            return true;
        }

        private async Task<string> GenerateCommunitySummaryAsync(
            GraphCommunity community, 
            List<GraphRAGEntity> entities,
            CancellationToken cancellationToken)
        {
            var prompt = $@"
            Analyze this group of related code entities and generate a summary of their architectural purpose and relationships.
            
            Community: {community.Title}
            Entities: {string.Join(", ", entities.Select(e => e.Name))}
            
            Provide a concise summary (2-3 sentences) that explains:
            1. What this community of code represents (architectural pattern, functional area, etc.)
            2. How the entities relate to each other
            3. The overall purpose or responsibility of this code grouping
            
            Focus on architectural insights and patterns rather than implementation details.
            ";

            return await _azureOpenAI.GenerateCompletionAsync(prompt, cancellationToken);
        }
        
        private Dictionary<string, List<GraphRAGEntity>> BuildEntityGraph(
            List<GraphRAGEntity> entities, 
            List<GraphRAGRelationship> relationships)
        {
            // Build adjacency list representation for community detection
            var graph = entities.ToDictionary(e => e.Id, e => new List<GraphRAGEntity>());
            
            foreach (var relationship in relationships)
            {
                if (graph.ContainsKey(relationship.Source) && graph.ContainsKey(relationship.Target))
                {
                    var targetEntity = entities.First(e => e.Id == relationship.Target);
                    graph[relationship.Source].Add(targetEntity);
                }
            }
            
            return graph;
        }
    }
}
```

#### GraphRAG Query Service (Global & Local Search)
```csharp
namespace Archie.Application.GraphRAG
{
    public interface IGraphRAGQueryService
    {
        Task<GraphRAGResult> ExecuteGlobalSearchAsync(
            Guid knowledgeGraphId,
            string query,
            QueryContext context = null,
            CancellationToken cancellationToken = default);
            
        Task<GraphRAGResult> ExecuteLocalSearchAsync(
            Guid knowledgeGraphId,
            string query,
            string focusEntityId = null,
            QueryContext context = null,
            CancellationToken cancellationToken = default);
            
        Task<GraphRAGResult> ExecuteHybridSearchAsync(
            Guid knowledgeGraphId,
            string query,
            QueryContext context = null,
            CancellationToken cancellationToken = default);
    }

    public class GraphRAGQueryService : IGraphRAGQueryService
    {
        private readonly IGraphRAGEngine _graphRAGEngine;
        private readonly IGlobalSearchService _globalSearch;
        private readonly ILocalSearchService _localSearch;
        private readonly IAzureOpenAIService _azureOpenAI;
        private readonly ILogger<GraphRAGQueryService> _logger;

        public async Task<GraphRAGResult> ExecuteGlobalSearchAsync(
            Guid knowledgeGraphId,
            string query,
            QueryContext context = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var queryId = Guid.NewGuid().ToString();
            
            try
            {
                _logger.LogInformation("Executing GraphRAG global search for query: {Query}", query);
                
                // Load community data for global search
                var communities = await _graphRAGEngine.GetCommunitiesAsync(knowledgeGraphId, cancellationToken);
                
                // Perform community-based search (Microsoft GraphRAG global pattern)
                var relevantCommunities = await _globalSearch.FindRelevantCommunitiesAsync(
                    communities, 
                    query, 
                    cancellationToken);
                
                // Generate response based on community reports
                var globalResponse = await GenerateGlobalResponseAsync(
                    query, 
                    relevantCommunities, 
                    cancellationToken);
                
                var result = new GraphRAGResult
                {
                    QueryId = queryId,
                    Response = globalResponse.Answer,
                    QueryType = GraphRAGQueryType.GlobalSearch,
                    Confidence = globalResponse.Confidence,
                    ProcessingTime = stopwatch.Elapsed
                };
                
                // Add community contexts
                foreach (var community in relevantCommunities)
                {
                    result.AddContext(new GraphRAGContext
                    {
                        CommunityId = community.Id,
                        Content = community.Summary,
                        Relevance = community.Rank,
                        Type = ContextType.CommunityReport
                    });
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Global GraphRAG search failed for query: {Query}", query);
                throw;
            }
        }

        public async Task<GraphRAGResult> ExecuteLocalSearchAsync(
            Guid knowledgeGraphId,
            string query,
            string focusEntityId = null,
            QueryContext context = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var queryId = Guid.NewGuid().ToString();
            
            try
            {
                _logger.LogInformation("Executing GraphRAG local search for query: {Query}, focus entity: {EntityId}", query, focusEntityId);
                
                // Find relevant entities and their local neighborhoods
                var relevantEntities = await _localSearch.FindRelevantEntitiesAsync(
                    knowledgeGraphId, 
                    query, 
                    focusEntityId, 
                    cancellationToken);
                
                // Get entity relationships and context
                var entityNetworks = await _graphRAGEngine.GetEntityNetworksAsync(
                    knowledgeGraphId, 
                    relevantEntities.Select(e => e.Id).ToList(), 
                    cancellationToken);
                
                // Generate response based on entity details and relationships
                var localResponse = await GenerateLocalResponseAsync(
                    query, 
                    relevantEntities, 
                    entityNetworks, 
                    cancellationToken);
                
                var result = new GraphRAGResult
                {
                    QueryId = queryId,
                    Response = localResponse.Answer,
                    QueryType = GraphRAGQueryType.LocalSearch,
                    Confidence = localResponse.Confidence,
                    ProcessingTime = stopwatch.Elapsed
                };
                
                // Add entity contexts and sources
                foreach (var entity in relevantEntities)
                {
                    result.AddContext(new GraphRAGContext
                    {
                        EntityId = entity.Id,
                        Content = entity.Description,
                        Relevance = entity.Rank,
                        Type = ContextType.EntityDescription
                    });
                    
                    // Add source code references
                    if (entity.Type == EntityType.Method || entity.Type == EntityType.Class)
                    {
                        result.AddSource(new GraphRAGSource
                        {
                            EntityId = entity.Id,
                            FilePath = GetEntityFilePath(entity),
                            CodeSnippet = await GetEntityCodeSnippet(entity, cancellationToken),
                            Relevance = entity.Rank
                        });
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Local GraphRAG search failed for query: {Query}", query);
                throw;
            }
        }

        public async Task<GraphRAGResult> ExecuteHybridSearchAsync(
            Guid knowledgeGraphId,
            string query,
            QueryContext context = null,
            CancellationToken cancellationToken = default)
        {
            // Execute both global and local search, then combine results intelligently
            var globalTask = ExecuteGlobalSearchAsync(knowledgeGraphId, query, context, cancellationToken);
            var localTask = ExecuteLocalSearchAsync(knowledgeGraphId, query, null, context, cancellationToken);
            
            var results = await Task.WhenAll(globalTask, localTask);
            var globalResult = results[0];
            var localResult = results[1];
            
            // Combine results using hybrid fusion strategy
            var hybridResponse = await FuseGlobalLocalResults(query, globalResult, localResult, cancellationToken);
            
            var result = new GraphRAGResult
            {
                QueryId = Guid.NewGuid().ToString(),
                Response = hybridResponse,
                QueryType = GraphRAGQueryType.HybridSearch,
                Confidence = Math.Max(globalResult.Confidence, localResult.Confidence),
                ProcessingTime = TimeSpan.FromMilliseconds(globalResult.ProcessingTime.TotalMilliseconds + localResult.ProcessingTime.TotalMilliseconds)
            };
            
            // Combine contexts from both searches
            result.Contexts.AddRange(globalResult.Contexts);
            result.Contexts.AddRange(localResult.Contexts);
            result.Sources.AddRange(globalResult.Sources);
            result.Sources.AddRange(localResult.Sources);
            
            return result;
        }

        private async Task<(string Answer, double Confidence)> GenerateGlobalResponseAsync(
            string query,
            List<GraphCommunity> communities,
            CancellationToken cancellationToken)
        {
            var communityContext = string.Join("\n\n", communities.Select(c => 
                $"Community: {c.Title}\nSummary: {c.Summary}\nEntities: {string.Join(", ", c.EntityIds.Take(5))}"));
            
            var prompt = $@"
            Based on the architectural communities and patterns identified in this codebase, answer the following question:

            Question: {query}

            Available Communities and Patterns:
            {communityContext}

            Provide a comprehensive answer that:
            1. Addresses the architectural aspects of the question
            2. References relevant communities and patterns
            3. Explains how different parts of the codebase work together
            4. Focuses on high-level design and structure

            Provide confidence score (0.0-1.0) for your answer accuracy.
            Format: [CONFIDENCE:0.85] Your detailed answer here.
            ";

            var response = await _azureOpenAI.GenerateCompletionAsync(prompt, cancellationToken);
            return ParseConfidenceResponse(response);
        }

        private async Task<(string Answer, double Confidence)> GenerateLocalResponseAsync(
            string query,
            List<GraphRAGEntity> entities,
            Dictionary<string, List<GraphRAGRelationship>> entityNetworks,
            CancellationToken cancellationToken)
        {
            var entityContext = string.Join("\n\n", entities.Select(e => 
                $"Entity: {e.Name} ({e.Type})\nDescription: {e.Description}\n" +
                $"Related entities: {string.Join(", ", entityNetworks.GetValueOrDefault(e.Id, new List<GraphRAGRelationship>()).Take(3).Select(r => r.Target))}"));
            
            var prompt = $@"
            Based on the specific code entities and their relationships, answer the following question:

            Question: {query}

            Available Entities and Relationships:
            {entityContext}

            Provide a detailed answer that:
            1. References specific code entities and their roles
            2. Explains the relationships and interactions between entities
            3. Provides concrete examples from the codebase
            4. Focuses on implementation details and specific functionality

            Provide confidence score (0.0-1.0) for your answer accuracy.
            Format: [CONFIDENCE:0.92] Your detailed answer here.
            ";

            var response = await _azureOpenAI.GenerateCompletionAsync(prompt, cancellationToken);
            return ParseConfidenceResponse(response);
        }
        
        private (string Answer, double Confidence) ParseConfidenceResponse(string response)
        {
            var confidenceMatch = Regex.Match(response, @"\[CONFIDENCE:([\d\.]+)\]\s*(.*)");
            if (confidenceMatch.Success)
            {
                var confidence = double.Parse(confidenceMatch.Groups[1].Value);
                var answer = confidenceMatch.Groups[2].Value.Trim();
                return (answer, confidence);
            }
            
            return (response, 0.5); // Default confidence if not provided
        }
    }
}
```

### GraphQL API Extensions

```graphql
# GraphRAG Knowledge Graph Types
type KnowledgeGraph {
  id: ID!
  repositoryId: ID!
  status: GraphStatus!
  createdAt: DateTime!
  lastUpdatedAt: DateTime!
  metrics: GraphRAGMetrics!
  configuration: GraphConfiguration!
  errors: [GraphError!]!
  isQueryReady: Boolean!
}

enum GraphStatus {
  NOT_BUILT
  BUILDING_COMMUNITIES
  GENERATING_PARQUET_FILES
  LOADING_IN_MEMORY
  READY
  FAILED
  REQUIRES_UPDATE
}

type GraphRAGMetrics {
  entityCount: Int!
  relationshipCount: Int!
  communityCount: Int!
  maxCommunityLevel: Int!
  communityDistribution: JSON!
  constructionTime: Float!
  deltaTableSize: Float!
  lastUpdated: DateTime!
  averageEntityRank: Float!
  averageRelationshipWeight: Float!
}

# GraphRAG Community Detection
type GraphCommunity {
  id: ID!
  title: String!
  summary: String!
  entityIds: [String!]!
  level: Int!
  rank: Float!
  metadata: CommunityMetadata!
  detectedAt: DateTime!
  
  # Navigation
  entities: [GraphRAGEntity!]!
  subCommunities: [GraphCommunity!]!
  parentCommunity: GraphCommunity
}

# Enhanced entities for GraphRAG
type GraphRAGEntity {
  id: ID!
  name: String!
  description: String!
  type: EntityType!
  textUnits: [String!]!
  attributes: JSON!
  embeddings: [Float!]!
  rank: Float!
  communityIds: [String!]!
  
  # Navigation
  communities: [GraphCommunity!]!
  relationships: [GraphRAGRelationship!]!
  neighborhood(maxDistance: Int = 2): [GraphRAGEntity!]!
}

# Enhanced relationships for GraphRAG
type GraphRAGRelationship {
  id: ID!
  source: String!
  target: String!
  description: String!
  weight: Float!
  type: RelationshipType!
  textUnits: [String!]!
  attributes: JSON!
  rank: Float!
  
  # Navigation
  sourceEntity: GraphRAGEntity!
  targetEntity: GraphRAGEntity!
}

# GraphRAG Query and Results
type GraphRAGQuery {
  queryId: ID!
  query: String!
  type: GraphRAGQueryType!
  scope: SearchScope!
  context: QueryContext!
  createdAt: DateTime!
}

enum GraphRAGQueryType {
  GLOBAL_SEARCH
  LOCAL_SEARCH
  HYBRID_SEARCH
}

type GraphRAGResult {
  queryId: ID!
  response: String!
  contexts: [GraphRAGContext!]!
  sources: [GraphRAGSource!]!
  confidence: Float!
  processingTime: Float!
  queryType: GraphRAGQueryType!
}

type GraphRAGContext {
  communityId: String
  entityId: String
  content: String!
  relevance: Float!
  type: ContextType!
}

enum ContextType {
  COMMUNITY_REPORT
  ENTITY_DESCRIPTION
  RELATIONSHIP_DESCRIPTION
  SOURCE_CODE
  DOCUMENTATION
}

type GraphRAGSource {
  entityId: String!
  filePath: String!
  startLine: Int
  endLine: Int
  codeSnippet: String
  relevance: Float!
}

# Queries
extend type Query {
  # Knowledge Graph Management
  knowledgeGraph(id: ID!): KnowledgeGraph
  knowledgeGraphByRepository(repositoryId: ID!): KnowledgeGraph
  
  # Community Exploration
  graphCommunities(knowledgeGraphId: ID!, level: Int): [GraphCommunity!]!
  graphCommunity(id: ID!): GraphCommunity
  
  # GraphRAG Entities and Relationships  
  graphragEntity(id: ID!): GraphRAGEntity
  graphragEntities(knowledgeGraphId: ID!, filter: GraphRAGEntityFilter): [GraphRAGEntity!]!
  graphragRelationships(knowledgeGraphId: ID!, filter: GraphRAGRelationshipFilter): [GraphRAGRelationship!]!
  
  # GraphRAG Queries (Main Feature)
  graphragGlobalSearch(
    knowledgeGraphId: ID!
    query: String!
    context: QueryContextInput
  ): GraphRAGResult!
  
  graphragLocalSearch(
    knowledgeGraphId: ID!
    query: String!
    focusEntityId: String
    context: QueryContextInput
  ): GraphRAGResult!
  
  graphragHybridSearch(
    knowledgeGraphId: ID!
    query: String!
    context: QueryContextInput
  ): GraphRAGResult!
}

# Mutations
extend type Mutation {
  buildKnowledgeGraph(
    repositoryId: ID!
    configuration: GraphConfigurationInput
  ): KnowledgeGraph!
  
  updateKnowledgeGraph(id: ID!): KnowledgeGraph!
  
  deleteKnowledgeGraph(id: ID!): Boolean!
}

# Subscriptions
extend type Subscription {
  knowledgeGraphBuildProgress(id: ID!): KnowledgeGraph!
  graphragQueryProgress(queryId: ID!): GraphRAGResult!
}

# Input types
input GraphConfigurationInput {
  maxCommunityLevels: Int = 3
  minCommunitySize: Float = 5.0
  entityRankThreshold: Float = 0.1
  maxTextUnitsPerEntity: Int = 10
  enableHierarchicalCommunities: Boolean = true
  enableGlobalSearch: Boolean = true
  enableLocalSearch: Boolean = true
  deltaTableCatalog: String
}

input QueryContextInput {
  repositoryId: ID
  includeSource: Boolean = true
  maxSources: Int = 5
  includeRelationships: Boolean = true
  maxRelationshipDepth: Int = 2
}

input GraphRAGEntityFilter {
  types: [EntityType!]
  communityIds: [String!]
  minRank: Float
  hasDescription: Boolean
}

input GraphRAGRelationshipFilter {
  types: [RelationshipType!]
  minWeight: Float
  sourceEntityId: String
  targetEntityId: String
}
```

### Implementation Roadmap

#### Phase 6A: GraphRAG Foundation (Weeks 1-3)
**Microsoft GraphRAG Integration**
1. **Microsoft GraphRAG Library Setup**
   - Install Microsoft.GraphRAG NuGet packages for .NET 9
   - Configure Azure Databricks workspace in Australia East region
   - Set up community detection algorithms using Microsoft's patterns
   - Create Delta Lake table schemas following Microsoft GraphRAG patterns with ACID compliance

2. **Knowledge Graph Construction Pipeline**
   - Build entity aggregation service consuming Feature 05 analysis results
   - Implement community detection using Microsoft's hierarchical clustering
   - Create Delta Lake table storage following entities/relationships/communities schema with versioning
   - Add in-memory graph loading and indexing capabilities

#### Phase 6B: Query Engine Implementation (Weeks 3-5)
**Global and Local Search**
1. **Global Search Implementation (Community-Based)**
   - Implement community-based search using Microsoft GraphRAG patterns
   - Build community report generation with Azure OpenAI integration
   - Create high-level architectural query processing
   - Add global search result ranking and confidence scoring

2. **Local Search Implementation (Entity-Based)**
   - Implement entity neighborhood search and traversal
   - Build detailed code exploration query processing  
   - Create relationship-aware context generation
   - Add local search result fusion and source attribution

#### Phase 6C: Production Readiness (Weeks 5-6)
**Performance and Integration**
1. **Australian Sovereignty Compliance**
   - Verify all GraphRAG processing occurs within Australia East region
   - Implement compliance monitoring and reporting
   - Add audit trails for GraphRAG operations and queries
   - Create data residency validation and enforcement

2. **GraphQL API and Integration**
   - Add comprehensive GraphQL resolvers for all GraphRAG operations
   - Implement real-time subscriptions for build progress and query results
   - Create integration points for Feature 14 (Visual Discovery Interface)
   - Add comprehensive error handling and resilience patterns

### Performance Requirements

#### GraphRAG Construction Targets
- **Knowledge Graph Build**: Complete graph for 50K entities within 30 minutes
- **Community Detection**: Identify architectural communities within 10 minutes
- **Delta Lake Storage**: Store entities/relationships/communities with ACID transactions within 5 minutes
- **In-Memory Loading**: Load graph for queries within 2 minutes
- **Memory Usage**: Maintain efficient memory usage (<8GB) for large repositories

#### Query Performance Targets
- **Global Search**: <3 seconds for community-based architectural queries
- **Local Search**: <1 second for entity-specific detailed queries  
- **Hybrid Search**: <5 seconds for combined global/local analysis
- **Concurrent Queries**: Support 50+ simultaneous GraphRAG queries
- **Australian Latency**: Sub-200ms for all AI service calls within region

### Security and Compliance

#### Australian Data Sovereignty GraphRAG
- **Delta Lake Storage Australia East**: All knowledge graph data stored within Australian region with ACID compliance
- **In-Memory Processing Australia East**: Graph construction and queries processed locally
- **Azure OpenAI Australia East**: Community detection routed through Australian endpoints
- **Compliance Monitoring**: Real-time validation of data residency requirements

#### GraphRAG Data Security
- **Encrypted Delta Lake Storage**: All graph data encrypted at rest using Azure Key Vault keys with ACID compliance
- **Access Control**: Repository-based permissions for GraphRAG operations and queries
- **Query Audit Logging**: Complete audit trail for all GraphRAG queries and results
- **API Security**: Secure GraphQL endpoints with authentication and rate limiting

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)

#### GraphRAG Construction Tests
```csharp
[TestFixture]
public class GraphRAGKnowledgeConstructionServiceTests
{
    [Test]
    public async Task BuildKnowledgeGraphAsync_ValidRepository_CreatesGraphRAGStructure()
    {
        // Test complete knowledge graph construction pipeline
        // Verify entities, relationships, communities are properly generated
    }
    
    [Test]
    public async Task DetectCommunitiesAsync_CodeEntities_IdentifiesArchitecturalPatterns()
    {
        // Test community detection algorithms
        // Verify architectural patterns are correctly identified
    }
    
    [Test]
    public async Task StoreDeltaTablesAsync_GraphData_CreatesValidDeltaTables()
    {
        // Test Delta Lake table storage following Microsoft schemas with ACID transactions
        // Verify file structure and data integrity
    }
}
```

#### GraphRAG Query Tests
```csharp
[TestFixture]
public class GraphRAGQueryServiceTests
{
    [Test]
    public async Task ExecuteGlobalSearchAsync_ArchitecturalQuery_ReturnsRelevantCommunities()
    {
        // Test global search with community-based queries
        // Verify architectural insights and accuracy
    }
    
    [Test]
    public async Task ExecuteLocalSearchAsync_EntityQuery_ReturnsDetailedContext()
    {
        // Test local search with entity-specific queries
        // Verify detailed code exploration and relationships
    }
    
    [Test]
    public async Task ExecuteHybridSearchAsync_ComplexQuery_CombinesGlobalLocal()
    {
        // Test hybrid search combining both approaches
        // Verify result fusion and enhanced accuracy
    }
}
```

### Integration Testing Requirements (40% coverage minimum)

#### End-to-End GraphRAG Pipeline
- **Complete Repository Processing**: Test full pipeline from Feature 05 entities to queryable GraphRAG
- **Multi-Repository GraphRAG**: Test knowledge graph construction across multiple repositories
- **Query Accuracy**: Validate GraphRAG query results against expected architectural insights
- **Performance**: Verify construction and query performance meets targets
- **Australian Sovereignty**: Test all processing occurs within Australia East region

#### Microsoft GraphRAG Library Integration
- **Delta Lake Compatibility**: Test Delta Lake tables work with Microsoft GraphRAG patterns and queries
- **Community Detection**: Validate community algorithms produce expected architectural groupings
- **Global/Local Search**: Test query patterns match Microsoft GraphRAG examples
- **Vector Integration**: Verify Azure AI Search vector similarity enhances GraphRAG results

### Quality Assurance

#### Code Review Checkpoints
- [ ] Microsoft GraphRAG implementation follows official patterns and schemas
- [ ] Community detection identifies meaningful architectural patterns
- [ ] Global search provides accurate high-level architectural insights  
- [ ] Local search delivers detailed entity-specific exploration
- [ ] Delta Lake storage follows Microsoft's GraphRAG schema specifications with ACID compliance
- [ ] Query performance meets sub-3-second targets for typical repositories
- [ ] Australian data sovereignty requirements are enforced throughout
- [ ] Integration with Feature 05 analysis data is seamless and reliable

#### Definition of Done Checklist
- [ ] All unit tests pass with >80% coverage
- [ ] Integration tests pass with >40% coverage
- [ ] Knowledge graph construction works for repositories analyzed by Feature 05
- [ ] Community detection identifies 90% of architectural patterns correctly
- [ ] Global search answers architectural questions with high accuracy
- [ ] Local search provides detailed code exploration with source references  
- [ ] Delta Lake tables created following Microsoft GraphRAG schemas with versioning
- [ ] Query performance meets established targets
- [ ] GraphQL API supports all GraphRAG operations
- [ ] Australian sovereignty compliance verified and monitored
- [ ] Foundation ready for Feature 14 (Visual Discovery Interface) integration

## Conclusion

Feature 13 delivers the core Microsoft GraphRAG implementation that transforms Archie from a traditional repository analysis tool into an enterprise-grade AI-powered code exploration platform. By implementing Microsoft's official GraphRAG patterns with **Azure Databricks Delta Lake** storage and community detection, this feature provides the 3x to 90x accuracy improvement over vanilla RAG that makes complex architectural queries and conversational code exploration truly effective.

The feature enables DeepWiki-like functionality with enterprise advantages including Australian data sovereignty, superior query accuracy through global/local search patterns, and seamless integration with existing analysis infrastructure. This foundation directly enables Feature 13 (Visual Discovery Interface) and Feature 14 (Enterprise Analytics) while delivering immediate value through conversational GraphRAG queries for architectural understanding and code exploration.