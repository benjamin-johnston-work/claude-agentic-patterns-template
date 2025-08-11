'use client';

import { useState } from 'react';
import { signIn } from 'next-auth/react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Loader2, AlertCircle, Building2, User, Code } from 'lucide-react';

interface SignInFormProps {
  callbackUrl?: string;
  error?: string;
}

const errorMessages: Record<string, string> = {
  Signin: 'Try signing in with a different account.',
  OAuthSignin: 'Try signing in with a different account.',
  OAuthCallback: 'Try signing in with a different account.',
  OAuthCreateAccount: 'Try signing in with a different account.',
  EmailCreateAccount: 'Try signing in with a different account.',
  Callback: 'Try signing in with a different account.',
  OAuthAccountNotLinked: 'To confirm your identity, sign in with the same account you used originally.',
  EmailSignin: 'Check your email address.',
  CredentialsSignin: 'Sign in failed. Check the details you provided are correct.',
  SessionRequired: 'Please sign in to access this page.',
  AccessDenied: 'You do not have permission to sign in.',
  Verification: 'The sign in link is no longer valid. It may have been used already or it may have expired.',
  Configuration: 'There was a problem with the server configuration.',
  Default: 'Unable to sign in.',
};

export function SignInForm({ callbackUrl, error }: SignInFormProps) {
  const [isLoading, setIsLoading] = useState(false);
  const [showDeveloperLogin, setShowDeveloperLogin] = useState(false);
  const [devFormData, setDevFormData] = useState({
    email: 'developer@archie.dev',
    name: 'Developer User'
  });

  // Check if we're in development mode
  const isDevelopment = process.env.NODE_ENV === 'development';

  const handleAzureSignIn = async () => {
    setIsLoading(true);
    try {
      await signIn('azure-ad', {
        callbackUrl: callbackUrl || '/dashboard',
        redirect: true,
      });
    } catch (error) {
      console.error('Sign in error:', error);
      setIsLoading(false);
    }
  };

  const handleDeveloperSignIn = async () => {
    setIsLoading(true);
    try {
      await signIn('development', {
        email: devFormData.email,
        name: devFormData.name,
        callbackUrl: callbackUrl || '/dashboard',
        redirect: true,
      });
    } catch (error) {
      console.error('Developer sign in error:', error);
      setIsLoading(false);
    }
  };

  return (
    <Card className="w-full max-w-md mx-auto shadow-lg">
      <CardHeader className="space-y-1">
        <CardTitle className="text-2xl font-bold text-center">Sign In</CardTitle>
        <CardDescription className="text-center">
          {showDeveloperLogin 
            ? 'Development Mode - Use any credentials'
            : 'Use your organizational account to access Archie'
          }
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {error && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              {errorMessages[error] || errorMessages.Default}
            </AlertDescription>
          </Alert>
        )}
        
        <div className="space-y-4">
          {!showDeveloperLogin ? (
            <>
              {/* Azure AD Sign In */}
              <Button
                onClick={handleAzureSignIn}
                disabled={isLoading}
                className="w-full h-11 text-base font-medium"
                size="lg"
              >
                {isLoading ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Signing in...
                  </>
                ) : (
                  <>
                    <Building2 className="mr-2 h-4 w-4" />
                    Continue with Azure AD
                  </>
                )}
              </Button>
              
              {/* Development Mode Toggle */}
              {isDevelopment && (
                <div className="text-center">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setShowDeveloperLogin(true)}
                    className="text-xs"
                  >
                    <Code className="mr-1 h-3 w-3" />
                    Development Mode
                  </Button>
                </div>
              )}
            </>
          ) : (
            <>
              {/* Developer Sign In Form */}
              <div className="space-y-3">
                <div className="space-y-2">
                  <Label htmlFor="dev-email">Email</Label>
                  <Input
                    id="dev-email"
                    type="email"
                    placeholder="developer@archie.dev"
                    value={devFormData.email}
                    onChange={(e) => setDevFormData({...devFormData, email: e.target.value})}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="dev-name">Name</Label>
                  <Input
                    id="dev-name"
                    type="text"
                    placeholder="Developer User"
                    value={devFormData.name}
                    onChange={(e) => setDevFormData({...devFormData, name: e.target.value})}
                  />
                </div>
              </div>

              <Button
                onClick={handleDeveloperSignIn}
                disabled={isLoading || !devFormData.email}
                className="w-full h-11 text-base font-medium"
                size="lg"
              >
                {isLoading ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Signing in...
                  </>
                ) : (
                  <>
                    <User className="mr-2 h-4 w-4" />
                    Sign In as Developer
                  </>
                )}
              </Button>

              <div className="text-center">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setShowDeveloperLogin(false)}
                  className="text-xs"
                >
                  Back to Azure AD
                </Button>
              </div>
            </>
          )}
        </div>

        {!showDeveloperLogin && (
          <>
            <div className="text-center space-y-2">
              <p className="text-sm text-muted-foreground">
                Secure authentication powered by Microsoft
              </p>
            </div>

            {/* Security and compliance info */}
            <div className="bg-muted/50 rounded-lg p-4 space-y-2">
              <div className="flex items-center text-sm font-medium">
                <svg
                  className="mr-2 h-4 w-4 text-green-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
                  />
                </svg>
                Enterprise Security
              </div>
              <p className="text-xs text-muted-foreground ml-6">
                Your authentication is handled securely through your organization's Azure Active Directory
              </p>
            </div>
          </>
        )}

        {showDeveloperLogin && (
          <div className="bg-yellow-50 dark:bg-yellow-900/20 rounded-lg p-4 space-y-2">
            <div className="flex items-center text-sm font-medium text-yellow-800 dark:text-yellow-200">
              <Code className="mr-2 h-4 w-4" />
              Development Mode
            </div>
            <p className="text-xs text-yellow-700 dark:text-yellow-300 ml-6">
              This is for development only. Any email/name combination will work.
            </p>
          </div>
        )}
      </CardContent>
    </Card>
  );
}