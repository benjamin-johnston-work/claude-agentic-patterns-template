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
- `.\scripts\Test-ArchieEnvironment.ps1` - Environment health check

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
- **Backend**: .NET 9.0 Web API
- **Data Storage**: Azure Search
- **Search Engine**: Azure AI Search
- **AI**: Azure OpenAI GPT-4.1
- **Testing**: NUnit 4.1.0, Moq
- **GraphQL**: HotChocolate

### Architecture Pattern
- **Clean Architecture**: Domain, Application, Infrastructure, API layers
- **Domain-Driven Design**: Value objects, entities, use cases
- **Repository Pattern**: Data access abstraction via Azure Search
- **CQRS**: Command/Query separation for complex operations

## Core Features & Implementation Status

### ✅ Completed Features
- **Feature 01**: Repository Connection & Management
- **Feature 02**: Azure AI Search Implementation
- **Feature 03**: Enhanced AI-Powered Documentation Generation
  - Content-based analysis (not filename-based)
  - Project purpose extraction from README analysis
  - Component relationship mapping
  - Domain-specific documentation generation
  
### 🔧 Key Services
- `ContentSummarizationService`: Analyzes file content for AI context
- `RepositoryAnalysisService`: Enhanced with content-driven understanding
- `AIDocumentationGeneratorService`: Generates accurate, context-aware documentation

## Code Style & Best Practices

- **Modular Design**: Files under 500 lines
- **Environment Safety**: Never hardcode secrets
- **Test-First**: Write tests before implementation using NUnit
- **Clean Architecture**: Separate concerns
- **Documentation**: Keep updated

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
- Never hardcode Azure OpenAI endpoints or API keys
- Maintain Azure subscription details accuracy
- Keep environment-specific settings in appsettings files

## Important Instructions
- Do what has been asked; nothing more, nothing less
- NEVER create files unless they're absolutely necessary for achieving your goal
- ALWAYS prefer editing an existing file to creating a new one
- NEVER proactively create documentation files (*.md) or README files
- Never save working files, text/mds and tests to the root folder
- Always test with Test-ArchieEnvironment.ps1 before saying a feature is complete.
- You always need to use scripts to start and stop application from the scripts folder. You should always use the stop script before the start