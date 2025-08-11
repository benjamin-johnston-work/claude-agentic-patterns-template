import { gql } from '@apollo/client';

// Repository Mutations
export const ADD_REPOSITORY = gql`
  mutation AddRepository($input: AddRepositoryInput!) {
    addRepository(input: $input) {
      id
      name
      url
      language
      description
      status
      createdAt
    }
  }
`;

export const REMOVE_REPOSITORY = gql`
  mutation RemoveRepository($repositoryId: ID!) {
    removeRepository(repositoryId: $repositoryId) {
      success
      message
    }
  }
`;

export const REFRESH_REPOSITORY = gql`
  mutation RefreshRepository($repositoryId: ID!) {
    refreshRepository(repositoryId: $repositoryId) {
      id
      status
      updatedAt
    }
  }
`;

export const VALIDATE_REPOSITORY = gql`
  mutation ValidateRepository($input: ValidateRepositoryInput!) {
    validateRepository(input: $input) {
      isValid
      repository {
        name
        description
        language
        isPrivate
        branches
      }
    }
  }
`;

// Documentation Mutations
export const GENERATE_DOCUMENTATION = gql`
  mutation GenerateDocumentation($repositoryId: ID!, $options: DocumentationGenerationOptions) {
    generateDocumentation(repositoryId: $repositoryId, options: $options) {
      id
      status
      sections {
        id
        title
        type
        status
      }
      generatedAt
    }
  }
`;

export const UPDATE_DOCUMENTATION_SECTION = gql`
  mutation UpdateDocumentationSection($sectionId: ID!, $content: String!) {
    updateDocumentationSection(sectionId: $sectionId, content: $content) {
      id
      title
      content
      updatedAt
    }
  }
`;

export const DELETE_DOCUMENTATION = gql`
  mutation DeleteDocumentation($documentationId: ID!) {
    deleteDocumentation(documentationId: $documentationId) {
      success
      message
    }
  }
`;

// Search Mutations
export const INDEX_REPOSITORY = gql`
  mutation IndexRepository($repositoryId: ID!, $forceReindex: Boolean = false) {
    indexRepository(repositoryId: $repositoryId, forceReindex: $forceReindex) {
      success
      message
      indexStatus {
        status
        progress
        totalFiles
        processedFiles
        lastUpdated
      }
    }
  }
`;

// Conversation Mutations
export const START_CONVERSATION = gql`
  mutation StartConversation($input: StartConversationInput!) {
    startConversation(input: $input) {
      id
      title
      repositoryId
      repositoryName
      createdAt
    }
  }
`;

export const SEND_MESSAGE = gql`
  mutation SendMessage($input: SendMessageInput!) {
    sendMessage(input: $input) {
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
    }
  }
`;

export const PROCESS_QUERY = gql`
  mutation ProcessQuery($input: ProcessQueryInput!) {
    processQuery(input: $input) {
      response
      confidence
      sources {
        type
        url
        title
        excerpt
      }
      suggestions
      conversationId
    }
  }
`;

export const ARCHIVE_CONVERSATION = gql`
  mutation ArchiveConversation($conversationId: ID!) {
    archiveConversation(conversationId: $conversationId) {
      success
      message
    }
  }
`;

export const DELETE_CONVERSATION = gql`
  mutation DeleteConversation($conversationId: ID!) {
    deleteConversation(conversationId: $conversationId) {
      success
      message
    }
  }
`;

// Knowledge Graph Mutations
export const BUILD_KNOWLEDGE_GRAPH = gql`
  mutation BuildKnowledgeGraph($input: BuildKnowledgeGraphInput!) {
    buildKnowledgeGraph(input: $input) {
      id
      status
      statistics {
        totalEntities
        totalRelationships
        averageComplexity
      }
      createdAt
    }
  }
`;

export const UPDATE_KNOWLEDGE_GRAPH = gql`
  mutation UpdateKnowledgeGraph($graphId: ID!, $repositoryIds: [ID!]!) {
    updateKnowledgeGraph(graphId: $graphId, repositoryIds: $repositoryIds) {
      id
      status
      statistics {
        totalEntities
        totalRelationships
        averageComplexity
      }
      updatedAt
    }
  }
`;

export const ANALYZE_ARCHITECTURE = gql`
  mutation AnalyzeArchitecture($repositoryId: ID!) {
    analyzeArchitecture(repositoryId: $repositoryId) {
      patterns {
        id
        type
        name
        confidence
        hasViolations
        hasCriticalViolations
      }
      complexityScore
      architecturalHealth
      analysisDate
    }
  }
`;

export const ANALYZE_ENTITY_RELATIONSHIPS = gql`
  mutation AnalyzeEntityRelationships($repositoryId: ID!, $entityIds: [ID!]) {
    analyzeEntityRelationships(repositoryId: $repositoryId, entityIds: $entityIds) {
      relationships {
        id
        sourceEntityId
        targetEntityId
        type
        metadata {
          strength
          description
        }
      }
      paths {
        entities
        relationships
        totalWeight
      }
    }
  }
`;

export const DETECT_ARCHITECTURAL_PATTERNS = gql`
  mutation DetectArchitecturalPatterns($repositoryId: ID!, $options: PatternDetectionOptions) {
    detectArchitecturalPatterns(repositoryId: $repositoryId, options: $options) {
      patterns {
        id
        type
        name
        description
        confidence
        hasViolations
        hasCriticalViolations
        entities {
          id
          name
          type
        }
      }
      antiPatterns {
        type
        description
        severity
        location {
          filePath
          lineNumber
        }
      }
      recommendations
    }
  }
`;

export const FIND_RELATIONSHIP_PATH = gql`
  mutation FindRelationshipPath($sourceEntityId: ID!, $targetEntityId: ID!, $maxDepth: Int = 5) {
    findRelationshipPath(sourceEntityId: $sourceEntityId, targetEntityId: $targetEntityId, maxDepth: $maxDepth) {
      paths {
        entities {
          id
          name
          type
        }
        relationships {
          id
          type
          metadata {
            strength
            description
          }
        }
        totalWeight
        depth
      }
      shortestPath {
        entities {
          id
          name
          type
        }
        relationships {
          id
          type
        }
        totalWeight
        depth
      }
    }
  }
`;