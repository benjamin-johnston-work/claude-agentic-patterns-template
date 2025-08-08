# Feature 02: Core Infrastructure and DevOps Pipeline

## Feature Overview

**Feature ID**: F02  
**Feature Name**: Core Infrastructure and DevOps Pipeline  
**Phase**: Phase 1 (Weeks 1-4)  
**Bounded Context**: Cross-cutting Infrastructure  

### Business Value Proposition
Establish the foundational Azure infrastructure and DevOps pipeline that enables reliable, scalable deployment and operation of the DeepWiki platform. This feature provides the essential operational capabilities needed for all subsequent feature releases.

### User Impact
- Development teams can deploy features with confidence through automated pipelines
- Operations teams have comprehensive monitoring and alerting capabilities
- End users benefit from reliable service availability and performance
- Stakeholders have visibility into system health and performance metrics

### Success Criteria
- Automated deployment pipeline with >90% success rate
- Infrastructure provisioned consistently across all environments
- Comprehensive monitoring and alerting operational
- Security scanning integrated into CI/CD pipeline
- Zero-downtime deployment capability demonstrated

### Dependencies
- Azure subscription and resource group established
- Neo4j AuraDB instance provisioned
- GitHub repository configured with appropriate permissions

## Technical Specification

### Azure Resources Architecture

#### Core Azure Services Configuration
```bicep
// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-deepwiki-${environment}'
  location: location
  tags: commonTags
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-deepwiki-${environment}'
  location: location
  sku: {
    name: 'B1'  // Basic tier for Phase 1
    capacity: 1
  }
  properties: {
    reserved: false
  }
}

// App Service for GraphQL API
resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-deepwiki-api-${environment}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v6.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment
        }
        {
          name: 'ApplicationInsights__ConnectionString'
          value: applicationInsights.properties.ConnectionString
        }
      ]
    }
  }
}

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: 'sb-deepwiki-${environment}'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {}
}

// Service Bus Queues
resource repositoryAnalysisQueue 'Microsoft.ServiceBus/namespaces/queues@2022-01-01-preview' = {
  parent: serviceBusNamespace
  name: 'repository-analysis'
  properties: {
    maxDeliveryCount: 3
    deadLetteringOnMessageExpiration: true
    defaultMessageTimeToLive: 'P1D'
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: 'kv-deepwiki-${environment}'
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: webApp.identity.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
    enableRbacAuthorization: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 30
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai-deepwiki-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Redfield'
    Request_Source: 'IbizaAIExtension'
  }
}

// Storage Account for artifacts
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: 'stdeepwiki${environment}${uniqueString(rg.id)}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        blob: {
          enabled: true
        }
        file: {
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
  }
}
```

### DevOps Pipeline Configuration

#### Azure DevOps Pipeline (azure-pipelines.yml)
```yaml
trigger:
  branches:
    include:
      - main
      - develop
      - feature/*

variables:
  buildConfiguration: 'Release'
  dotNetFramework: 'net6.0'
  dotNetVersion: '6.0.x'
  azureServiceConnectionDev: 'azure-deepwiki-dev'
  azureServiceConnectionProd: 'azure-deepwiki-prod'

stages:
  - stage: Build
    displayName: 'Build and Test'
    jobs:
      - job: Build
        displayName: 'Build Application'
        pool:
          vmImage: 'ubuntu-latest'
        steps:
          - task: UseDotNet@2
            displayName: 'Use .NET $(dotNetVersion)'
            inputs:
              version: '$(dotNetVersion)'

          - task: DotNetCoreCLI@2
            displayName: 'Restore packages'
            inputs:
              command: 'restore'
              projects: '**/*.csproj'

          - task: DotNetCoreCLI@2
            displayName: 'Build application'
            inputs:
              command: 'build'
              projects: '**/*.csproj'
              arguments: '--configuration $(buildConfiguration) --no-restore'

          - task: DotNetCoreCLI@2
            displayName: 'Run unit tests'
            inputs:
              command: 'test'
              projects: '**/*Tests/*.csproj'
              arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage" --results-directory $(Agent.TempDirectory)'
              publishTestResults: true

          - task: PublishCodeCoverageResults@1
            displayName: 'Publish code coverage'
            inputs:
              codeCoverageTool: 'Cobertura'
              summaryFileLocation: '$(Agent.TempDirectory)/*/coverage.cobertura.xml'

          - task: DotNetCoreCLI@2
            displayName: 'Publish application'
            inputs:
              command: 'publish'
              projects: '**/*.csproj'
              arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
              publishWebProjects: true

          - task: PublishBuildArtifacts@1
            displayName: 'Publish artifacts'
            inputs:
              pathToPublish: '$(Build.ArtifactStagingDirectory)'
              artifactName: 'drop'

  - stage: SecurityScan
    displayName: 'Security Scanning'
    dependsOn: Build
    jobs:
      - job: SecurityScan
        displayName: 'Security Analysis'
        pool:
          vmImage: 'ubuntu-latest'
        steps:
          - task: DotNetCoreCLI@2
            displayName: 'Install security scan tool'
            inputs:
              command: 'custom'
              custom: 'tool'
              arguments: 'install --global security-scan'

          - script: |
              security-scan **/*.csproj --output $(Build.ArtifactStagingDirectory)/security-report.json
            displayName: 'Run security scan'

          - task: PublishBuildArtifacts@1
            displayName: 'Publish security report'
            inputs:
              pathToPublish: '$(Build.ArtifactStagingDirectory)/security-report.json'
              artifactName: 'security-report'

  - stage: DeployDev
    displayName: 'Deploy to Development'
    dependsOn: [Build, SecurityScan]
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/develop'))
    jobs:
      - deployment: DeployDev
        displayName: 'Deploy to Dev Environment'
        pool:
          vmImage: 'ubuntu-latest'
        environment: 'deepwiki-dev'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureResourceManagerTemplateDeployment@3
                  displayName: 'Deploy Azure infrastructure'
                  inputs:
                    deploymentScope: 'Resource Group'
                    azureResourceManagerConnection: '$(azureServiceConnectionDev)'
                    subscriptionId: '$(devSubscriptionId)'
                    action: 'Create Or Update Resource Group'
                    resourceGroupName: 'rg-deepwiki-dev'
                    location: 'East US'
                    templateLocation: 'Linked artifact'
                    csmFile: '$(Pipeline.Workspace)/drop/infrastructure/main.bicep'
                    overrideParameters: |
                      -environment dev
                      -location "East US"

                - task: AzureWebApp@1
                  displayName: 'Deploy web application'
                  inputs:
                    azureSubscription: '$(azureServiceConnectionDev)'
                    appType: 'webApp'
                    appName: 'app-deepwiki-api-dev'
                    package: '$(Pipeline.Workspace)/drop/**/*.zip'

  - stage: IntegrationTests
    displayName: 'Integration Testing'
    dependsOn: DeployDev
    jobs:
      - job: IntegrationTests
        displayName: 'Run Integration Tests'
        pool:
          vmImage: 'ubuntu-latest'
        steps:
          - task: DotNetCoreCLI@2
            displayName: 'Run integration tests'
            inputs:
              command: 'test'
              projects: '**/*IntegrationTests/*.csproj'
              arguments: '--configuration $(buildConfiguration)'
            env:
              TEST_ENVIRONMENT_URL: 'https://app-deepwiki-api-dev.azurewebsites.net'

  - stage: DeployProduction
    displayName: 'Deploy to Production'
    dependsOn: IntegrationTests
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - deployment: DeployProduction
        displayName: 'Deploy to Production Environment'
        pool:
          vmImage: 'ubuntu-latest'
        environment: 'deepwiki-production'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureResourceManagerTemplateDeployment@3
                  displayName: 'Deploy Azure infrastructure'
                  inputs:
                    deploymentScope: 'Resource Group'
                    azureResourceManagerConnection: '$(azureServiceConnectionProd)'
                    subscriptionId: '$(prodSubscriptionId)'
                    action: 'Create Or Update Resource Group'
                    resourceGroupName: 'rg-deepwiki-prod'
                    location: 'East US'
                    templateLocation: 'Linked artifact'
                    csmFile: '$(Pipeline.Workspace)/drop/infrastructure/main.bicep'
                    overrideParameters: |
                      -environment prod
                      -location "East US"

                - task: AzureWebApp@1
                  displayName: 'Deploy web application'
                  inputs:
                    azureSubscription: '$(azureServiceConnectionProd)'
                    appType: 'webApp'
                    appName: 'app-deepwiki-api-prod'
                    package: '$(Pipeline.Workspace)/drop/**/*.zip'
                    deploymentMethod: 'zipDeploy'
                    takeAppOfflineFlag: false
```

### Configuration Management

#### Application Configuration Structure
```csharp
public class ApplicationConfiguration
{
    public AzureConfiguration Azure { get; set; }
    public Neo4jConfiguration Neo4j { get; set; }
    public GitHubConfiguration GitHub { get; set; }
    public MonitoringConfiguration Monitoring { get; set; }
}

public class AzureConfiguration
{
    public string ServiceBusConnectionString { get; set; }
    public string StorageConnectionString { get; set; }
    public string KeyVaultUrl { get; set; }
    public string ApplicationInsightsConnectionString { get; set; }
}

public class Neo4jConfiguration
{
    public string ConnectionString { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Database { get; set; }
}

public class MonitoringConfiguration
{
    public bool EnableDetailedLogging { get; set; }
    public int MetricsIntervalSeconds { get; set; }
    public List<string> AlertRecipients { get; set; }
}
```

### Security Configuration

#### Key Vault Secrets Management
```csharp
public interface ISecretManager
{
    Task<string> GetSecretAsync(string secretName);
    Task SetSecretAsync(string secretName, string secretValue);
    Task<IDictionary<string, string>> GetSecretsAsync(params string[] secretNames);
}

public class KeyVaultSecretManager : ISecretManager
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultSecretManager> _logger;
    
    public KeyVaultSecretManager(SecretClient secretClient, ILogger<KeyVaultSecretManager> logger)
    {
        _secretClient = secretClient;
        _logger = logger;
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Secret {SecretName} not found", secretName);
            return null;
        }
    }
}
```

### Monitoring and Observability

#### Application Insights Integration
```csharp
public class TelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    
    public TelemetryService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }
    
    public void TrackRepositoryConnection(string repositoryUrl, bool success, TimeSpan duration)
    {
        _telemetryClient.TrackEvent("RepositoryConnection", 
            new Dictionary<string, string>
            {
                ["RepositoryUrl"] = repositoryUrl,
                ["Success"] = success.ToString()
            },
            new Dictionary<string, double>
            {
                ["DurationMs"] = duration.TotalMilliseconds
            });
    }
    
    public void TrackCustomMetric(string metricName, double value, IDictionary<string, string> properties = null)
    {
        _telemetryClient.TrackMetric(metricName, value, properties);
    }
}
```

### Performance Requirements
- Infrastructure deployment completion < 10 minutes
- Application deployment < 5 minutes
- Zero-downtime deployment for production
- Pipeline execution time < 30 minutes end-to-end
- Service startup time < 2 minutes after deployment

## Implementation Guidance

### Recommended Development Approach
1. **Infrastructure Foundation**: Start with Bicep templates and basic Azure resource provisioning
2. **Pipeline Structure**: Build the CI/CD pipeline incrementally, starting with build and test
3. **Configuration Management**: Implement secure configuration handling with Key Vault
4. **Monitoring Setup**: Establish comprehensive logging and monitoring early
5. **Security Integration**: Add security scanning and validation throughout the pipeline

### Key Architectural Decisions
- Use Bicep over ARM templates for better readability and maintainability
- Implement blue-green deployment pattern for zero-downtime deployments
- Separate development and production environments completely
- Use managed identities for secure Azure resource access
- Implement comprehensive logging and monitoring from the start

### Technical Risks and Mitigation
1. **Risk**: Azure resource provisioning failures
   - **Mitigation**: Implement retry logic and idempotent templates
   - **Fallback**: Manual resource creation procedures documented

2. **Risk**: Pipeline deployment failures
   - **Mitigation**: Comprehensive testing in development environment
   - **Fallback**: Rollback procedures and manual deployment scripts

3. **Risk**: Security vulnerabilities in dependencies
   - **Mitigation**: Automated security scanning and regular updates
   - **Fallback**: Manual security review process

### Deployment Considerations
- Use separate Azure subscriptions for development and production
- Implement resource naming conventions for consistency
- Configure appropriate backup and disaster recovery procedures
- Set up cost monitoring and alerting for Azure resources

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)
- **Configuration Management**
  - Configuration loading and validation
  - Secret retrieval and caching
  - Error handling for missing configuration
  - Environment-specific configuration loading

- **Telemetry Services**
  - Custom metric tracking
  - Event logging functionality
  - Error handling and fallback scenarios
  - Performance counter integration

- **Infrastructure Components**
  - Service registration and dependency injection
  - Health check implementations
  - Startup and shutdown procedures
  - Cross-cutting concern implementations

### Integration Testing Requirements (30% coverage minimum)
- **Azure Services Integration**
  - Key Vault secret retrieval
  - Service Bus message handling
  - Application Insights telemetry
  - Storage account connectivity

- **End-to-End Pipeline Testing**
  - Complete deployment workflow
  - Environment promotion validation
  - Rollback procedure testing
  - Performance benchmarking

- **Infrastructure Validation**
  - Resource provisioning verification
  - Configuration deployment validation
  - Security policy enforcement
  - Monitoring alert functionality

### Performance Testing
- Infrastructure deployment time benchmarking
- Application startup performance validation
- Resource utilization under load
- Cost optimization analysis

## Quality Assurance

### Code Review Checkpoints
- [ ] Bicep templates follow Azure best practices
- [ ] Pipeline stages have appropriate gates and approvals
- [ ] Configuration management is secure and comprehensive
- [ ] Monitoring and alerting cover all critical scenarios
- [ ] Security scanning is integrated and enforced
- [ ] Documentation is complete and accurate
- [ ] Resource naming follows established conventions
- [ ] Cost optimization measures are implemented

### Definition of Done Checklist
- [ ] All Azure resources deploy successfully
- [ ] CI/CD pipeline executes without errors
- [ ] Security scanning passes all checks
- [ ] Integration tests pass in all environments
- [ ] Monitoring and alerting are operational
- [ ] Configuration management works across environments
- [ ] Performance meets specified requirements
- [ ] Security review completed and approved
- [ ] Infrastructure documentation updated
- [ ] Runbook procedures documented

### Monitoring and Observability
- **Custom Metrics**
  - Pipeline success/failure rates
  - Deployment duration tracking
  - Resource provisioning time
  - Security scan results

- **Alerts**
  - Pipeline failure notifications
  - Resource deployment failures
  - Security vulnerability detection
  - Cost threshold exceeding

- **Dashboards**
  - DevOps pipeline health overview
  - Azure resource utilization
  - Security compliance status
  - Performance metrics tracking

### Documentation Requirements
- Infrastructure as Code documentation
- Deployment runbook and procedures
- Security configuration guide
- Monitoring and alerting setup
- Troubleshooting guide for common issues