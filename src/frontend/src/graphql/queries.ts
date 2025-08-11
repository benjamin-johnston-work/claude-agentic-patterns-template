import { gql } from '@apollo/client';

// Repository Queries
export const GET_REPOSITORIES = gql`
  query GetRepositories($filter: RepositoryFilterInput) {
    repositories(filter: $filter) {
      id
      name
      url
      language
      description
      status
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
      hasDocumentation
      documentationStatus
      documentationLastGenerated
      hasKnowledgeGraph
      complexityScore
      architecturalHealth
      createdAt
      updatedAt
    }
  }
`;

export const GET_REPOSITORY = gql`
  query GetRepository($id: ID!) {
    repository(id: $id) {
      id
      name
      url
      language
      description
      status
      branches {
        name
        isDefault
        lastCommit {
          hash
          message
          author
          timestamp
        }
        createdAt
      }
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
      hasDocumentation
      documentationStatus
      documentationLastGenerated
      hasKnowledgeGraph
      complexityScore
      architecturalHealth
      createdAt
      updatedAt
    }
  }
`;

export const GET_REPOSITORY_STRUCTURE = gql`
  query GetRepositoryStructure($repositoryId: ID!) {
    repository(id: $repositoryId) {
      id
      name
      fileStructure {
        folders {
          name
          path
          files {
            name
            path
            size
            language
            lastModified
          }
          subfolders {
            name
            path
          }
        }
        files {
          name
          path
          size
          language
          lastModified
        }
      }
    }
  }
`;

export const GET_FILE_CONTENT = gql`
  query GetFileContent($repositoryId: ID!, $filePath: String!) {
    fileContent(repositoryId: $repositoryId, filePath: $filePath) {
      content
      language
      size
      lastModified
      path
    }
  }
`;

// Documentation Queries
export const GET_REPOSITORY_DOCUMENTATION = gql`
  query GetRepositoryDocumentation($repositoryId: ID!, $documentationId: ID) {
    repository(id: $repositoryId) {
      id
      name
      documentation(id: $documentationId) {
        id
        title
        status
        sections {
          id
          title
          content
          type
          order
          metadata {
            wordCount
            readingTime
            lastModified
          }
        }
        metadata {
          totalSections
          totalWords
          estimatedReadingTime
          lastGenerated
          generationDuration
        }
        statistics {
          sectionsGenerated
          wordsGenerated
          averageSectionLength
        }
        generatedAt
        createdAt
        updatedAt
      }
    }
  }
`;

export const SEARCH_DOCUMENTATION = gql`
  query SearchDocumentation($query: String!, $repositoryId: ID, $filters: [DocumentationSearchFilter!]) {
    searchDocumentation(query: $query, repositoryId: $repositoryId, filters: $filters) {
      results {
        id
        title
        excerpt
        sectionTitle
        repositoryId
        repositoryName
        score
        highlights
      }
      totalCount
      searchDuration
    }
  }
`;

// Search Queries
export const SEARCH_REPOSITORIES = gql`
  query SearchRepositories($input: SearchRepositoriesInput!) {
    searchRepositories(input: $input) {
      results {
        id
        title
        content
        excerpt
        repositoryId
        repositoryName
        filePath
        language
        score
        highlights
        lastModified
      }
      totalCount
      searchDuration
      facets {
        field
        values {
          value
          count
        }
      }
    }
  }
`;

export const GET_SEARCH_SUGGESTIONS = gql`
  query GetSearchSuggestions($query: String!, $count: Int = 5) {
    searchSuggestions(query: $query, count: $count) {
      suggestions
      popularQueries
    }
  }
`;

export const GET_DOCUMENT = gql`
  query GetDocument($documentId: String!) {
    document(documentId: $documentId) {
      id
      title
      content
      repositoryId
      repositoryName
      filePath
      language
      lastModified
    }
  }
`;

export const GET_INDEX_STATUS = gql`
  query GetIndexStatus($repositoryId: ID!) {
    indexStatus(repositoryId: $repositoryId) {
      status
      progress
      totalFiles
      processedFiles
      errorMessage
      lastUpdated
      estimatedTimeRemaining
    }
  }
`;

// Conversation Queries
export const GET_CONVERSATIONS = gql`
  query GetConversations($repositoryId: ID, $limit: Int = 20, $offset: Int = 0) {
    conversations(repositoryId: $repositoryId, limit: $limit, offset: $offset) {
      id
      title
      repositoryId
      repositoryName
      messageCount
      lastMessageAt
      createdAt
      metadata {
        tags
        summary
        participants
      }
    }
  }
`;

export const GET_CONVERSATION = gql`
  query GetConversation($conversationId: ID!) {
    conversation(id: $conversationId) {
      id
      title
      repositoryId
      repositoryName
      messages {
        id
        content
        sender
        timestamp
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
      metadata {
        tags
        summary
        participants
      }
      createdAt
      updatedAt
    }
  }
`;

// Knowledge Graph Queries
export const GET_KNOWLEDGE_GRAPH = gql`
  query GetKnowledgeGraph($repositoryIds: [ID!]!) {
    knowledgeGraph(repositoryIds: $repositoryIds) {
      id
      entities {
        id
        name
        type
        location {
          filePath
          lineNumber
          columnNumber
        }
        metadata {
          description
          complexity
          dependencies
          attributes {
            name
            type
            accessModifier
          }
        }
        complexityScore
      }
      relationships {
        id
        sourceEntityId
        targetEntityId
        type
        metadata {
          strength
          description
          sourceLocation {
            filePath
            lineNumber
            columnNumber
          }
        }
      }
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
      metadata {
        analysisDate
        version
        repositoryIds
      }
      status
      createdAt
      updatedAt
    }
  }
`;

export const GET_CODE_ENTITIES = gql`
  query GetCodeEntities($repositoryId: ID!, $type: EntityType, $limit: Int = 100) {
    repository(id: $repositoryId) {
      id
      codeEntities(type: $type, limit: $limit) {
        id
        name
        type
        location {
          filePath
          lineNumber
          columnNumber
        }
        metadata {
          description
          complexity
          dependencies
          attributes {
            name
            type
            accessModifier
          }
        }
        complexityScore
      }
    }
  }
`;

export const GET_ARCHITECTURAL_PATTERNS = gql`
  query GetArchitecturalPatterns($repositoryId: ID!) {
    repository(id: $repositoryId) {
      id
      architecturalPatterns {
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
          antiPatterns {
            type
            description
            severity
            location {
              filePath
              lineNumber
            }
          }
          violations {
            type
            description
            severity
            location {
              filePath
              lineNumber
            }
          }
        }
        entities {
          id
          name
          type
        }
      }
    }
  }
`;