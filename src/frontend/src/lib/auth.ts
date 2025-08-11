import type { NextAuthOptions } from 'next-auth';
import AzureADProvider from 'next-auth/providers/azure-ad';
import CredentialsProvider from 'next-auth/providers/credentials';
import type { JWT } from 'next-auth/jwt';

// Check if we have valid Azure AD credentials for production
const hasValidAzureADConfig = () => {
  const clientId = process.env.AZURE_AD_CLIENT_ID;
  const clientSecret = process.env.AZURE_AD_CLIENT_SECRET;
  
  return clientId && 
         clientSecret && 
         clientId !== 'your-azure-ad-client-id' && 
         clientSecret !== 'your-azure-ad-client-secret';
};

export const authOptions: NextAuthOptions = {
  providers: [
    // Production Azure AD provider (only if credentials are configured)
    ...(hasValidAzureADConfig() ? [
      AzureADProvider({
        clientId: process.env.AZURE_AD_CLIENT_ID!,
        clientSecret: process.env.AZURE_AD_CLIENT_SECRET!,
        tenantId: process.env.AZURE_AD_TENANT_ID || 'ceb29e10-60d1-4f6b-86b1-b7d497b5b66e',
        authorization: {
          params: {
            scope: 'openid profile email User.Read offline_access',
          },
        },
      })
    ] : []),
    
    // Development credentials provider (for local development)
    ...(process.env.NODE_ENV === 'development' ? [
      CredentialsProvider({
        id: 'development',
        name: 'Development Login',
        credentials: {
          email: { 
            label: 'Email', 
            type: 'email', 
            placeholder: 'developer@archie.dev' 
          },
          name: { 
            label: 'Name', 
            type: 'text', 
            placeholder: 'Developer User' 
          }
        },
        async authorize(credentials) {
          // In development mode, accept any email/name combination
          if (credentials?.email) {
            return {
              id: 'dev-user-' + Date.now(),
              email: credentials.email,
              name: credentials.name || 'Development User',
              image: null,
            };
          }
          return null;
        },
      })
    ] : []),
  ],
  callbacks: {
    async jwt({ token, account, profile, user }) {
      // Handle development provider
      if (account?.provider === 'development') {
        // For development, use user data directly
        if (user) {
          token.name = user.name;
          token.email = user.email;
          token.picture = user.image;
          token.sub = user.id;
        }
        // Set long expiry for development
        token.expiresAt = Math.floor(Date.now() / 1000) + (24 * 60 * 60); // 24 hours
        return token;
      }

      // Handle Azure AD provider
      if (account && account.provider === 'azure-ad') {
        token.accessToken = account.access_token;
        token.refreshToken = account.refresh_token;
        token.idToken = account.id_token;
        token.expiresAt = account.expires_at;
        token.tokenType = account.token_type;
      }

      // Add user profile information for Azure AD
      if (profile && account?.provider === 'azure-ad') {
        token.name = profile.name;
        token.email = profile.email;
        token.picture = (profile as any).picture;
        token.sub = profile.sub;
      }

      // Skip token refresh logic for development provider
      if (token.provider === 'development') {
        return token;
      }

      // Check if token is expired and try to refresh (Azure AD only)
      if (token.expiresAt && Date.now() < (token.expiresAt as number) * 1000) {
        return token;
      }

      // Token is expired, try to refresh it (Azure AD only)
      if (token.refreshToken) {
        try {
          const refreshedTokens = await refreshAccessToken(token);
          return refreshedTokens;
        } catch (error) {
          console.error('Error refreshing access token:', error);
          // Return token with error flag
          return {
            ...token,
            error: 'RefreshAccessTokenError',
          };
        }
      }

      return token;
    },
    async session({ session, token }) {
      // Send properties to the client
      session.accessToken = token.accessToken as string;
      session.idToken = token.idToken as string;
      session.error = token.error as string;
      
      if (token.error) {
        session.error = token.error;
      }

      // Add user information to session
      if (session.user) {
        session.user.id = token.sub;
        session.user.name = token.name;
        session.user.email = token.email;
        session.user.image = token.picture;
      }

      return session;
    },
    async redirect({ url, baseUrl }) {
      // Allows relative callback URLs
      if (url.startsWith('/')) return `${baseUrl}${url}`;
      // Allows callback URLs on the same origin
      if (new URL(url).origin === baseUrl) return url;
      return baseUrl;
    },
  },
  pages: {
    signIn: '/auth/signin',
    error: '/auth/error',
    signOut: '/auth/signout',
  },
  session: {
    strategy: 'jwt',
    maxAge: 24 * 60 * 60, // 24 hours
  },
  jwt: {
    maxAge: 24 * 60 * 60, // 24 hours
  },
  events: {
    async signOut(message) {
      // Log sign out event
      console.log('User signed out:', message);
    },
    async session(message) {
      // Log session events if needed
      if (process.env.NODE_ENV === 'development') {
        console.log('Session event:', message);
      }
    },
  },
  debug: process.env.NODE_ENV === 'development',
};

/**
 * Refresh the access token using the refresh token (Azure AD only)
 */
async function refreshAccessToken(token: JWT): Promise<JWT> {
  // Only refresh Azure AD tokens
  if (!hasValidAzureADConfig()) {
    return {
      ...token,
      error: 'RefreshAccessTokenError',
    };
  }

  try {
    const response = await fetch('https://login.microsoftonline.com/common/oauth2/v2.0/token', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: new URLSearchParams({
        grant_type: 'refresh_token',
        client_id: process.env.AZURE_AD_CLIENT_ID!,
        client_secret: process.env.AZURE_AD_CLIENT_SECRET!,
        refresh_token: token.refreshToken as string,
        scope: 'openid profile email User.Read offline_access',
      }),
    });

    const tokens = await response.json();

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${tokens.error_description || 'Failed to refresh token'}`);
    }

    return {
      ...token,
      accessToken: tokens.access_token,
      expiresAt: Math.floor(Date.now() / 1000 + tokens.expires_in),
      refreshToken: tokens.refresh_token ?? token.refreshToken, // Fall back to old refresh token
      idToken: tokens.id_token,
    };
  } catch (error) {
    console.error('Error refreshing access token:', error);
    
    return {
      ...token,
      error: 'RefreshAccessTokenError',
    };
  }
}

/**
 * Type augmentation for NextAuth
 */
declare module 'next-auth' {
  interface Session {
    accessToken?: string;
    idToken?: string;
    error?: string;
    user: {
      id?: string;
      name?: string | null;
      email?: string | null;
      image?: string | null;
    };
  }
}

declare module 'next-auth/jwt' {
  interface JWT {
    accessToken?: string;
    refreshToken?: string;
    idToken?: string;
    expiresAt?: number;
    tokenType?: string;
    error?: string;
  }
}