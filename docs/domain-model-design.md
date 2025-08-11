# Domain Model Design for DeepWiki Clone with Azure AI Search

## Overview

This document defines the optimal domain model to support the full DeepWiki vision - an AI-powered repository documentation platform for conversational code exploration. The model is designed for Azure AI Search with vector embeddings.

## Core Design Principles

1. **Azure AI Search Document Model**: Each entity becomes a searchable document with embeddings
2. **Rich Metadata**: Support conversational AI queries about code structure
3. **Bounded Context Separation**: Clear separation between Repository, Documentation, AI Analysis, and Knowledge Graph contexts
4. **Relationship Modeling**: Use document references and search filters instead of graph relationships

## Domain Model by Bounded Context

### 1. Repository Management Context

#### Repository Entity (Enhanced)
```csharp
public class Repository : BaseEntity, IAggregateRoot
{
    // Core Properties
    public string Name { get; private set; }
    public string Url { get; private set; }
    public string Owner { get; private set; }  // NEW: GitHub owner/org
    public string FullName { get; private set; } // NEW: owner/repo format
    public string CloneUrl { get; private set; } // NEW: for cloning
    public string Language { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastPushedAt { get; private set; } // NEW
    
    // Repository Metadata
    public bool IsPrivate { get; private set; } // NEW
    public bool IsFork { get; private set; } // NEW
    public bool IsArchived { get; private set; } // NEW
    public string DefaultBranch { get; private set; } = "main"; // NEW
    public string? License { get; private set; } // NEW
    public int Size { get; private set; } // NEW: repository size
    public int StargazersCount { get; private set; } // NEW
    public int ForksCount { get; private set; } // NEW
    public int OpenIssuesCount { get; private set; } // NEW
    
    // Status and Processing
    public RepositoryStatus Status { get; private set; }
    public RepositoryStatistics Statistics { get; private set; }
    public DateTime? LastAnalyzed { get; private set; } // NEW
    public DateTime? LastIndexed { get; private set; } // NEW
    
    // Collections
    private readonly List<Branch> _branches = new();
    private readonly List<string> _topics = new(); // NEW: repository topics
    private readonly List<RepositoryFile> _files = new(); // NEW
    
    public IReadOnlyList<Branch> Branches => _branches.AsReadOnly();
    public IReadOnlyList<string> Topics => _topics.AsReadOnly();
    public IReadOnlyList<RepositoryFile> Files => _files.AsReadOnly();
}
```

#### New: RepositoryFile Entity
```csharp
public class RepositoryFile : BaseEntity
{
    public Guid RepositoryId { get; private set; }
    public string Path { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Extension { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public int Size { get; private set; }
    public int LineCount { get; private set; }
    public string Sha { get; private set; } = string.Empty; // Git SHA
    public DateTime LastModified { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty; // For AI analysis
    
    // AI/Search Properties
    public List<string> CodeSymbols { get; private set; } = new(); // Functions, classes, etc.
    public string Summary { get; private set; } = string.Empty; // AI-generated summary
    public float[] ContentEmbedding { get; private set; } = Array.Empty<float>(); // For vector search
}
```

#### Enhanced Branch Entity
```csharp
public class Branch : BaseEntity
{
    public Guid RepositoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public bool IsProtected { get; private set; } // NEW
    public string LastCommitSha { get; private set; } = string.Empty; // NEW
    public DateTime? LastCommitDate { get; private set; } // NEW
    public string? LastCommitMessage { get; private set; } // NEW
    public string? LastCommitAuthor { get; private set; } // NEW
}
```

#### New: Commit Entity
```csharp
public class Commit : BaseEntity
{
    public Guid RepositoryId { get; private set; }
    public string Sha { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public string AuthorEmail { get; private set; } = string.Empty;
    public DateTime AuthorDate { get; private set; }
    public string CommitterName { get; private set; } = string.Empty;
    public string CommitterEmail { get; private set; } = string.Empty;
    public DateTime CommitterDate { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public List<string> ModifiedFiles { get; private set; } = new();
}
```

### 2. Documentation Context

#### Documentation Entity
```csharp
public class Documentation : BaseEntity, IAggregateRoot
{
    public Guid RepositoryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public DateTime GeneratedAt { get; private set; }
    public string GeneratedBy { get; private set; } = string.Empty; // AI model info
    public DocumentationType Type { get; private set; }
    public DocumentationStatus Status { get; private set; }
    
    // AI Properties
    public float[] ContentEmbedding { get; private set; } = Array.Empty<float>();
    public string Summary { get; private set; } = string.Empty;
    public List<string> Keywords { get; private set; } = new();
    
    // Collections
    private readonly List<DocumentationSection> _sections = new();
    public IReadOnlyList<DocumentationSection> Sections => _sections.AsReadOnly();
}

public enum DocumentationType
{
    Overview,
    Architecture,
    API,
    Tutorial,
    Concepts,
    FAQ
}

public enum DocumentationStatus
{
    Generating,
    Ready,
    Outdated,
    Error
}
```

#### DocumentationSection Entity
```csharp
public class DocumentationSection : BaseEntity
{
    public Guid DocumentationId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public SectionType Type { get; private set; }
    public float[] ContentEmbedding { get; private set; } = Array.Empty<float>();
    
    // Code References
    public List<CodeReference> CodeReferences { get; private set; } = new();
}

public class CodeReference
{
    public string FilePath { get; set; } = string.Empty;
    public string CodeSymbol { get; set; } = string.Empty; // Class, method, etc.
    public int? StartLine { get; set; }
    public int? EndLine { get; set; }
    public string Context { get; set; } = string.Empty;
}

public enum SectionType
{
    Introduction,
    QuickStart,
    Concepts,
    Examples,
    Reference,
    Troubleshooting
}
```

### 3. AI Analysis Context

#### AnalysisJob Entity
```csharp
public class AnalysisJob : BaseEntity, IAggregateRoot
{
    public Guid RepositoryId { get; private set; }
    public AnalysisType Type { get; private set; }
    public AnalysisStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int ProcessedFiles { get; private set; }
    public int TotalFiles { get; private set; }
    public double ProgressPercentage => TotalFiles > 0 ? (double)ProcessedFiles / TotalFiles * 100 : 0;
    
    // Results
    public List<string> GeneratedInsights { get; private set; } = new();
    public Dictionary<string, object> Metadata { get; private set; } = new();
}

public enum AnalysisType
{
    FullRepository,
    Incremental,
    DocumentationGeneration,
    PatternDetection,
    DependencyAnalysis
}

public enum AnalysisStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}
```

#### Insight Entity
```csharp
public class Insight : BaseEntity
{
    public Guid RepositoryId { get; private set; }
    public InsightType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public float Confidence { get; private set; } // 0.0 to 1.0
    public DateTime GeneratedAt { get; private set; }
    public string GeneratedBy { get; private set; } = string.Empty; // AI model
    public InsightSeverity Severity { get; private set; }
    
    // Context
    public List<string> RelatedFiles { get; private set; } = new();
    public List<string> RelatedCodeSymbols { get; private set; } = new();
    public Dictionary<string, object> Evidence { get; private set; } = new();
    
    // AI Properties
    public float[] ContentEmbedding { get; private set; } = Array.Empty<float>();
}

public enum InsightType
{
    Architecture,
    CodeQuality,
    Performance,
    Security,
    Patterns,
    Dependencies,
    Complexity,
    Documentation
}

public enum InsightSeverity
{
    Info,
    Low,
    Medium,
    High,
    Critical
}
```

#### Query Entity (for conversational AI)
```csharp
public class Query : BaseEntity
{
    public Guid RepositoryId { get; private set; }
    public string SessionId { get; private set; } = string.Empty;
    public string Question { get; private set; } = string.Empty;
    public string Response { get; private set; } = string.Empty;
    public DateTime AskedAt { get; private set; }
    public DateTime? AnsweredAt { get; private set; }
    public QueryType Type { get; private set; }
    public float ResponseConfidence { get; private set; }
    
    // Context
    public List<string> RelatedFiles { get; private set; } = new();
    public List<string> UsedInsights { get; private set; } = new();
    public string Context { get; private set; } = string.Empty; // Conversation context
    
    // AI Properties
    public float[] QuestionEmbedding { get; private set; } = Array.Empty<float>();
    public float[] ResponseEmbedding { get; private set; } = Array.Empty<float>();
}

public enum QueryType
{
    General,
    Architecture,
    Implementation,
    Documentation,
    Debugging,
    Patterns,
    Dependencies
}
```

### 4. Knowledge Graph Context (Adapted for Azure AI Search)

#### CodeEntity Entity
```csharp
public class CodeEntity : BaseEntity
{
    public Guid RepositoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty; // Namespace.Class.Method
    public CodeEntityType Type { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public int StartLine { get; private set; }
    public int EndLine { get; private set; }
    public string Content { get; private set; } = string.Empty; // Code content
    
    // Metadata
    public string? Namespace { get; private set; }
    public string? ParentEntity { get; private set; } // Parent class/module
    public List<string> Parameters { get; private set; } = new(); // For methods
    public string? ReturnType { get; private set; } // For methods
    public string? AccessModifier { get; private set; }
    public List<string> Modifiers { get; private set; } = new(); // static, abstract, etc.
    
    // Relationships (as references for Azure AI Search)
    public List<string> Dependencies { get; private set; } = new(); // What this depends on
    public List<string> Dependents { get; private set; } = new(); // What depends on this
    public List<string> Implements { get; private set; } = new(); // Interfaces
    public List<string> Extends { get; private set; } = new(); // Base classes
    public List<string> Calls { get; private set; } = new(); // Methods called
    public List<string> CalledBy { get; private set; } = new(); // Methods that call this
    
    // AI Properties
    public float[] ContentEmbedding { get; private set; } = Array.Empty<float>();
    public string Summary { get; private set; } = string.Empty; // AI-generated summary
    public List<string> Concepts { get; private set; } = new(); // Related concepts
}

public enum CodeEntityType
{
    Namespace,
    Class,
    Interface,
    Enum,
    Struct,
    Method,
    Function,
    Property,
    Field,
    Variable,
    Constant,
    Module,
    Package
}
```

#### Concept Entity
```csharp
public class Concept : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ConceptCategory Category { get; private set; }
    public float Confidence { get; private set; }
    public DateTime DiscoveredAt { get; private set; }
    
    // Repository Context
    public List<Guid> RelatedRepositories { get; private set; } = new();
    public List<string> RelatedCodeEntities { get; private set; } = new();
    public List<string> RelatedFiles { get; private set; } = new();
    public List<string> Examples { get; private set; } = new(); // Code examples
    
    // AI Properties  
    public float[] ContentEmbedding { get; private set; } = Array.Empty<float>();
    public List<string> Synonyms { get; private set; } = new();
    public List<string> RelatedConcepts { get; private set; } = new();
}

public enum ConceptCategory
{
    Pattern,
    Architecture,
    Algorithm,
    DataStructure,
    Framework,
    Library,
    Methodology,
    Principle,
    AntiPattern,
    Refactoring
}
```

## Azure AI Search Document Mapping

Each entity becomes a searchable document in Azure AI Search with:

1. **Core Fields**: Entity properties mapped to search fields
2. **Vector Fields**: Embeddings for semantic search
3. **Facet Fields**: For filtering (repository, language, type, etc.)
4. **Full-Text Fields**: For keyword search (content, description, etc.)
5. **Relationship Fields**: References to related entities

## Benefits of This Model

1. **Supports Full DeepWiki Vision**: All bounded contexts covered
2. **Azure AI Search Optimized**: Document-based with rich metadata
3. **Conversational AI Ready**: Embeddings and context for AI queries
4. **Scalable**: Can handle large repositories with complex relationships
5. **Flexible**: Easy to extend with new entity types and properties
6. **Search-Optimized**: Designed for fast semantic and keyword search