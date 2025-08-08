# Feature 07: Knowledge Graph Construction and Relationship Mapping

## Feature Overview

**Feature ID**: F07  
**Feature Name**: Knowledge Graph Construction and Relationship Mapping  
**Phase**: Phase 3 (Weeks 9-12)  
**Bounded Context**: Knowledge Graph Context  

### Business Value Proposition
Build sophisticated relationships between code components, dependencies, and concepts using Neo4j graph algorithms. This feature transforms parsed code structures into a rich knowledge graph that enables advanced architectural insights, dependency analysis, and pattern detection.

### User Impact
- Architects can visualize complex system dependencies and relationships
- Developers gain insights into code coupling and architectural patterns
- Teams can identify refactoring opportunities and architectural smells
- New developers can understand system architecture through interactive graph exploration

### Success Criteria
- Accurate relationship mapping for 95% of code dependencies
- Complete architectural pattern detection for common design patterns
- Graph queries execute within 3 seconds for typical operations
- Support for impact analysis and change propagation tracking
- Visualization support for 1000+ node graphs

### Dependencies
- F01: Repository Connection and Management (for repository data)
- F03: File Parsing and Code Structure Indexing (for code structures)
- F04: GraphQL API Foundation (for API access)

## Technical Specification

### Domain Model
```csharp
// Knowledge Graph Aggregates
public class CodeEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string FullName { get; private set; }
    public EntityType Type { get; private set; }
    public Guid RepositoryId { get; private set; }
    public SourceLocation Location { get; private set; }
    public EntityMetadata Metadata { get; private set; }
    public List<Relationship> OutgoingRelationships { get; private set; }
    public List<Relationship> IncomingRelationships { get; private set; }
    public EntityComplexity Complexity { get; private set; }
    public DateTime AnalyzedAt { get; private set; }
}

public class Relationship
{
    public Guid Id { get; private set; }
    public Guid SourceEntityId { get; private set; }
    public Guid TargetEntityId { get; private set; }
    public RelationshipType Type { get; private set; }
    public RelationshipStrength Strength { get; private set; }
    public RelationshipMetadata Metadata { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsInferred { get; private set; }
}

public class ArchitecturalPattern
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public PatternType Type { get; private set; }
    public string Description { get; private set; }
    public List<CodeEntity> Participants { get; private set; }
    public List<Relationship> PatternRelationships { get; private set; }
    public float ConfidenceScore { get; private set; }
    public DateTime DetectedAt { get; private set; }
    public Guid RepositoryId { get; private set; }
}

public class DependencyCluster
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public List<CodeEntity> Entities { get; private set; }
    public ClusterType Type { get; private set; }
    public float CohesionScore { get; private set; }
    public float CouplingScore { get; private set; }
    public List<DependencyCluster> Dependencies { get; private set; }
    public DateTime CreatedAt { get; private set; }
}

public class ImpactAnalysis
{
    public Guid Id { get; private set; }
    public CodeEntity SourceEntity { get; private set; }
    public List<ImpactedEntity> ImpactedEntities { get; private set; }
    public ImpactScope Scope { get; private set; }
    public DateTime AnalyzedAt { get; private set; }
    public TimeSpan AnalysisDuration { get; private set; }
}

public enum EntityType
{
    Class,
    Interface,
    Method,
    Property,
    Function,
    Module,
    Package,
    Namespace,
    File,
    Database,
    ExternalService
}

public enum RelationshipType
{
    Inherits,
    Implements,
    Uses,
    Calls,
    Contains,
    DependsOn,
    Aggregates,
    Composes,
    Associates,
    Realizes,
    Extends,
    Imports,
    References
}

public enum PatternType
{
    Creational,
    Structural,
    Behavioral,
    Architectural,
    Enterprise,
    Concurrency,
    Messaging,
    DataAccess
}

public enum ClusterType
{
    Module,
    Layer,
    Component,
    Service,
    Domain,
    Infrastructure
}
```

### API Specification

#### GraphQL Schema Extensions
```graphql
type CodeEntity {
  id: ID!
  name: String!
  fullName: String!
  type: EntityType!
  repository: Repository!
  location: SourceLocation!
  metadata: EntityMetadata!
  outgoingRelationships: [Relationship!]!
  incomingRelationships: [Relationship!]!
  complexity: EntityComplexity!
  analyzedAt: DateTime!
  
  # Navigation queries
  dependencies: [CodeEntity!]!
  dependents: [CodeEntity!]!
  relatedEntities(type: RelationshipType): [CodeEntity!]!
  
  # Analysis queries
  impactAnalysis: ImpactAnalysis!
  architecturalPatterns: [ArchitecturalPattern!]!
  cluster: DependencyCluster
}

type Relationship {
  id: ID!
  source: CodeEntity!
  target: CodeEntity!
  type: RelationshipType!
  strength: RelationshipStrength!
  metadata: RelationshipMetadata!
  createdAt: DateTime!
  isInferred: Boolean!
}

type ArchitecturalPattern {
  id: ID!
  name: String!
  type: PatternType!
  description: String!
  participants: [CodeEntity!]!
  relationships: [Relationship!]!
  confidenceScore: Float!
  detectedAt: DateTime!
  repository: Repository!
  
  # Pattern-specific details
  examples: [PatternExample!]!
  benefits: [String!]!
  drawbacks: [String!]!
  alternatives: [ArchitecturalPattern!]!
}

type DependencyCluster {
  id: ID!
  name: String!
  entities: [CodeEntity!]!
  type: ClusterType!
  cohesionScore: Float!
  couplingScore: Float!
  dependencies: [DependencyCluster!]!
  createdAt: DateTime!
  
  # Cluster analysis
  stability: ClusterStability!
  maintainability: Float!
  refactoringOpportunities: [RefactoringOpportunity!]!
}

type ImpactAnalysis {
  id: ID!
  sourceEntity: CodeEntity!
  impactedEntities: [ImpactedEntity!]!
  scope: ImpactScope!
  analyzedAt: DateTime!
  analysisDuration: Int! # milliseconds
  
  # Impact visualization
  impactGraph: ImpactGraph!
  riskAssessment: RiskAssessment!
  changeRecommendations: [ChangeRecommendation!]!
}

type ImpactedEntity {
  entity: CodeEntity!
  impactType: ImpactType!
  severity: ImpactSeverity!
  confidence: Float!
  path: [Relationship!]! # Path from source to impacted entity
}

type EntityMetadata {
  tags: [String!]!
  annotations: [Annotation!]!
  metrics: EntityMetrics!
  lastModified: DateTime!
  author: String
  reviewStatus: ReviewStatus!
}

type EntityComplexity {
  cyclomaticComplexity: Int!
  cognitiveComplexity: Int!
  dependencyComplexity: Int!
  maintenanceComplexity: Float!
  overallScore: Float!
}

type RelationshipStrength {
  weight: Float!
  frequency: Int!
  importance: ImportanceLevel!
  stability: StabilityLevel!
}

type ImpactGraph {
  nodes: [ImpactNode!]!
  edges: [ImpactEdge!]!
  layers: [ImpactLayer!]!
  metrics: ImpactMetrics!
}

enum EntityType {
  CLASS
  INTERFACE
  METHOD
  PROPERTY
  FUNCTION
  MODULE
  PACKAGE
  NAMESPACE
  FILE
  DATABASE
  EXTERNAL_SERVICE
}

enum RelationshipType {
  INHERITS
  IMPLEMENTS
  USES
  CALLS
  CONTAINS
  DEPENDS_ON
  AGGREGATES
  COMPOSES
  ASSOCIATES
  REALIZES
  EXTENDS
  IMPORTS
  REFERENCES
}

enum PatternType {
  CREATIONAL
  STRUCTURAL
  BEHAVIORAL
  ARCHITECTURAL
  ENTERPRISE
  CONCURRENCY
  MESSAGING
  DATA_ACCESS
}

enum ImpactType {
  DIRECT
  INDIRECT
  TRANSITIVE
  CYCLIC
}

enum ImpactSeverity {
  LOW
  MEDIUM
  HIGH
  CRITICAL
}

# Extended Repository type
extend type Repository {
  knowledgeGraph: KnowledgeGraph!
  architecturalPatterns: [ArchitecturalPattern!]!
  dependencyClusters: [DependencyCluster!]!
  
  # Analysis operations
  analyzeImpact(entityId: ID!): ImpactAnalysis!
  findRelatedEntities(entityId: ID!, maxDepth: Int = 3): [CodeEntity!]!
  detectPatterns(patternTypes: [PatternType!]): [ArchitecturalPattern!]!
  analyzeDependencies: DependencyAnalysisResult!
}

type KnowledgeGraph {
  id: ID!
  repository: Repository!
  entities: [CodeEntity!]!
  relationships: [Relationship!]!
  patterns: [ArchitecturalPattern!]!
  clusters: [DependencyCluster!]!
  metrics: GraphMetrics!
  lastUpdated: DateTime!
  
  # Graph queries
  shortestPath(fromId: ID!, toId: ID!): [Relationship!]!
  findCycles: [CyclicDependency!]!
  centralityAnalysis: CentralityAnalysis!
  communityDetection: [DependencyCluster!]!
}

# New Queries
extend type Query {
  knowledgeGraph(repositoryId: ID!): KnowledgeGraph
  codeEntity(id: ID!): CodeEntity
  relationship(id: ID!): Relationship
  architecturalPattern(id: ID!): ArchitecturalPattern
  
  # Advanced graph queries
  findPath(
    repositoryId: ID!
    fromEntityId: ID!
    toEntityId: ID!
    relationshipTypes: [RelationshipType!]
  ): [Relationship!]!
  
  searchEntities(
    repositoryId: ID!
    query: String!
    entityTypes: [EntityType!]
    filters: EntityFilterInput
  ): [CodeEntity!]!
  
  analyzeArchitecture(repositoryId: ID!): ArchitecturalAnalysisResult!
}

# New Mutations
extend type Mutation {
  buildKnowledgeGraph(repositoryId: ID!): KnowledgeGraphBuildJob!
  refreshKnowledgeGraph(repositoryId: ID!): KnowledgeGraph!
  
  # Manual relationship management
  addRelationship(input: AddRelationshipInput!): Relationship!
  removeRelationship(relationshipId: ID!): Boolean!
  updateRelationshipStrength(relationshipId: ID!, strength: Float!): Relationship!
  
  # Pattern management
  confirmPattern(patternId: ID!): ArchitecturalPattern!
  rejectPattern(patternId: ID!): Boolean!
  createCustomPattern(input: CreatePatternInput!): ArchitecturalPattern!
}

# New Subscriptions
extend type Subscription {
  knowledgeGraphUpdated(repositoryId: ID!): KnowledgeGraphUpdateEvent!
  patternsDetected(repositoryId: ID!): PatternDetectionEvent!
  relationshipsChanged(repositoryId: ID!): RelationshipChangeEvent!
}

type KnowledgeGraphBuildJob {
  id: ID!
  repositoryId: ID!
  status: JobStatus!
  progress: Float!
  currentPhase: GraphBuildPhase!
  estimatedCompletion: DateTime
  startedAt: DateTime!
}

enum GraphBuildPhase {
  EXTRACTING_ENTITIES
  ANALYZING_RELATIONSHIPS
  DETECTING_PATTERNS
  BUILDING_CLUSTERS
  OPTIMIZING_GRAPH
  COMPLETED
}

input AddRelationshipInput {
  sourceEntityId: ID!
  targetEntityId: ID!
  type: RelationshipType!
  metadata: RelationshipMetadataInput
}

input EntityFilterInput {
  complexity: ComplexityFilterInput
  lastModifiedAfter: DateTime
  tags: [String!]
  hasPattern: Boolean
}
```

### Database Schema Changes

#### Enhanced Neo4j Schema
```cypher
// Enhanced Entity nodes with graph properties
(:CodeEntity {
  id: string,
  name: string,
  fullName: string,
  type: string,
  repositoryId: string,
  location: {
    startLine: integer,
    endLine: integer,
    file: string
  },
  complexity: {
    cyclomatic: integer,
    cognitive: integer,
    dependency: integer,
    maintenance: float,
    overall: float
  },
  metadata: {
    tags: [string],
    annotations: [{}],
    lastModified: datetime,
    author: string,
    reviewStatus: string
  },
  analyzedAt: datetime,
  
  // Graph analysis properties
  centrality: {
    betweenness: float,
    closeness: float,
    degree: float,
    pagerank: float
  },
  clusteringCoefficient: float,
  communityId: string
})

// Enhanced Relationship edges with strength metrics
()-[:RELATIONSHIP {
  type: string,
  strength: {
    weight: float,
    frequency: integer,
    importance: string,
    stability: string
  },
  metadata: {
    context: string,
    confidence: float,
    source: string, // "parsed" | "inferred" | "manual"
    validatedAt: datetime
  },
  createdAt: datetime,
  isInferred: boolean
}]->()

// Pattern nodes for architectural pattern detection
(:ArchitecturalPattern {
  id: string,
  name: string,
  type: string,
  description: string,
  confidenceScore: float,
  detectedAt: datetime,
  repositoryId: string,
  participants: [string], // Entity IDs
  patternMetadata: {
    category: string,
    complexity: string,
    benefits: [string],
    drawbacks: [string],
    examples: [string]
  }
})

// Cluster nodes for dependency analysis
(:DependencyCluster {
  id: string,
  name: string,
  type: string,
  cohesionScore: float,
  couplingScore: float,
  createdAt: datetime,
  repositoryId: string,
  stability: {
    abstractness: float,
    instability: float,
    distance: float
  },
  metrics: {
    size: integer,
    internalRelationships: integer,
    externalRelationships: integer
  }
})

// Enhanced relationship types for comprehensive analysis
(:CodeEntity)-[:INHERITS {strength: {}, metadata: {}}]->(:CodeEntity)
(:CodeEntity)-[:IMPLEMENTS {strength: {}, metadata: {}}]->(:CodeEntity)
(:CodeEntity)-[:USES {strength: {}, metadata: {}, usageType: string}]->(:CodeEntity)
(:CodeEntity)-[:CALLS {strength: {}, metadata: {}, callType: string}]->(:CodeEntity)
(:CodeEntity)-[:CONTAINS {strength: {}, metadata: {}}]->(:CodeEntity)
(:CodeEntity)-[:DEPENDS_ON {strength: {}, metadata: {}, dependencyType: string}]->(:CodeEntity)
(:CodeEntity)-[:AGGREGATES {strength: {}, metadata: {}}]->(:CodeEntity)
(:CodeEntity)-[:COMPOSES {strength: {}, metadata: {}}]->(:CodeEntity)

// Pattern participation relationships
(:CodeEntity)-[:PARTICIPATES_IN {role: string, importance: float}]->(:ArchitecturalPattern)
(:ArchitecturalPattern)-[:CONTAINS_PATTERN {subpatternType: string}]->(:ArchitecturalPattern)

// Cluster membership and dependencies
(:CodeEntity)-[:BELONGS_TO {membershipType: string, centrality: float}]->(:DependencyCluster)
(:DependencyCluster)-[:DEPENDS_ON_CLUSTER {strength: float, coupling: string}]->(:DependencyCluster)

// Indexes for performance
CREATE INDEX entity_fullname IF NOT EXISTS FOR (e:CodeEntity) ON (e.fullName);
CREATE INDEX entity_type_repo IF NOT EXISTS FOR (e:CodeEntity) ON (e.type, e.repositoryId);
CREATE INDEX relationship_type IF NOT EXISTS FOR ()-[r:RELATIONSHIP]-() ON (r.type);
CREATE INDEX pattern_repo_type IF NOT EXISTS FOR (p:ArchitecturalPattern) ON (p.repositoryId, p.type);
CREATE INDEX cluster_repo IF NOT EXISTS FOR (c:DependencyCluster) ON (c.repositoryId);

// Graph algorithms for analysis
CALL gds.graph.project(
  'codeGraph',
  'CodeEntity',
  'RELATIONSHIP'
) YIELD nodeCount, relationshipCount;
```

### Integration Points

#### Knowledge Graph Builder Service
```csharp
public interface IKnowledgeGraphService
{
    Task<KnowledgeGraphBuildJob> BuildKnowledgeGraphAsync(Guid repositoryId);
    Task<ImpactAnalysis> AnalyzeImpactAsync(Guid entityId);
    Task<List<ArchitecturalPattern>> DetectPatternsAsync(Guid repositoryId, List<PatternType> patternTypes = null);
    Task<List<DependencyCluster>> AnalyzeDependencyClustersAsync(Guid repositoryId);
    Task<List<Relationship>> FindShortestPathAsync(Guid fromEntityId, Guid toEntityId);
}

public class Neo4jKnowledgeGraphService : IKnowledgeGraphService
{
    private readonly IDriver _neo4jDriver;
    private readonly ICodeStructureService _codeStructureService;
    private readonly IPatternDetectionService _patternDetectionService;
    private readonly ILogger<Neo4jKnowledgeGraphService> _logger;

    public async Task<KnowledgeGraphBuildJob> BuildKnowledgeGraphAsync(Guid repositoryId)
    {
        var job = new KnowledgeGraphBuildJob
        {
            Id = Guid.NewGuid(),
            RepositoryId = repositoryId,
            Status = JobStatus.Running,
            CurrentPhase = GraphBuildPhase.ExtractingEntities,
            StartedAt = DateTime.UtcNow
        };

        // Start background processing
        _ = Task.Run(() => ProcessKnowledgeGraphBuildAsync(job));
        
        return job;
    }

    private async Task ProcessKnowledgeGraphBuildAsync(KnowledgeGraphBuildJob job)
    {
        try
        {
            await UpdateJobProgress(job.Id, 0.1f, GraphBuildPhase.ExtractingEntities);
            
            // 1. Extract all code entities
            var entities = await ExtractEntitiesAsync(job.RepositoryId);
            
            await UpdateJobProgress(job.Id, 0.3f, GraphBuildPhase.AnalyzingRelationships);
            
            // 2. Analyze and create relationships
            var relationships = await AnalyzeRelationshipsAsync(entities);
            
            await UpdateJobProgress(job.Id, 0.5f, GraphBuildPhase.DetectingPatterns);
            
            // 3. Detect architectural patterns
            var patterns = await DetectArchitecturalPatternsAsync(entities, relationships);
            
            await UpdateJobProgress(job.Id, 0.7f, GraphBuildPhase.BuildingClusters);
            
            // 4. Create dependency clusters
            var clusters = await CreateDependencyClustersAsync(entities, relationships);
            
            await UpdateJobProgress(job.Id, 0.9f, GraphBuildPhase.OptimizingGraph);
            
            // 5. Optimize graph structure and calculate metrics
            await OptimizeGraphStructureAsync(job.RepositoryId);
            await CalculateGraphMetricsAsync(job.RepositoryId);
            
            await UpdateJobProgress(job.Id, 1.0f, GraphBuildPhase.Completed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Knowledge graph build failed for repository {RepositoryId}", job.RepositoryId);
            await UpdateJobError(job.Id, ex.Message);
        }
    }

    private async Task<List<CodeEntity>> ExtractEntitiesAsync(Guid repositoryId)
    {
        var entities = new List<CodeEntity>();
        
        // Extract from parsed code structures
        var classes = await _codeStructureService.GetClassesAsync(repositoryId);
        entities.AddRange(classes.Select(c => new CodeEntity
        {
            Id = Guid.NewGuid(),
            Name = c.Name,
            FullName = c.FullName,
            Type = EntityType.Class,
            RepositoryId = repositoryId,
            Location = c.Location,
            Complexity = CalculateEntityComplexity(c),
            AnalyzedAt = DateTime.UtcNow
        }));
        
        var methods = await _codeStructureService.GetMethodsAsync(repositoryId);
        entities.AddRange(methods.Select(m => new CodeEntity
        {
            Id = Guid.NewGuid(),
            Name = m.Name,
            FullName = $"{m.Class.FullName}.{m.Name}",
            Type = EntityType.Method,
            RepositoryId = repositoryId,
            Location = m.Location,
            Complexity = CalculateEntityComplexity(m),
            AnalyzedAt = DateTime.UtcNow
        }));

        // Store entities in Neo4j
        await StoreEntitiesAsync(entities);
        
        return entities;
    }

    private async Task<List<Relationship>> AnalyzeRelationshipsAsync(List<CodeEntity> entities)
    {
        var relationships = new List<Relationship>();
        
        using var session = _neo4jDriver.AsyncSession();
        
        // Analyze inheritance relationships
        var inheritanceQuery = @"
            MATCH (child:CodeEntity)-[:INHERITS]->(parent:CodeEntity)
            WHERE child.repositoryId = $repositoryId
            RETURN child.id as childId, parent.id as parentId";
            
        var inheritanceResult = await session.RunAsync(inheritanceQuery, 
            new { repositoryId = entities.First().RepositoryId.ToString() });
            
        await inheritanceResult.ForEachAsync(record =>
        {
            relationships.Add(new Relationship
            {
                Id = Guid.NewGuid(),
                SourceEntityId = Guid.Parse(record["childId"].As<string>()),
                TargetEntityId = Guid.Parse(record["parentId"].As<string>()),
                Type = RelationshipType.Inherits,
                Strength = new RelationshipStrength
                {
                    Weight = 0.8f,
                    Importance = ImportanceLevel.High,
                    Stability = StabilityLevel.Stable
                },
                CreatedAt = DateTime.UtcNow,
                IsInferred = false
            });
        });

        // Analyze method call relationships
        await AnalyzeMethodCallRelationshipsAsync(relationships, entities);
        
        // Analyze dependency relationships
        await AnalyzeDependencyRelationshipsAsync(relationships, entities);
        
        return relationships;
    }

    public async Task<ImpactAnalysis> AnalyzeImpactAsync(Guid entityId)
    {
        var impactedEntities = new List<ImpactedEntity>();
        
        using var session = _neo4jDriver.AsyncSession();
        
        // Use Neo4j graph algorithms to find impact
        var impactQuery = @"
            MATCH (source:CodeEntity {id: $entityId})
            CALL gds.bfs.stream('codeGraph', {
                sourceNode: source,
                maxDepth: 5
            })
            YIELD path, nodeIds
            UNWIND nodeIds as nodeId
            MATCH (impacted:CodeEntity) WHERE id(impacted) = nodeId
            RETURN impacted, length(path) as distance, path";
            
        var result = await session.RunAsync(impactQuery, new { entityId = entityId.ToString() });
        
        await result.ForEachAsync(record =>
        {
            var impactedNode = record["impacted"].As<INode>();
            var distance = record["distance"].As<int>();
            
            impactedEntities.Add(new ImpactedEntity
            {
                Entity = MapNodeToEntity(impactedNode),
                ImpactType = distance == 1 ? ImpactType.Direct : ImpactType.Indirect,
                Severity = CalculateImpactSeverity(distance),
                Confidence = CalculateImpactConfidence(distance)
            });
        });

        return new ImpactAnalysis
        {
            Id = Guid.NewGuid(),
            SourceEntityId = entityId,
            ImpactedEntities = impactedEntities,
            Scope = DetermineImpactScope(impactedEntities),
            AnalyzedAt = DateTime.UtcNow
        };
    }
}
```

#### Pattern Detection Service
```csharp
public interface IPatternDetectionService
{
    Task<List<ArchitecturalPattern>> DetectPatternsAsync(Guid repositoryId, List<PatternType> patternTypes = null);
    Task<ArchitecturalPattern> DetectSpecificPatternAsync(Guid repositoryId, string patternName);
    Task<float> CalculatePatternConfidenceAsync(ArchitecturalPattern pattern);
}

public class AIPatternDetectionService : IPatternDetectionService
{
    private readonly OpenAIClient _openAIClient;
    private readonly IDriver _neo4jDriver;
    private readonly ILogger<AIPatternDetectionService> _logger;

    public async Task<List<ArchitecturalPattern>> DetectPatternsAsync(Guid repositoryId, List<PatternType> patternTypes = null)
    {
        var patterns = new List<ArchitecturalPattern>();
        
        // Common patterns to detect
        var patternsToDetect = patternTypes ?? new List<PatternType>
        {
            PatternType.Creational,
            PatternType.Structural,
            PatternType.Behavioral,
            PatternType.Architectural
        };

        foreach (var patternType in patternsToDetect)
        {
            var detectedPatterns = await DetectPatternTypeAsync(repositoryId, patternType);
            patterns.AddRange(detectedPatterns);
        }

        return patterns;
    }

    private async Task<List<ArchitecturalPattern>> DetectPatternTypeAsync(Guid repositoryId, PatternType patternType)
    {
        var patterns = new List<ArchitecturalPattern>();

        switch (patternType)
        {
            case PatternType.Creational:
                patterns.AddRange(await DetectCreationalPatternsAsync(repositoryId));
                break;
            case PatternType.Structural:
                patterns.AddRange(await DetectStructuralPatternsAsync(repositoryId));
                break;
            case PatternType.Behavioral:
                patterns.AddRange(await DetectBehavioralPatternsAsync(repositoryId));
                break;
            case PatternType.Architectural:
                patterns.AddRange(await DetectArchitecturalLayeringAsync(repositoryId));
                break;
        }

        return patterns;
    }

    private async Task<List<ArchitecturalPattern>> DetectCreationalPatternsAsync(Guid repositoryId)
    {
        var patterns = new List<ArchitecturalPattern>();
        
        // Detect Singleton pattern
        var singletonQuery = @"
            MATCH (c:CodeEntity {type: 'Class', repositoryId: $repositoryId})
            WHERE EXISTS {
                MATCH (c)-[:HAS_METHOD]->(constructor:CodeEntity {type: 'Method'})
                WHERE constructor.name = 'constructor' AND constructor.accessLevel = 'PRIVATE'
            }
            AND EXISTS {
                MATCH (c)-[:HAS_METHOD]->(getInstance:CodeEntity {type: 'Method'})
                WHERE getInstance.name =~ '(?i).*getInstance.*' AND getInstance.isStatic = true
            }
            RETURN c";

        using var session = _neo4jDriver.AsyncSession();
        var result = await session.RunAsync(singletonQuery, new { repositoryId = repositoryId.ToString() });
        
        await result.ForEachAsync(record =>
        {
            var classNode = record["c"].As<INode>();
            patterns.Add(new ArchitecturalPattern
            {
                Id = Guid.NewGuid(),
                Name = "Singleton",
                Type = PatternType.Creational,
                Description = "Ensures a class has only one instance and provides global access to it",
                Participants = new List<CodeEntity> { MapNodeToEntity(classNode) },
                ConfidenceScore = 0.9f,
                DetectedAt = DateTime.UtcNow,
                RepositoryId = repositoryId
            });
        });

        // Detect Factory patterns
        patterns.AddRange(await DetectFactoryPatternsAsync(repositoryId));

        return patterns;
    }
}
```

### Security Requirements
- Secure access to Neo4j database with proper authentication
- Input validation for all graph query parameters
- Rate limiting for complex graph analysis operations (10 per minute per user)
- Access control for sensitive architectural information
- Audit logging for pattern detection and relationship modifications

### Performance Requirements
- Knowledge graph construction < 10 minutes for repositories up to 5000 entities
- Graph queries execute < 3 seconds for typical operations
- Impact analysis completes < 30 seconds for 1000+ entity graphs
- Pattern detection accuracy > 85% for common design patterns
- Support concurrent graph operations for multiple repositories

## Implementation Guidance

### Recommended Development Approach
1. **Graph Schema Design**: Design comprehensive Neo4j schema with proper indexing
2. **Relationship Analysis**: Implement code relationship detection algorithms
3. **Pattern Detection**: Build AI-powered architectural pattern recognition
4. **Graph Algorithms**: Integrate Neo4j graph algorithms for analysis
5. **API Integration**: Add GraphQL operations for graph queries
6. **Visualization Support**: Prepare data structures for graph visualization

### Key Architectural Decisions
- Use Neo4j graph database for natural relationship storage and querying
- Implement hybrid pattern detection combining rule-based and AI approaches
- Store relationship strength metrics for sophisticated analysis
- Use graph algorithms for centrality and community detection
- Implement comprehensive caching for frequently accessed graph data

### Technical Risks and Mitigation
1. **Risk**: Graph complexity causing performance issues
   - **Mitigation**: Implement query optimization and result caching
   - **Fallback**: Hierarchical graph partitioning for large codebases

2. **Risk**: Pattern detection false positives
   - **Mitigation**: Implement confidence scoring and manual validation
   - **Fallback**: Rule-based fallback for critical pattern detection

3. **Risk**: Relationship inference accuracy
   - **Mitigation**: Multiple analysis passes and validation
   - **Fallback**: Manual relationship verification workflows

### Deployment Considerations
- Deploy with dedicated Neo4j instance for optimal performance
- Configure graph algorithm libraries and procedures
- Set up comprehensive monitoring for graph operation performance
- Implement backup strategies for graph data

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)
- **Graph Builder Service**
  - Entity extraction accuracy
  - Relationship detection logic
  - Pattern recognition algorithms
  - Performance optimization functions

- **Pattern Detection**
  - Individual pattern detection accuracy
  - Confidence score calculations
  - False positive/negative handling
  - Multi-pattern scenarios

- **Impact Analysis**
  - Dependency traversal algorithms
  - Impact severity calculations
  - Path finding accuracy
  - Performance with large graphs

### Integration Testing Requirements (30% coverage minimum)
- **End-to-End Graph Construction**
  - Complete knowledge graph building workflow
  - Multi-repository graph management
  - Real-time graph updates
  - Query performance validation

- **Pattern Detection Integration**
  - AI service integration for pattern recognition
  - Neo4j query execution
  - Result accuracy validation
  - Performance benchmarking

- **Graph Query Operations**
  - Complex graph traversal queries
  - Impact analysis accuracy
  - Visualization data preparation
  - Concurrent operation handling

### Test Data Requirements
- Sample repositories with known architectural patterns
- Performance benchmarking datasets
- Complex dependency scenarios
- Edge cases and malformed structures

## Quality Assurance

### Code Review Checkpoints
- [ ] Neo4j schema design supports efficient queries
- [ ] Graph algorithms are properly implemented
- [ ] Pattern detection logic is accurate and comprehensive
- [ ] Performance optimizations are implemented
- [ ] Relationship strength calculations are meaningful
- [ ] Error handling covers graph operation failures
- [ ] Security measures protect sensitive architectural data
- [ ] Monitoring and logging are comprehensive

### Definition of Done Checklist
- [ ] Knowledge graph construction works for all supported languages
- [ ] Architectural patterns are detected with high accuracy
- [ ] Impact analysis provides meaningful results
- [ ] Graph queries perform within requirements
- [ ] Integration tests pass for all scenarios
- [ ] Performance requirements are met
- [ ] Security review completed
- [ ] Documentation and visualization support prepared

### Monitoring and Observability
- **Custom Metrics**
  - Graph construction success rates and duration
  - Pattern detection accuracy and confidence scores
  - Query performance and complexity
  - Relationship inference success rates

- **Alerts**
  - Graph construction failures
  - Query performance degradation
  - Pattern detection confidence below threshold
  - Database connectivity issues

- **Dashboards**
  - Knowledge graph health and metrics
  - Pattern detection analytics
  - Graph query performance trends
  - Architectural insights usage

### Documentation Requirements
- Knowledge graph schema and relationship documentation
- Pattern detection algorithms and accuracy metrics
- Graph query examples and best practices
- Performance optimization guidelines
- Architectural analysis interpretation guide