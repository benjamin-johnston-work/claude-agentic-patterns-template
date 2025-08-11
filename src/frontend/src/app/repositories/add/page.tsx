'use client';

import React, { useState } from 'react';
import { useMutation } from '@apollo/client';
import { useRouter } from 'next/navigation';
import { ADD_REPOSITORY, VALIDATE_REPOSITORY } from '@/graphql/mutations';
import { GET_REPOSITORIES } from '@/graphql/queries';
import { MainLayout } from '@/components/layout/MainLayout';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { 
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useToast } from '@/hooks/use-toast';
import { 
  ArrowLeft,
  GitBranch,
  Github,
  Lock,
  Loader2,
  CheckCircle,
  AlertCircle,
  ExternalLink,
  Info
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { AddRepositoryInput, RepositoryStatus } from '@/types';

interface FormData extends AddRepositoryInput {
  url: string;
  accessToken?: string;
  branch?: string;
}

interface ValidationErrors {
  url?: string;
  accessToken?: string;
  branch?: string;
}

export default function AddRepositoryPage() {
  const router = useRouter();
  const { toast } = useToast();
  
  const [formData, setFormData] = useState<FormData>({
    url: '',
    accessToken: '',
    branch: 'main'
  });
  
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const [isValidating, setIsValidating] = useState(false);
  const [validationResult, setValidationResult] = useState<{
    isValid: boolean;
    repository?: {
      name: string;
      description: string;
      language: string;
      isPrivate: boolean;
      branches: string[];
    };
  } | null>(null);

  const [addRepository, { loading: isSubmitting }] = useMutation(ADD_REPOSITORY, {
    refetchQueries: [{ query: GET_REPOSITORIES }],
    onCompleted: (data) => {
      toast({
        title: "Repository Connected",
        description: `${data.addRepository.name} has been successfully connected and is being analyzed.`,
      });
      router.push('/repositories');
    },
    onError: (error) => {
      toast({
        title: "Connection Failed",
        description: error.message || "Failed to connect repository. Please check your credentials and try again.",
        variant: "destructive",
      });
    },
  });

  const [validateRepositoryMutation, { loading: isValidationLoading }] = useMutation(VALIDATE_REPOSITORY, {
    onCompleted: (data) => {
      if (data.validateRepository.isValid) {
        const mockResult = {
          isValid: true,
          repository: {
            name: data.validateRepository.repository.name,
            description: data.validateRepository.repository.description,
            language: data.validateRepository.repository.language,
            isPrivate: data.validateRepository.repository.isPrivate,
            branches: data.validateRepository.repository.branches
          }
        };
        
        setValidationResult(mockResult);
        setIsValidating(false);
        
        if (mockResult.repository?.branches && !formData.branch) {
          setFormData(prev => ({ 
            ...prev, 
            branch: mockResult.repository!.branches.includes('main') ? 'main' : 
                    mockResult.repository!.branches.includes('master') ? 'master' :
                    mockResult.repository!.branches[0]
          }));
        }
      } else {
        setValidationResult({ isValid: false });
        setIsValidating(false);
        toast({
          title: "Validation Failed",
          description: "Repository is not accessible or does not exist",
          variant: "destructive",
        });
      }
    },
    onError: (error) => {
      setValidationResult({ isValid: false });
      setIsValidating(false);
      toast({
        title: "Validation Failed",
        description: error.message || "Failed to validate repository",
        variant: "destructive",
      });
    },
  });

  const validateRepositoryUrl = (url: string): boolean => {
    if (!url.trim()) {
      setValidationErrors(prev => ({ ...prev, url: 'Repository URL is required' }));
      return false;
    }

    // GitHub URL validation
    const githubRegex = /^https:\/\/github\.com\/[\w\-\.]+\/[\w\-\.]+(?:\.git)?$/;
    if (!githubRegex.test(url.trim())) {
      setValidationErrors(prev => ({ 
        ...prev, 
        url: 'Please enter a valid GitHub repository URL (e.g., https://github.com/username/repository)' 
      }));
      return false;
    }

    setValidationErrors(prev => ({ ...prev, url: undefined }));
    return true;
  };

  const handleUrlChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const url = e.target.value;
    setFormData(prev => ({ ...prev, url }));
    setValidationResult(null);
    
    if (validationErrors.url) {
      validateRepositoryUrl(url);
    }
  };

  const handleUrlBlur = () => {
    validateRepositoryUrl(formData.url);
  };

  const validateRepository = async () => {
    if (!validateRepositoryUrl(formData.url)) {
      return;
    }

    setIsValidating(true);
    
    // Make real API call to validate repository
    await validateRepositoryMutation({
      variables: {
        input: {
          url: formData.url.trim(),
          ...(formData.accessToken?.trim() && { accessToken: formData.accessToken.trim() }),
        }
      }
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate form
    const isUrlValid = validateRepositoryUrl(formData.url);
    
    if (!isUrlValid) {
      return;
    }

    const input: AddRepositoryInput = {
      url: formData.url.trim(),
      ...(formData.accessToken?.trim() && { accessToken: formData.accessToken.trim() }),
      ...(formData.branch?.trim() && { branch: formData.branch.trim() }),
    };

    await addRepository({
      variables: { input }
    });
  };

  const handleBack = () => {
    router.push('/repositories');
  };

  const canSubmit = formData.url.trim() && !validationErrors.url && !isSubmitting && !isValidating && !isValidationLoading;

  return (
    <MainLayout>
      <div className="max-w-2xl mx-auto space-y-6">
        {/* Header */}
        <div className="flex items-center space-x-4">
          <Button variant="outline" onClick={handleBack} disabled={isSubmitting}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Repositories
          </Button>
        </div>

        {/* Main Form Card */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center space-x-2">
              <Github className="h-5 w-5" />
              <span>Connect Repository</span>
            </CardTitle>
            <CardDescription>
              Connect a GitHub repository to start analyzing and generating documentation
            </CardDescription>
          </CardHeader>
          
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Repository URL */}
              <div className="space-y-2">
                <label htmlFor="url" className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Repository URL *
                </label>
                <div className="relative">
                  <Input
                    id="url"
                    type="url"
                    placeholder="https://github.com/username/repository"
                    value={formData.url}
                    onChange={handleUrlChange}
                    onBlur={handleUrlBlur}
                    disabled={isSubmitting}
                    className={cn(
                      validationErrors.url && "border-red-500 focus-visible:ring-red-500"
                    )}
                  />
                  <Github className="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                </div>
                {validationErrors.url && (
                  <p className="text-sm text-red-600 flex items-center space-x-1">
                    <AlertCircle className="h-3 w-3" />
                    <span>{validationErrors.url}</span>
                  </p>
                )}
                <p className="text-xs text-gray-500">
                  Enter the full GitHub repository URL. Public repositories don't require an access token.
                </p>
              </div>

              {/* Repository Validation Button */}
              {formData.url && !validationErrors.url && !validationResult && (
                <div>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={validateRepository}
                    disabled={isValidating || isSubmitting}
                    className="w-full"
                  >
                    {isValidating ? (
                      <>
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Validating Repository...
                      </>
                    ) : (
                      <>
                        <CheckCircle className="h-4 w-4 mr-2" />
                        Validate Repository
                      </>
                    )}
                  </Button>
                </div>
              )}

              {/* Repository Validation Results */}
              {validationResult && (
                <Card className="border-green-200 bg-green-50 dark:border-green-800 dark:bg-green-900/20">
                  <CardContent className="pt-6">
                    <div className="flex items-start space-x-3">
                      <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400 mt-0.5" />
                      <div className="flex-1 space-y-2">
                        <h3 className="font-medium text-green-900 dark:text-green-100">
                          Repository Validated Successfully
                        </h3>
                        {validationResult.repository && (
                          <div className="space-y-1 text-sm text-green-700 dark:text-green-300">
                            <p><strong>Name:</strong> {validationResult.repository.name}</p>
                            <p><strong>Language:</strong> {validationResult.repository.language}</p>
                            <p><strong>Type:</strong> {validationResult.repository.isPrivate ? 'Private' : 'Public'}</p>
                            <p><strong>Available Branches:</strong> {validationResult.repository.branches.join(', ')}</p>
                          </div>
                        )}
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}

              {/* Access Token */}
              <div className="space-y-2">
                <label htmlFor="accessToken" className="text-sm font-medium text-gray-700 dark:text-gray-300 flex items-center space-x-2">
                  <Lock className="h-3 w-3" />
                  <span>Access Token (Optional)</span>
                </label>
                <Input
                  id="accessToken"
                  type="password"
                  placeholder="ghp_xxxxxxxxxxxxxxxxxxxx"
                  value={formData.accessToken}
                  onChange={(e) => setFormData(prev => ({ ...prev, accessToken: e.target.value }))}
                  disabled={isSubmitting}
                />
                <div className="text-xs text-gray-500 space-y-1">
                  <p>Required only for private repositories or to increase rate limits.</p>
                  <p className="flex items-center space-x-1">
                    <ExternalLink className="h-3 w-3" />
                    <a 
                      href="https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token"
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-200"
                    >
                      Learn how to create a GitHub token
                    </a>
                  </p>
                </div>
              </div>

              {/* Branch Selection */}
              {validationResult?.repository?.branches && (
                <div className="space-y-2">
                  <label htmlFor="branch" className="text-sm font-medium text-gray-700 dark:text-gray-300 flex items-center space-x-2">
                    <GitBranch className="h-3 w-3" />
                    <span>Branch</span>
                  </label>
                  <Select
                    value={formData.branch}
                    onValueChange={(value) => setFormData(prev => ({ ...prev, branch: value }))}
                    disabled={isSubmitting}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select a branch" />
                    </SelectTrigger>
                    <SelectContent>
                      {validationResult.repository.branches.map((branch) => (
                        <SelectItem key={branch} value={branch}>
                          {branch} {branch === 'main' || branch === 'master' ? '(default)' : ''}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-gray-500">
                    Select the branch to analyze. You can change this later.
                  </p>
                </div>
              )}

              {/* Info Box */}
              <Card className="border-blue-200 bg-blue-50 dark:border-blue-800 dark:bg-blue-900/20">
                <CardContent className="pt-6">
                  <div className="flex items-start space-x-3">
                    <Info className="h-5 w-5 text-blue-600 dark:text-blue-400 mt-0.5" />
                    <div className="text-sm text-blue-700 dark:text-blue-300 space-y-2">
                      <h3 className="font-medium">What happens next?</h3>
                      <ul className="list-disc list-inside space-y-1 ml-4">
                        <li>We'll clone and analyze your repository structure</li>
                        <li>Code files will be indexed for intelligent search</li>
                        <li>AI-powered documentation will be generated automatically</li>
                        <li>Knowledge graphs will be built to map code relationships</li>
                      </ul>
                      <p>This process typically takes 2-5 minutes depending on repository size.</p>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Submit Button */}
              <div className="flex justify-end space-x-3">
                <Button type="button" variant="outline" onClick={handleBack} disabled={isSubmitting}>
                  Cancel
                </Button>
                <Button type="submit" disabled={!canSubmit}>
                  {isSubmitting ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Connecting...
                    </>
                  ) : (
                    <>
                      <Github className="h-4 w-4 mr-2" />
                      Connect Repository
                    </>
                  )}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </MainLayout>
  );
}