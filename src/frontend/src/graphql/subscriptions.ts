import { gql } from '@apollo/client';

// Chat Subscriptions
export const CHAT_MESSAGE_RECEIVED = gql`
  subscription ChatMessageReceived($conversationId: ID!) {
    messageReceived(conversationId: $conversationId) {
      id
      content
      sender
      timestamp
      conversationId
      metadata {
        type
        attachments {
          type
          url
          title
          description
        }
      }
      repositoryContext
    }
  }
`;

export const CONVERSATION_UPDATED = gql`
  subscription ConversationUpdated($conversationId: ID!) {
    conversationUpdated(conversationId: $conversationId) {
      id
      title
      messageCount
      lastMessageAt
      metadata {
        tags
        summary
        participants
      }
      updatedAt
    }
  }
`;

// Repository Analysis Subscriptions
export const REPOSITORY_ANALYSIS_PROGRESS = gql`
  subscription RepositoryAnalysisProgress($repositoryId: ID!) {
    repositoryAnalysisProgress(repositoryId: $repositoryId) {
      repositoryId
      stage
      progress
      totalSteps
      currentStep
      message
      estimatedTimeRemaining
      errors
      warnings
    }
  }
`;

export const REPOSITORY_STATUS_CHANGED = gql`
  subscription RepositoryStatusChanged($repositoryId: ID!) {
    repositoryStatusChanged(repositoryId: $repositoryId) {
      id
      status
      updatedAt
      statistics {
        fileCount
        lineCount
        languageBreakdown {
          language
          fileCount
          lineCount
          percentage
        }
      }
    }
  }
`;

// Documentation Generation Subscriptions
export const DOCUMENTATION_GENERATION_PROGRESS = gql`
  subscription DocumentationGenerationProgress($repositoryId: ID!) {
    documentationGenerationProgress(repositoryId: $repositoryId) {
      repositoryId
      documentationId
      status
      progress
      sectionsCompleted
      totalSections
      currentSection
      estimatedTimeRemaining
      errors
    }
  }
`;

export const DOCUMENTATION_SECTION_GENERATED = gql`
  subscription DocumentationSectionGenerated($documentationId: ID!) {
    documentationSectionGenerated(documentationId: $documentationId) {
      id
      title
      content
      type
      order
      status
      metadata {
        wordCount
        readingTime
        lastModified
      }
      generatedAt
    }
  }
`;

// Search Indexing Subscriptions
export const INDEXING_PROGRESS = gql`
  subscription IndexingProgress($repositoryId: ID!) {
    indexingProgress(repositoryId: $repositoryId) {
      repositoryId
      status
      progress
      totalFiles
      processedFiles
      currentFile
      errors
      warnings
      estimatedTimeRemaining
      lastUpdated
    }
  }
`;

// Knowledge Graph Construction Subscriptions
export const KNOWLEDGE_GRAPH_CONSTRUCTION_PROGRESS = gql`
  subscription KnowledgeGraphConstructionProgress($repositoryId: ID!) {
    knowledgeGraphConstructionProgress(repositoryId: $repositoryId) {
      repositoryId
      graphId
      stage
      progress
      entitiesProcessed
      totalEntities
      relationshipsFound
      currentFile
      estimatedTimeRemaining
      errors
      warnings
    }
  }
`;

export const KNOWLEDGE_GRAPH_UPDATED = gql`
  subscription KnowledgeGraphUpdated($graphId: ID!) {
    knowledgeGraphUpdated(graphId: $graphId) {
      id
      status
      statistics {
        totalEntities
        totalRelationships
        averageComplexity
        entityTypeBreakdown {
          type
          count
        }
        relationshipTypeBreakdown {
          type
          count
        }
      }
      updatedAt
    }
  }
`;

// Architectural Analysis Subscriptions
export const ARCHITECTURAL_ANALYSIS_PROGRESS = gql`
  subscription ArchitecturalAnalysisProgress($repositoryId: ID!) {
    architecturalAnalysisProgress(repositoryId: $repositoryId) {
      repositoryId
      stage
      progress
      patternsDetected
      violationsFound
      currentAnalysis
      estimatedTimeRemaining
      warnings
    }
  }
`;

export const PATTERN_DETECTED = gql`
  subscription PatternDetected($repositoryId: ID!) {
    patternDetected(repositoryId: $repositoryId) {
      id
      type
      name
      description
      confidence
      hasViolations
      hasCriticalViolations
      metadata {
        detectedAt
        pattern
      }
      entities {
        id
        name
        type
      }
    }
  }
`;

// System Health Subscriptions
export const SYSTEM_HEALTH_STATUS = gql`
  subscription SystemHealthStatus {
    systemHealthStatus {
      timestamp
      services {
        name
        status
        responseTime
        errorRate
        uptime
      }
      resources {
        cpuUsage
        memoryUsage
        diskUsage
        activeConnections
      }
      performance {
        averageQueryTime
        activeSubscriptions
        cacheHitRate
      }
    }
  }
`;

// User Activity Subscriptions
export const USER_ACTIVITY_STREAM = gql`
  subscription UserActivityStream($userId: ID!) {
    userActivityStream(userId: $userId) {
      id
      userId
      activityType
      resourceType
      resourceId
      description
      timestamp
      metadata {
        ipAddress
        userAgent
        location
      }
    }
  }
`;