import { ApolloClient, InMemoryCache, createHttpLink, split, from } from '@apollo/client';
import { GraphQLWsLink } from '@apollo/client/link/subscriptions';
import { getMainDefinition } from '@apollo/client/utilities';
import { setContext } from '@apollo/client/link/context';
import { onError } from '@apollo/client/link/error';
import { createClient } from 'graphql-ws';
import { getSession } from 'next-auth/react';

// HTTP Link for queries and mutations
const httpLink = createHttpLink({
  uri: process.env.NEXT_PUBLIC_GRAPHQL_ENDPOINT || 'https://localhost:7001/graphql',
  credentials: 'include',
});

// WebSocket Link for subscriptions
const wsLink = typeof window !== 'undefined' ? new GraphQLWsLink(
  createClient({
    url: process.env.NEXT_PUBLIC_GRAPHQL_WS_ENDPOINT || 'wss://localhost:7001/graphql',
    connectionParams: async () => {
      const session = await getSession();
      return {
        authToken: session?.accessToken || '',
      };
    },
  })
) : null;

// Authentication Link
const authLink = setContext(async (_, { headers }) => {
  const session = await getSession();
  
  return {
    headers: {
      ...headers,
      authorization: session?.accessToken ? `Bearer ${session.accessToken}` : '',
      'Content-Type': 'application/json',
    },
  };
});

// Error Link
const errorLink = onError(({ graphQLErrors, networkError, operation, forward }) => {
  if (graphQLErrors) {
    graphQLErrors.forEach(({ message, locations, path }) => {
      console.error(
        `[GraphQL error]: Message: ${message}, Location: ${locations}, Path: ${path}`
      );
    });
  }

  if (networkError) {
    console.error(`[Network error]: ${networkError}`);
    
    // Handle authentication errors
    if ('statusCode' in networkError && networkError.statusCode === 401) {
      // Clear any client-side authentication state
      if (typeof window !== 'undefined') {
        window.location.href = '/auth/signin';
      }
    }
  }
});

// Split link between HTTP and WebSocket
const splitLink = typeof window !== 'undefined' && wsLink 
  ? split(
      ({ query }) => {
        const definition = getMainDefinition(query);
        return (
          definition.kind === 'OperationDefinition' &&
          definition.operation === 'subscription'
        );
      },
      wsLink,
      from([errorLink, authLink, httpLink])
    )
  : from([errorLink, authLink, httpLink]);

// Create Apollo Client
export const apolloClient = new ApolloClient({
  link: splitLink,
  cache: new InMemoryCache({
    typePolicies: {
      Repository: {
        fields: {
          files: {
            merge(existing = [], incoming) {
              return [...existing, ...incoming];
            },
          },
          documentation: {
            merge(existing, incoming) {
              return incoming || existing;
            },
          },
          knowledgeGraph: {
            merge(existing, incoming) {
              return incoming || existing;
            },
          },
        },
      },
      SearchResults: {
        fields: {
          results: {
            merge(existing = [], incoming) {
              return incoming; // Always replace with new results
            },
          },
        },
      },
      Conversation: {
        fields: {
          messages: {
            merge(existing = [], incoming) {
              const existingIds = existing.map((msg: any) => msg.__ref || msg.id);
              const newMessages = incoming.filter((msg: any) => {
                const msgId = msg.__ref || msg.id;
                return !existingIds.includes(msgId);
              });
              return [...existing, ...newMessages];
            },
          },
        },
      },
      Query: {
        fields: {
          repositories: {
            merge(existing = [], incoming) {
              return incoming;
            },
          },
          searchRepositories: {
            merge(existing, incoming) {
              return incoming;
            },
          },
        },
      },
    },
  }),
  defaultOptions: {
    watchQuery: {
      errorPolicy: 'all',
      notifyOnNetworkStatusChange: true,
    },
    query: {
      errorPolicy: 'all',
      notifyOnNetworkStatusChange: true,
    },
    mutate: {
      errorPolicy: 'all',
    },
  },
  connectToDevTools: process.env.NODE_ENV === 'development',
});

// Helper function to get authentication token
async function getAuthToken(): Promise<string | null> {
  try {
    const session = await getSession();
    return session?.accessToken || null;
  } catch (error) {
    console.error('Error getting auth token:', error);
    return null;
  }
}