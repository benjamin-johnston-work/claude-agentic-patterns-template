# Claude Code Configuration

## Project Overview

This project is configured for development with Claude Code CLI.

## Local Development Dependencies

### Required Software
- .NET 9.0 SDK or later
- Git for Windows 2.30+
- Node.js 20.x LTS
- PowerShell 5.1+ or PowerShell Core 7.0+

### Quick Setup
Run the automated setup script from the project root:
```powershell
# Run as Administrator
.\scripts\Setup-ArchieDevEnvironment.ps1
```

### Manual Setup (if automated setup fails)
1. Install .NET 9.0 SDK from https://dotnet.microsoft.com/download
4. Install Git for Windows
5. Run `dotnet restore` to restore NuGet packages

### Health Check
Validate your environment setup:
```powershell
.\scripts\Test-ArchieEnvironment.ps1
```

## Azure Environment Configuration

### Azure Subscription Details
- **Subscription**: RACT-AI-Non-Production
- **Subscription ID**: `c2edcde5-7ea8-4c97-808b-7993f422725d`
- **Tenant ID**: `ceb29e10-60d1-4f6b-86b1-b7d497b5b66e`
- **Tenant Domain**: `ract.com.au`
- **Primary User**: `b.johnston@ract.com.au`

### Resource Groups
- **Development**: `ract-ai-dev-agents-rg` (Australia East)

### Azure Resources (Development Environment)
- **Azure OpenAI**: `ract-ai-foundry-dev` (Australia East)
- **Azure AI Search**: `ract-archie-search-dev` (Australia East)
- **Key Vault**: `ract-archie-kv-dev` (Australia East)
- **Storage Account**: `ractknowledgemvp` (Australia East)

### Common Azure CLI Commands
```bash
# Login and set subscription
az login
az account set --subscription "c2edcde5-7ea8-4c97-808b-7993f422725d"

# Resource group operations
az group list --location australiaeast
az resource list --resource-group ract-ai-dev-agents-rg --output table

# Azure AI Search operations
az search service show --name ract-archie-search-dev --resource-group ract-ai-dev-agents-rg
az search admin-key show --service-name ract-archie-search-dev --resource-group ract-ai-dev-agents-rg

# Azure OpenAI operations
az cognitiveservices account show --name ract-ai-foundry-dev --resource-group ract-ai-dev-agents-rg
az cognitiveservices account keys list --name ract-ai-foundry-dev --resource-group ract-ai-dev-agents-rg

# Key Vault operations
az keyvault show --name ract-archie-kv-dev --resource-group ract-ai-dev-agents-rg
az keyvault secret list --vault-name ract-archie-kv-dev
```

## Common Commands
- `'powershell "Stop-Process-Force"'` - Stop Process

## Enhanced Build Commands

### Development Workflow
- `dotnet build` - Build entire solution
- `dotnet test` - Run all tests  
- `dotnet run --project src\Archie.Api` - Start API server
- `.\scripts\Start-ArchieDevEnvironment.ps1` - Start full development environment
- `.\scripts\Stop-ArchieDevEnvironment.ps1` - Stop development environment
- `.\scripts\Test-ArchieEnvironment.ps1` - Environment health check

### Frontend Development
- `cd src/frontend && npm install` - Install frontend dependencies
- `npm run dev` - Start Next.js development server (http://localhost:3000)
- `npm run build` - Build production frontend
- `npm run test` - Run Jest unit tests
- `npx playwright test` - Run end-to-end tests

### Testing & Validation
- **API Testing**: Use GraphQL playground at `/graphql` when API running
- **Documentation Testing**: Test with diverse repositories to validate accuracy
- **Content Analysis Validation**: Check logs for proper content-based analysis

### Troubleshooting
- Check Azure OpenAI connectivity for documentation generation
- Verify Azure Search connection for repository storage
- Monitor ContentSummarizationService for file processing errors

## Architecture & Tech Stack

### Primary Technologies
- **Backend**: .NET 9.0 Web API with GraphQL (HotChocolate)
- **Frontend**: Next.js 14 with TypeScript, Tailwind CSS, shadcn/ui
- **Architecture**: Microsoft GraphRAG with Azure AI Search vector indexing
- **Data Storage**: Azure AI Search with semantic search capabilities
- **AI Services**: Azure OpenAI GPT-4.1, text-embedding-ada-002
- **Authentication**: NextAuth.js with Azure AD integration
- **Testing**: NUnit 4.1.0, Moq, Jest (frontend), Playwright (E2E)
- **Infrastructure**: Azure Cloud Services (Search, OpenAI, Key Vault)

### Architecture Pattern
- **Clean Architecture**: Domain, Application, Infrastructure, API layers with strict dependency inversion
- **Microsoft GraphRAG**: Knowledge graph construction with semantic search and AI-powered insights
- **Domain-Driven Design**: Rich value objects, aggregates, domain events, and use cases
- **Repository Pattern**: Data access abstraction via Azure Search with vector embeddings
- **CQRS**: Command/Query separation with GraphQL mutations/queries
- **Event-Driven Architecture**: Domain events for cross-cutting concerns
- **Microservices-Ready**: Structured for future service decomposition

## Core Features & Implementation Status

### ✅ Completed Features (F01-F14)
- **F01: Repository Connection & Management**: GitHub integration, validation, indexing pipeline
- **F02: Azure AI Search Implementation**: Vector search, semantic indexing, document storage
- **F03: AI-Powered Documentation Generation**: Content-based analysis, contextual documentation
- **F04: Conversational Query Interface**: Natural language repository queries with context
- **F05: Semantic Kernel Code Analysis**: Advanced code understanding and relationship mapping
- **F06: Event-Driven Architecture**: Domain events, messaging, cross-service communication  
- **F07: Rate Limiting & API Optimization**: Request throttling, performance monitoring
- **F08: GitHub Webhooks & Real-time Updates**: Live repository synchronization
- **F09: Azure DevOps CI/CD Infrastructure**: Automated deployment pipelines
- **F10: Authentication & Security**: Azure AD integration, role-based access control
- **F11: Performance Monitoring & Observability**: Application insights, telemetry
- **F12: GraphRAG Knowledge Construction**: Entity extraction, relationship mapping
- **F13: GraphRAG Visual Discovery Interface**: Interactive knowledge graph exploration
- **F14: Enterprise GraphRAG Analytics**: Compliance reporting, usage analytics
  
### 🔧 Key Services
- **Core Analysis Services**:
  - `ContentSummarizationService`: File content analysis for AI context
  - `RepositoryAnalysisService`: Content-driven repository understanding
  - `AIDocumentationGeneratorService`: Context-aware documentation generation
- **Azure Integration Services**:
  - `AzureSearchService`: Vector search and semantic indexing
  - `AzureOpenAIEmbeddingService`: Text embedding generation
  - `CodeSymbolExtractor`: Programming language symbol extraction
- **Repository Services**:
  - `GitHubService`: GitHub API integration and webhook handling
  - `GitRepositoryService`: Git operations and repository management
  - `RepositoryIndexingService`: Automated repository content indexing
- **Conversation Services**:
  - `ConversationalAIService`: Natural language query processing
  - `ConversationContextService`: Context management for conversations

## Code Style & Best Practices

- **Modular Design**: Files under 500 lines
- **Environment Safety**: Never hardcode secrets
- **Test-First**: Write tests before implementation using NUnit
- **Clean Architecture**: Separate concerns
- **Documentation**: Keep updated

## Git Commit Conventions

**All commits must follow conventional commit format with proper prefixing:**

### Commit Types
- **Feature**: New feature implementation (`Feature: Add repository indexing pipeline`)
- **Task**: Development tasks, refactoring, improvements (`Task: Refactor AzureSearchService for better error handling`)
- **Bug**: Bug fixes (`Bug: Fix null reference in DocumentationGenerator`)  
- **Hotfix**: Critical production fixes (`Hotfix: Resolve Azure Search connection timeout`)
- **Security**: Security-related changes (`Security: Remove hardcoded API keys from configuration`)
- **Docs**: Documentation updates (`Docs: Update API integration guide`)
- **Test**: Test additions/modifications (`Test: Add unit tests for ConversationService`)
- **CI/CD**: Build, deployment, pipeline changes (`CI/CD: Update Azure deployment pipeline`)

### Format Examples
```
Feature: Implement F05 semantic kernel code analysis foundation
Task: Optimize Azure Search query performance for large repositories  
Bug: Fix repository validation failing on private repositories
Security: Implement local secrets configuration pattern
```

### Commit Message Structure
```
Type: Brief description (50 chars max)

Optional detailed explanation of what and why.
Include breaking changes, migration notes.

🤖 Generated with [Claude Code](https://claude.ai/code)
Co-Authored-By: Claude <noreply@anthropic.com>
```

## Security Practices

### Local Secrets Management
**NEVER commit secrets to the repository**. Follow this pattern:

1. **Development Environment**:
   - Real API keys stored in `appsettings.Local.json` (not tracked by git)
   - Placeholders in `appsettings.Development.json` (tracked by git)
   - Application configured to load local secrets via `Program.cs`

2. **Required Local Secrets File** (`src/Archie.Api/appsettings.Local.json`):
```json
{
    "GitHub": {
        "DefaultAccessToken": "your-actual-github-token"
    },
    "AzureSearch": {
        "AdminKey": "your-actual-azure-search-key",
        "QueryKey": "your-actual-azure-search-query-key"
    },
    "AzureOpenAI": {
        "ApiKey": "your-actual-azure-openai-key"
    }
}
```

3. **Production Deployment**: 
   - Use Azure Key Vault for secret storage
   - Environment variables for CI/CD pipelines
   - Managed identities where possible

4. **Git History Safety**:
   - `.gitignore` excludes `appsettings.Local.json`
   - Git history cleaned of any committed secrets
   - GitHub secret scanning protection enabled

## Testing Framework

- **Unit Testing**: NUnit 4.1.0 with constraint-based assertions
- **Test Structure**: `[TestFixture]` classes with `[Test]` methods and `[SetUp]` initialization
- **Test Organization**: Mirror source code structure in test projects
- **Mocking**: Moq framework for dependency isolation

## File Organization Rules

**Organize files in appropriate subdirectories:**
- `/src` - Source code files
- `/tests` - Test files
- `/docs` - Documentation and markdown files
- `/config` - Configuration files
- `/scripts` - Utility scripts
- `/examples` - Example code

## Review Process Guidelines

Before submitting any code changes:
1. **Build & Test**: Run `dotnet build` and `dotnet test`
2. **Environment Validation**: Run `.\scripts\Test-ArchieEnvironment.ps1`
3. **Architecture Compliance**: ✅/❌ Clean Architecture principles followed
4. **Testing Coverage**: ✅/❌ Unit tests written for new functionality
5. **Documentation**: ✅/❌ Updates maintain accuracy with implementation
6. **No Hardcoded Secrets**: ✅/❌ All sensitive data uses configuration

## AI Development Guidelines

### Documentation Generation (Feature 03)
- **Content Analysis**: Always analyze actual file content, never assume from filenames
- **Project Purpose**: Extract from README analysis, not metadata guessing  
- **Domain Detection**: Use keyword analysis for business domains (Game, Web API, Library, etc.)
- **Component Mapping**: Build relationships between code components for "How It Works" sections

### Prompt Engineering
- Include actual project purpose in AI prompts
- Use code examples from content analysis
- Apply domain-specific instructions (game vs API vs library documentation)

## ⚠️ Do Not Modify

### Critical Implementation Files
- `ContentSummarizationService.cs` - Contains language-specific content analysis patterns
- `AIDocumentationGeneratorService.BuildSectionPrompt()` - Enhanced AI context generation
- `RepositoryAnalysisContext` - Enhanced with content analysis properties
- Domain value objects: `ProjectPurpose`, `ComponentRelationshipMap`, `ContentSummary`

### Configuration Safety
- **NEVER hardcode secrets**: Use `appsettings.Local.json` pattern for development
- **Azure Key Vault**: Production secrets stored securely in Azure Key Vault
- **Environment Variables**: CI/CD pipelines use environment-based configuration
- **Managed Identities**: Prefer Azure managed identities over API keys where possible
- **Git History**: All secrets removed from git history using filter-branch
- **Subscription Details**: Maintain Azure subscription accuracy for resource access

## Important Instructions
- Do what has been asked; nothing more, nothing less
- NEVER create files unless they're absolutely necessary for achieving your goal
- ALWAYS prefer editing an existing file to creating a new one
- NEVER proactively create documentation files (*.md) or README files
- Never save working files, text/mds and tests to the root folder
- Always test with Test-ArchieEnvironment.ps1 before saying a feature is complete
- You always need to use scripts to start and stop application from the scripts folder. You should always use the stop script before the start
- **Follow git commit conventions**: Use proper prefixes (Feature/Task/Bug/Hotfix/Security/etc.)
- **Never commit secrets**: Always use `appsettings.Local.json` pattern for development keys
- **Frontend development**: Run `npm run dev` for Next.js, ensure both API and frontend are running for full functionality