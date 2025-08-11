'use client';

import React, { useState } from 'react';
import { useQuery } from '@apollo/client';
import { GET_REPOSITORIES } from '@/graphql/queries';
import { DocumentationViewer } from '@/components/documentation/DocumentationViewer';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { 
  BookOpen,
  Search,
  FileText,
  Clock,
  Loader2,
  AlertCircle
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { MainLayout } from '@/components/layout/MainLayout';

export default function DocumentationPage() {
  const [selectedRepository, setSelectedRepository] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  
  const { data, loading, error } = useQuery(GET_REPOSITORIES, {
    variables: {
      filter: {
        hasDocumentation: true,
        ...(searchTerm && { name: searchTerm })
      }
    },
    errorPolicy: 'all'
  });
  
  const repositories = data?.repositories || [];
  
  if (selectedRepository) {
    return (
      <MainLayout>
        <div className="h-full">
          <div className="flex items-center justify-between mb-6">
            <Button
              variant="outline"
              onClick={() => setSelectedRepository(null)}
            >
              ← Back to Documentation
            </Button>
          </div>
          <DocumentationViewer repositoryId={selectedRepository} />
        </div>
      </MainLayout>
    );
  }
  
  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
              Documentation
            </h1>
            <p className="text-gray-600 dark:text-gray-400">
              Browse AI-generated documentation for your repositories
            </p>
          </div>
        </div>
        
        {/* Search */}
        <div className="max-w-md">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <Input
              placeholder="Search repositories with documentation..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>
        
        {/* Repository Documentation List */}
        {loading ? (
          <div className="flex items-center justify-center h-64">
            <div className="text-center">
              <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
              <p>Loading documentation...</p>
            </div>
          </div>
        ) : error ? (
          <div className="text-center py-12">
            <AlertCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
            <div className="text-red-600 mb-4">Failed to load documentation</div>
            <Button variant="outline">
              Try Again
            </Button>
          </div>
        ) : repositories.length === 0 ? (
          <div className="text-center py-12">
            <BookOpen className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
              No documentation found
            </h3>
            <p className="text-gray-600 dark:text-gray-400 mb-4">
              {searchTerm 
                ? 'No repositories match your search criteria.'
                : 'Generate documentation for your repositories to see it here.'}
            </p>
            <Button variant="outline">
              Generate Documentation
            </Button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {repositories.map((repo: any) => (
              <Card 
                key={repo.id} 
                className="p-6 cursor-pointer hover:shadow-lg transition-shadow"
                onClick={() => setSelectedRepository(repo.id)}
              >
                <div className="space-y-4">
                  {/* Header */}
                  <div className="flex items-start justify-between">
                    <div className="flex items-center space-x-3">
                      <div className="w-10 h-10 bg-green-100 dark:bg-green-900 rounded-lg flex items-center justify-center">
                        <BookOpen className="w-5 h-5 text-green-600 dark:text-green-400" />
                      </div>
                      <div>
                        <h3 className="font-semibold text-gray-900 dark:text-white">
                          {repo.name}
                        </h3>
                        {repo.description && (
                          <p className="text-sm text-gray-600 dark:text-gray-400 line-clamp-1">
                            {repo.description}
                          </p>
                        )}
                      </div>
                    </div>
                    
                    <Badge 
                      className={cn(
                        repo.documentationStatus === 'ready' 
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
                          : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300'
                      )}
                    >
                      {repo.documentationStatus || 'Available'}
                    </Badge>
                  </div>
                  
                  {/* Documentation Info */}
                  <div className="space-y-2">
                    {repo.documentationLastGenerated && (
                      <div className="flex items-center space-x-2 text-sm text-gray-600 dark:text-gray-400">
                        <Clock className="w-4 h-4" />
                        <span>
                          Last updated: {new Date(repo.documentationLastGenerated).toLocaleDateString()}
                        </span>
                      </div>
                    )}
                    
                    <div className="flex items-center space-x-2">
                      <Badge variant="outline" className="text-xs">
                        <FileText className="w-3 h-3 mr-1" />
                        Full Documentation
                      </Badge>
                      
                      {repo.language && (
                        <Badge variant="secondary" className="text-xs">
                          {repo.language}
                        </Badge>
                      )}
                    </div>
                  </div>
                  
                  {/* Preview */}
                  <div className="text-sm text-gray-600 dark:text-gray-400">
                    Click to view complete documentation with code examples, 
                    architecture overview, and usage guides.
                  </div>
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>
    </MainLayout>
  );
}