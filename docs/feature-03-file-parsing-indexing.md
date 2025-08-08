# Feature 03: File Parsing and Code Structure Indexing

## Feature Overview

**Feature ID**: F03  
**Feature Name**: File Parsing and Code Structure Indexing  
**Phase**: Phase 1 (Weeks 1-4)  
**Bounded Context**: Repository Management Context  

### Business Value Proposition
Enable deep understanding of repository code structure by parsing and indexing files, classes, methods, and their relationships. This feature provides the foundation for AI-powered code analysis and documentation generation by creating a comprehensive structural representation of codebases.

### User Impact
- Developers can explore code structure and dependencies through intuitive queries
- Teams gain immediate insights into codebase architecture and complexity
- Repository maintainers can identify potential refactoring opportunities
- New team members can quickly understand code organization patterns

### Success Criteria
- Successfully parse and index 95% of files in supported languages (C#, JavaScript, Python, Go)
- Complete indexing of typical repository (1000 files) within 5 minutes
- Accurate relationship mapping between code entities (classes, methods, dependencies)
- Support for incremental indexing when files are modified

### Dependencies
- F01: Repository Connection and Management (for repository data and file access)
- F02: Core Infrastructure and DevOps Pipeline (for Azure services and deployment)

## Technical Specification

### Domain Model
```csharp
// Code Structure Aggregates
public class CodeFile
{
    public Guid Id { get; private set; }
    public string Path { get; private set; }
    public string Name { get; private set; }
    public string Extension { get; private set; }
    public string Language { get; private set; }
    public long Size { get; private set; }
    public int LineCount { get; private set; }
    public DateTime LastModified { get; private set; }
    public string ContentHash { get; private set; }
    public List<CodeClass> Classes { get; private set; }
    public List<CodeFunction> Functions { get; private set; }
    public List<ImportStatement> Imports { get; private set; }
    public CodeComplexity Complexity { get; private set; }
}

public class CodeClass
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Namespace { get; private set; }
    public string FullName { get; private set; }
    public AccessModifier AccessLevel { get; private set; }
    public bool IsAbstract { get; private set; }
    public bool IsInterface { get; private set; }
    public string BaseClass { get; private set; }
    public List<string> ImplementedInterfaces { get; private set; }
    public List<CodeMethod> Methods { get; private set; }
    public List<CodeProperty> Properties { get; private set; }
    public SourceLocation Location { get; private set; }
}

public class CodeMethod
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string ReturnType { get; private set; }
    public List<Parameter> Parameters { get; private set; }
    public AccessModifier AccessLevel { get; private set; }
    public bool IsStatic { get; private set; }
    public bool IsAsync { get; private set; }
    public SourceLocation Location { get; private set; }
    public CodeComplexity Complexity { get; private set; }
    public List<MethodCall> MethodCalls { get; private set; }
}

public class CodeComplexity
{
    public int CyclomaticComplexity { get; set; }
    public int CognitiveComplexity { get; set; }
    public int LinesOfCode { get; set; }
    public int Maintainability { get; set; }
}

public enum AccessModifier
{
    Public,
    Private,
    Protected,
    Internal,
    Package,
    Export
}
```

### API Specification

#### GraphQL Schema Extensions
```graphql
type CodeFile {
  id: ID!
  path: String!
  name: String!
  extension: String!
  language: String!
  size: Int!
  lineCount: Int!
  lastModified: DateTime!
  contentHash: String!
  classes: [CodeClass!]!
  functions: [CodeFunction!]!
  imports: [ImportStatement!]!
  complexity: CodeComplexity!
  repository: Repository!
}

type CodeClass {
  id: ID!
  name: String!
  namespace: String
  fullName: String!
  accessLevel: AccessModifier!
  isAbstract: Boolean!
  isInterface: Boolean!
  baseClass: String
  implementedInterfaces: [String!]!
  methods: [CodeMethod!]!
  properties: [CodeProperty!]!
  location: SourceLocation!
  file: CodeFile!
}

type CodeMethod {
  id: ID!
  name: String!
  returnType: String!
  parameters: [Parameter!]!
  accessLevel: AccessModifier!
  isStatic: Boolean!
  isAsync: Boolean!
  location: SourceLocation!
  complexity: CodeComplexity!
  methodCalls: [MethodCall!]!
  class: CodeClass
}

type CodeComplexity {
  cyclomaticComplexity: Int!
  cognitiveComplexity: Int!
  linesOfCode: Int!
  maintainabilityIndex: Int!
}

type SourceLocation {
  startLine: Int!
  endLine: Int!
  startColumn: Int!
  endColumn: Int!
}

type Parameter {
  name: String!
  type: String!
  isOptional: Boolean!
  defaultValue: String
}

type MethodCall {
  targetMethod: String!
  targetClass: String
  location: SourceLocation!
}

type ImportStatement {
  module: String!
  alias: String
  importedItems: [String!]!
  location: SourceLocation!
}

enum AccessModifier {
  PUBLIC
  PRIVATE
  PROTECTED
  INTERNAL
  PACKAGE
  EXPORT
}

# Extended Query types
extend type Repository {
  files(filter: FileFilter): [CodeFile!]!
  classes(filter: ClassFilter): [CodeClass!]!
  methods(filter: MethodFilter): [CodeMethod!]!
  codeComplexity: RepositoryComplexity!
}

input FileFilter {
  language: String
  extension: String
  pathContains: String
  minSize: Int
  maxSize: Int
  complexityThreshold: Int
}

input ClassFilter {
  namespace: String
  nameContains: String
  isInterface: Boolean
  accessLevel: AccessModifier
}

input MethodFilter {
  nameContains: String
  returnType: String
  accessLevel: AccessModifier
  minComplexity: Int
  maxComplexity: Int
}

type RepositoryComplexity {
  totalFiles: Int!
  totalClasses: Int!
  totalMethods: Int!
  averageComplexity: Float!
  languageBreakdown: [LanguageComplexity!]!
  hotspots: [ComplexityHotspot!]!
}

type ComplexityHotspot {
  file: CodeFile!
  class: CodeClass
  method: CodeMethod
  complexityScore: Int!
  reason: String!
}
```

### Database Schema Changes

#### Neo4j Schema Extensions
```cypher
// File nodes
CREATE CONSTRAINT file_id IF NOT EXISTS FOR (f:File) REQUIRE f.id IS UNIQUE;
CREATE CONSTRAINT file_path_repo IF NOT EXISTS FOR (f:File) REQUIRE (f.path, f.repositoryId) IS UNIQUE;

(:File {
  id: string,
  path: string,
  name: string,
  extension: string,
  language: string,
  size: integer,
  lineCount: integer,
  lastModified: datetime,
  contentHash: string,
  repositoryId: string,
  complexity: {
    cyclomaticComplexity: integer,
    cognitiveComplexity: integer,
    linesOfCode: integer,
    maintainabilityIndex: integer
  }
})

// Class nodes
CREATE CONSTRAINT class_id IF NOT EXISTS FOR (c:Class) REQUIRE c.id IS UNIQUE;
CREATE INDEX class_fullname IF NOT EXISTS FOR (c:Class) ON (c.fullName);

(:Class {
  id: string,
  name: string,
  namespace: string,
  fullName: string,
  accessLevel: string,
  isAbstract: boolean,
  isInterface: boolean,
  baseClass: string,
  implementedInterfaces: [string],
  location: {
    startLine: integer,
    endLine: integer,
    startColumn: integer,
    endColumn: integer
  },
  fileId: string,
  repositoryId: string
})

// Method nodes
CREATE CONSTRAINT method_id IF NOT EXISTS FOR (m:Method) REQUIRE m.id IS UNIQUE;
CREATE INDEX method_name_class IF NOT EXISTS FOR (m:Method) ON (m.name, m.classId);

(:Method {
  id: string,
  name: string,
  returnType: string,
  parameters: [{
    name: string,
    type: string,
    isOptional: boolean,
    defaultValue: string
  }],
  accessLevel: string,
  isStatic: boolean,
  isAsync: boolean,
  location: {
    startLine: integer,
    endLine: integer,
    startColumn: integer,
    endColumn: integer
  },
  complexity: {
    cyclomaticComplexity: integer,
    cognitiveComplexity: integer,
    linesOfCode: integer,
    maintainabilityIndex: integer
  },
  classId: string,
  fileId: string,
  repositoryId: string
})

// Property nodes
(:Property {
  id: string,
  name: string,
  type: string,
  accessLevel: string,
  isStatic: boolean,
  hasGetter: boolean,
  hasSetter: boolean,
  location: {
    startLine: integer,
    endLine: integer
  },
  classId: string,
  fileId: string
})

// Function nodes (for non-OOP languages)
(:Function {
  id: string,
  name: string,
  returnType: string,
  parameters: [{}],
  isAsync: boolean,
  isExported: boolean,
  location: {},
  complexity: {},
  fileId: string,
  repositoryId: string
})

// Import nodes
(:Import {
  id: string,
  module: string,
  alias: string,
  importedItems: [string],
  location: {},
  fileId: string,
  repositoryId: string
})

// Enhanced relationships
(:Repository)-[:CONTAINS]->(:File)
(:File)-[:DEFINES]->(:Class)
(:File)-[:DEFINES]->(:Function)
(:File)-[:IMPORTS]->(:Import)
(:Class)-[:HAS_METHOD]->(:Method)
(:Class)-[:HAS_PROPERTY]->(:Property)
(:Class)-[:INHERITS_FROM]->(:Class)
(:Class)-[:IMPLEMENTS]->(:Class) // For interfaces
(:Method)-[:CALLS]->(:Method)
(:Method)-[:ACCESSES]->(:Property)
(:File)-[:DEPENDS_ON]->(:File) // Based on imports
(:Import)-[:REFERENCES]->(:Class|:Function) // When resolvable
```

### Integration Points

#### Language-Specific Parsers
```csharp
public interface ILanguageParser
{
    string Language { get; }
    Task<ParseResult> ParseFileAsync(string filePath, string content);
    bool CanParse(string fileExtension);
}

public class CSharpParser : ILanguageParser
{
    public string Language => "C#";
    
    public async Task<ParseResult> ParseFileAsync(string filePath, string content)
    {
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = await tree.GetRootAsync();
        
        var result = new ParseResult
        {
            FilePath = filePath,
            Language = Language,
            Classes = ExtractClasses(root),
            Methods = ExtractMethods(root),
            Imports = ExtractImports(root),
            Complexity = CalculateComplexity(root)
        };
        
        return result;
    }
    
    public bool CanParse(string fileExtension) => 
        fileExtension.Equals(".cs", StringComparison.OrdinalIgnoreCase);
}

public class TypeScriptParser : ILanguageParser
{
    public string Language => "TypeScript";
    
    public async Task<ParseResult> ParseFileAsync(string filePath, string content)
    {
        // Use TypeScript compiler API or similar
        // Implementation specific to TypeScript parsing
        throw new NotImplementedException();
    }
    
    public bool CanParse(string fileExtension) => 
        fileExtension.EndsWith(".ts") || fileExtension.EndsWith(".tsx");
}
```

#### Parsing Service Architecture
```csharp
public interface ICodeParsingService
{
    Task<ParseResult> ParseFileAsync(Guid repositoryId, string filePath);
    Task<BatchParseResult> ParseRepositoryAsync(Guid repositoryId);
    Task<IncrementalParseResult> ParseChangedFilesAsync(Guid repositoryId, IEnumerable<string> changedFiles);
}

public class CodeParsingService : ICodeParsingService
{
    private readonly IEnumerable<ILanguageParser> _parsers;
    private readonly ICodeStructureRepository _codeRepository;
    private readonly IServiceBusClient _serviceBus;
    private readonly ILogger<CodeParsingService> _logger;
    
    public async Task<BatchParseResult> ParseRepositoryAsync(Guid repositoryId)
    {
        var repository = await _repositoryService.GetRepositoryAsync(repositoryId);
        var files = await GetRepositoryFilesAsync(repository.Path);
        
        var results = new List<ParseResult>();
        var errors = new List<ParseError>();
        
        await Parallel.ForEachAsync(files, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, async (file, ct) =>
        {
            try
            {
                var result = await ParseFileAsync(repositoryId, file.Path);
                results.Add(result);
                
                // Publish parsing completed event
                await _serviceBus.PublishAsync(new FileParsingCompletedEvent
                {
                    RepositoryId = repositoryId,
                    FilePath = file.Path,
                    ParsedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                errors.Add(new ParseError
                {
                    FilePath = file.Path,
                    Error = ex.Message
                });
                _logger.LogError(ex, "Failed to parse file {FilePath}", file.Path);
            }
        });
        
        return new BatchParseResult
        {
            RepositoryId = repositoryId,
            SuccessfulParses = results,
            Errors = errors,
            ParsedAt = DateTime.UtcNow
        };
    }
}
```

### Event-Driven Architecture

#### Parsing Events
```csharp
public class FileParsingRequestedEvent : IEvent
{
    public Guid RepositoryId { get; set; }
    public string FilePath { get; set; }
    public DateTime RequestedAt { get; set; }
}

public class FileParsingCompletedEvent : IEvent
{
    public Guid RepositoryId { get; set; }
    public string FilePath { get; set; }
    public ParseResult Result { get; set; }
    public DateTime ParsedAt { get; set; }
}

public class RepositoryParsingCompletedEvent : IEvent
{
    public Guid RepositoryId { get; set; }
    public BatchParseResult Result { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class CodeStructureIndexedEvent : IEvent
{
    public Guid RepositoryId { get; set; }
    public int TotalFiles { get; set; }
    public int TotalClasses { get; set; }
    public int TotalMethods { get; set; }
    public DateTime IndexedAt { get; set; }
}
```

### Security Requirements
- Input validation for all file paths and content
- Memory usage limits for large file processing
- Timeout limits for parsing operations (30 seconds per file)
- Sandboxed parsing environment to prevent code execution
- Virus scanning for uploaded files

### Performance Requirements
- Parse individual files < 5 seconds for files up to 10MB
- Process repository of 1000 files within 5 minutes
- Support concurrent parsing of up to 10 files
- Memory usage < 1GB during parsing operations
- Incremental parsing updates < 30 seconds

## Implementation Guidance

### Recommended Development Approach
1. **Parser Foundation**: Start with C# parser using Roslyn analyzers
2. **Storage Layer**: Implement Neo4j repository pattern for code structures
3. **Service Layer**: Build parsing service with proper error handling
4. **Event Integration**: Add Service Bus events for parsing lifecycle
5. **Language Expansion**: Add TypeScript/JavaScript parser
6. **Performance Optimization**: Implement parallel processing and caching

### Key Architectural Decisions
- Use Roslyn for C# parsing to leverage Microsoft's syntax analysis
- Implement parser factory pattern for extensible language support
- Store parsed structures in Neo4j for relationship queries
- Use Azure Functions for scalable parsing operations
- Implement incremental parsing to handle file changes efficiently

### Technical Risks and Mitigation
1. **Risk**: Parser memory usage with large files
   - **Mitigation**: Implement streaming parsers and memory limits
   - **Fallback**: Skip overly large files with warnings

2. **Risk**: Parsing accuracy for complex code constructs
   - **Mitigation**: Comprehensive test suite with real-world examples
   - **Fallback**: Best-effort parsing with error reporting

3. **Risk**: Neo4j performance with large object graphs
   - **Mitigation**: Batch operations and optimized Cypher queries
   - **Fallback**: Implement data archiving for old versions

### Deployment Considerations
- Deploy parsing logic as Azure Functions for auto-scaling
- Configure Service Bus queues for parsing workload distribution
- Implement health checks for parsing service availability
- Set up monitoring for parsing success rates and performance

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)
- **Language Parsers**
  - Syntax tree parsing accuracy
  - Code structure extraction
  - Error handling for malformed code
  - Performance with various file sizes

- **Code Structure Models**
  - Domain object creation and validation
  - Relationship mapping accuracy
  - Complexity calculations
  - Serialization/deserialization

- **Parsing Service**
  - File processing workflows
  - Batch parsing operations
  - Incremental update handling
  - Error recovery mechanisms

### Integration Testing Requirements (30% coverage minimum)
- **End-to-End Parsing Workflow**
  - Complete repository parsing process
  - Neo4j data persistence verification
  - Event publishing validation
  - GraphQL query result accuracy

- **Multi-Language Support**
  - C# repository parsing
  - JavaScript/TypeScript parsing
  - Python and Go parsing (future)
  - Mixed-language repository handling

- **Performance Testing**
  - Large repository processing (10,000+ files)
  - Concurrent parsing operations
  - Memory usage under load
  - Neo4j query performance

### Test Data Requirements
- Sample repositories with various languages and complexities
- Edge cases: empty files, malformed syntax, extremely large files
- Performance test repositories with known metrics
- Real-world open source repositories for validation

## Quality Assurance

### Code Review Checkpoints
- [ ] Parser implementations follow language-specific best practices
- [ ] Domain models properly encapsulate code structure concepts
- [ ] Neo4j schema design supports efficient queries
- [ ] Error handling covers all failure scenarios
- [ ] Performance optimizations are implemented
- [ ] Memory usage is controlled and monitored
- [ ] Event-driven communication is properly implemented
- [ ] Security considerations are addressed

### Definition of Done Checklist
- [ ] All supported languages parse correctly (C#, TypeScript/JavaScript)
- [ ] Code structures are accurately stored in Neo4j
- [ ] GraphQL API returns parsed code information
- [ ] Parsing events are published to Service Bus
- [ ] Performance requirements are met
- [ ] Integration tests pass for all scenarios
- [ ] Error handling works for invalid inputs
- [ ] Memory usage stays within limits
- [ ] Security review completed
- [ ] Documentation updated

### Monitoring and Observability
- **Custom Metrics**
  - Files parsed per minute
  - Parsing success/failure rates by language
  - Average parsing time by file size
  - Memory usage during parsing operations
  - Neo4j query performance

- **Alerts**
  - Parsing failure rate >5%
  - Memory usage >80% threshold
  - Neo4j connection issues
  - Service Bus message processing failures
  - Individual file parsing timeout

- **Dashboards**
  - Parsing pipeline health overview
  - Language-specific success rates
  - Repository complexity metrics
  - Performance trends over time

### Documentation Requirements
- Parser implementation guide for new languages
- Code structure schema documentation
- Neo4j query examples and patterns
- Performance tuning guide
- Troubleshooting common parsing issues