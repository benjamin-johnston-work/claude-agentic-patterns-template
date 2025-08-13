import { test, expect } from '@playwright/test';

test.describe('GraphQL API Integration Tests', () => {
  const GRAPHQL_ENDPOINT = 'http://localhost:5145/graphql';
  
  test('GraphQL Schema Introspection - Verify Schema Structure', async ({ request }) => {
    const introspectionQuery = {
      query: `
        query IntrospectionQuery {
          __schema {
            queryType { name }
            mutationType { name }
            subscriptionType { name }
            types {
              name
              kind
              description
            }
          }
        }
      `
    };

    const response = await request.post(GRAPHQL_ENDPOINT, {
      data: introspectionQuery
    });

    expect(response.status()).toBe(200);
    
    const data = await response.json();
    expect(data.errors).toBeUndefined();
    
    const schema = data.data.__schema;
    expect(schema.queryType.name).toBe('Query');
    expect(schema.mutationType.name).toBe('Mutation');
    
    // Verify key types exist
    const typeNames = schema.types.map((t: any) => t.name);
    expect(typeNames).toContain('Repository');
    expect(typeNames).toContain('Documentation');
    expect(typeNames).toContain('SearchResult');
    expect(typeNames).toContain('Conversation');
  });

  test('F01: Repository Queries - CRUD Operations', async ({ request }) => {
    let repositoryId: string;

    // Create repository
    const addRepositoryMutation = {
      query: `
        mutation AddRepository($input: AddRepositoryInput!) {
          addRepository(input: $input) {
            id
            name
            url
            owner
            language
            status
            createdAt
          }
        }
      `,
      variables: {
        input: {
          name: 'graphql-test-repo',
          url: 'https://github.com/facebook/react',
          owner: 'facebook',
          language: 'JavaScript',
          description: 'GraphQL API test repository'
        }
      }
    };

    const createResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: addRepositoryMutation
    });

    expect(createResponse.status()).toBe(200);
    
    const createData = await createResponse.json();
    expect(createData.errors).toBeUndefined();
    
    const repository = createData.data.addRepository;
    repositoryId = repository.id;
    
    expect(repository.name).toBe('graphql-test-repo');
    expect(repository.status).toBe('Connected');

    // Read repository
    const getRepositoryQuery = {
      query: `
        query GetRepository($id: ID!) {
          repository(id: $id) {
            id
            name
            url
            owner
            language
            description
            status
            documentCount
            indexedAt
          }
        }
      `,
      variables: { id: repositoryId }
    };

    const readResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: getRepositoryQuery
    });

    expect(readResponse.status()).toBe(200);
    
    const readData = await readResponse.json();
    expect(readData.errors).toBeUndefined();
    expect(readData.data.repository.id).toBe(repositoryId);
    expect(readData.data.repository.name).toBe('graphql-test-repo');

    // Update repository (if mutation exists)
    // Delete repository
    const deleteRepositoryMutation = {
      query: `
        mutation DeleteRepository($id: ID!) {
          deleteRepository(id: $id) {
            success
            message
          }
        }
      `,
      variables: { id: repositoryId }
    };

    const deleteResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: deleteRepositoryMutation
    });

    expect(deleteResponse.status()).toBe(200);
    
    const deleteData = await deleteResponse.json();
    expect(deleteData.errors).toBeUndefined();
    expect(deleteData.data.deleteRepository.success).toBe(true);
  });

  test('F01: Repository Validation - Invalid Input Handling', async ({ request }) => {
    const invalidRepositoryMutation = {
      query: `
        mutation AddRepository($input: AddRepositoryInput!) {
          addRepository(input: $input) {
            id
            name
          }
        }
      `,
      variables: {
        input: {
          name: '', // Invalid: empty name
          url: 'not-a-valid-url', // Invalid: malformed URL
          owner: '',
          language: 'UnknownLanguage'
        }
      }
    };

    const response = await request.post(GRAPHQL_ENDPOINT, {
      data: invalidRepositoryMutation
    });

    expect(response.status()).toBe(200); // GraphQL returns 200 even for validation errors
    
    const data = await response.json();
    expect(data.errors).toBeDefined();
    expect(data.errors.length).toBeGreaterThan(0);
    
    const errorMessages = data.errors.map((e: any) => e.message).join(' ');
    expect(errorMessages.toLowerCase()).toContain('validation');
  });

  test('F02: Search Queries - Vector Search Operations', async ({ request }) => {
    const searchQuery = {
      query: `
        query SearchDocuments($query: String!, $repositoryIds: [ID!]) {
          searchDocuments(query: $query, repositoryIds: $repositoryIds) {
            totalCount
            results {
              id
              title
              content
              filePath
              repositoryId
              score
              highlights {
                field
                fragments
              }
            }
          }
        }
      `,
      variables: {
        query: 'authentication middleware',
        repositoryIds: [] // Search all repositories
      }
    };

    const response = await request.post(GRAPHQL_ENDPOINT, {
      data: searchQuery
    });

    expect(response.status()).toBe(200);
    
    const data = await response.json();
    expect(data.errors).toBeUndefined();
    
    const searchResults = data.data.searchDocuments;
    expect(searchResults.totalCount).toBeGreaterThanOrEqual(0);
    
    if (searchResults.results.length > 0) {
      const firstResult = searchResults.results[0];
      expect(firstResult.id).toBeTruthy();
      expect(firstResult.score).toBeGreaterThan(0);
      expect(firstResult.repositoryId).toBeTruthy();
    }
  });

  test('F02: Indexing Operations - Start and Monitor', async ({ request }) => {
    // First create a test repository
    const addRepositoryMutation = {
      query: `
        mutation AddRepository($input: AddRepositoryInput!) {
          addRepository(input: $input) {
            id
          }
        }
      `,
      variables: {
        input: {
          name: 'indexing-test-repo',
          url: 'https://github.com/microsoft/vscode',
          owner: 'microsoft',
          language: 'TypeScript'
        }
      }
    };

    const createResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: addRepositoryMutation
    });

    const createData = await createResponse.json();
    const repositoryId = createData.data.addRepository.id;

    // Start indexing
    const startIndexingMutation = {
      query: `
        mutation StartIndexing($repositoryId: ID!) {
          startRepositoryIndexing(repositoryId: $repositoryId) {
            success
            message
            indexingId
          }
        }
      `,
      variables: { repositoryId }
    };

    const indexingResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: startIndexingMutation
    });

    expect(indexingResponse.status()).toBe(200);
    
    const indexingData = await indexingResponse.json();
    expect(indexingData.errors).toBeUndefined();
    expect(indexingData.data.startRepositoryIndexing.success).toBe(true);

    // Monitor indexing status
    const statusQuery = {
      query: `
        query GetIndexingStatus($repositoryId: ID!) {
          repository(id: $repositoryId) {
            id
            status
            documentCount
            indexedAt
            lastIndexingError
          }
        }
      `,
      variables: { repositoryId }
    };

    const statusResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: statusQuery
    });

    const statusData = await statusResponse.json();
    expect(statusData.errors).toBeUndefined();
    expect(statusData.data.repository.status).toMatch(/Indexing|Indexed|Connected/);

    // Cleanup
    const deleteRepositoryMutation = {
      query: `
        mutation DeleteRepository($id: ID!) {
          deleteRepository(id: $id) { success }
        }
      `,
      variables: { id: repositoryId }
    };

    await request.post(GRAPHQL_ENDPOINT, { data: deleteRepositoryMutation });
  });

  test('F03: Documentation Generation - Complete Flow', async ({ request }) => {
    // Create and index repository (simplified for testing)
    const repositoryId = await createAndIndexTestRepository(request);

    // Generate documentation
    const generateDocumentationMutation = {
      query: `
        mutation GenerateDocumentation($repositoryId: ID!) {
          generateDocumentation(repositoryId: $repositoryId) {
            id
            repositoryId
            title
            status
            sections {
              id
              title
              content
              type
              order
            }
            totalSections
            estimatedReadingTime
            generationDuration
            sectionsGenerated
            lastGenerated
          }
        }
      `,
      variables: { repositoryId }
    };

    const generateResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: generateDocumentationMutation
    });

    expect(generateResponse.status()).toBe(200);
    
    const generateData = await generateResponse.json();
    expect(generateData.errors).toBeUndefined();
    
    const documentation = generateData.data.generateDocumentation;
    expect(documentation.repositoryId).toBe(repositoryId);
    expect(documentation.status).toBe('Generated');
    expect(documentation.sections.length).toBeGreaterThan(0);
    
    // Verify frontend UX fields
    expect(documentation.totalSections).toBeGreaterThan(0);
    expect(documentation.estimatedReadingTime).toBeGreaterThan(0);
    expect(documentation.sectionsGenerated).toBeGreaterThan(0);
    expect(documentation.lastGenerated).toBeTruthy();

    // Get documentation
    const getDocumentationQuery = {
      query: `
        query GetDocumentation($repositoryId: ID!) {
          repository(id: $repositoryId) {
            documentation {
              id
              title
              status
              sections {
                id
                title
                content
                type
                order
              }
              totalSections
              estimatedReadingTime
            }
          }
        }
      `,
      variables: { repositoryId }
    };

    const getResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: getDocumentationQuery
    });

    const getData = await getResponse.json();
    expect(getData.errors).toBeUndefined();
    expect(getData.data.repository.documentation.status).toBe('Generated');

    // Cleanup
    await cleanupTestRepository(request, repositoryId);
  });

  test('F04: Conversational Queries - Chat Flow', async ({ request }) => {
    const conversationId = generateUUID();
    const repositoryId = await createAndIndexTestRepository(request);

    // Send message
    const sendMessageMutation = {
      query: `
        mutation SendMessage($input: SendMessageInput!) {
          sendMessage(input: $input) {
            id
            conversationId
            content
            role
            timestamp
            metadata {
              sources {
                title
                filePath
                repositoryId
              }
              confidence
              responseTime
            }
          }
        }
      `,
      variables: {
        input: {
          conversationId,
          message: 'What are the main components of this repository?',
          repositoryIds: [repositoryId]
        }
      }
    };

    const messageResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: sendMessageMutation
    });

    expect(messageResponse.status()).toBe(200);
    
    const messageData = await messageResponse.json();
    expect(messageData.errors).toBeUndefined();
    
    const message = messageData.data.sendMessage;
    expect(message.conversationId).toBe(conversationId);
    expect(message.role).toBe('Assistant');
    expect(message.content).toBeTruthy();
    expect(message.metadata.responseTime).toBeGreaterThan(0);

    // Get conversation
    const getConversationQuery = {
      query: `
        query GetConversation($conversationId: ID!) {
          conversation(id: $conversationId) {
            id
            title
            messages {
              id
              content
              role
              timestamp
            }
          }
        }
      `,
      variables: { conversationId }
    };

    const conversationResponse = await request.post(GRAPHQL_ENDPOINT, {
      data: getConversationQuery
    });

    const conversationData = await conversationResponse.json();
    expect(conversationData.errors).toBeUndefined();
    expect(conversationData.data.conversation.messages.length).toBeGreaterThanOrEqualTo(2);

    // Cleanup
    await cleanupTestRepository(request, repositoryId);
  });

  test('GraphQL Subscriptions - Real-time Updates', async ({ page }) => {
    test.setTimeout(30000);

    // This test would require WebSocket support for GraphQL subscriptions
    await page.goto('http://localhost:3000/chat');
    
    // Set up subscription listener (if WebSocket GraphQL subscriptions are implemented)
    const subscriptionQuery = `
      subscription IndexingProgress($repositoryId: ID!) {
        indexingProgress(repositoryId: $repositoryId) {
          repositoryId
          status
          documentCount
          progress
        }
      }
    `;

    // For now, just verify the subscription endpoint is available
    const response = await page.request.post(GRAPHQL_ENDPOINT, {
      data: { query: subscriptionQuery.replace('$repositoryId: ID!', '"test-id"') }
    });

    // Subscriptions might return a different response format
    expect(response.status()).toBe(200);
  });

  test('GraphQL Error Handling - Malformed Queries', async ({ request }) => {
    const malformedQuery = {
      query: `
        query {
          repository(id: "invalid-id") {
            nonExistentField
            anotherInvalidField {
              nestedInvalid
            }
          }
        }
      `
    };

    const response = await request.post(GRAPHQL_ENDPOINT, {
      data: malformedQuery
    });

    expect(response.status()).toBe(200); // GraphQL returns 200 even for errors
    
    const data = await response.json();
    expect(data.errors).toBeDefined();
    expect(data.errors.length).toBeGreaterThan(0);
  });

  test('GraphQL Performance - Query Complexity Limits', async ({ request }) => {
    // Test a deeply nested query to verify query complexity limits
    const complexQuery = {
      query: `
        query {
          repositories(first: 10) {
            nodes {
              id
              documentation {
                sections {
                  id
                  content
                }
              }
              searchResults: search(query: "test") {
                results {
                  highlights {
                    fragments
                  }
                }
              }
            }
          }
        }
      `
    };

    const startTime = Date.now();
    const response = await request.post(GRAPHQL_ENDPOINT, {
      data: complexQuery
    });
    const duration = Date.now() - startTime;

    expect(response.status()).toBe(200);
    expect(duration).toBeLessThan(10000); // Should complete within 10 seconds
    
    const data = await response.json();
    // Query should either succeed or be rejected with complexity error
    if (data.errors) {
      const errorMessage = data.errors[0].message.toLowerCase();
      expect(errorMessage).toMatch(/complexity|depth|limit/);
    } else {
      expect(data.data.repositories).toBeDefined();
    }
  });
});

// Helper functions
async function createAndIndexTestRepository(request: any): Promise<string> {
  const addRepositoryMutation = {
    query: `
      mutation AddRepository($input: AddRepositoryInput!) {
        addRepository(input: $input) {
          id
        }
      }
    `,
    variables: {
      input: {
        name: `test-repo-${Date.now()}`,
        url: 'https://github.com/microsoft/TypeScript',
        owner: 'microsoft',
        language: 'TypeScript',
        description: 'Test repository for API testing'
      }
    }
  };

  const response = await request.post('http://localhost:5145/graphql', {
    data: addRepositoryMutation
  });

  const data = await response.json();
  return data.data.addRepository.id;
}

async function cleanupTestRepository(request: any, repositoryId: string): Promise<void> {
  const deleteRepositoryMutation = {
    query: `
      mutation DeleteRepository($id: ID!) {
        deleteRepository(id: $id) {
          success
        }
      }
    `,
    variables: { id: repositoryId }
  };

  await request.post('http://localhost:5145/graphql', {
    data: deleteRepositoryMutation
  });
}

function generateUUID(): string {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    const r = Math.random() * 16 | 0;
    const v = c == 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}