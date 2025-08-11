'use client';

import React, { useState } from 'react';
import { useQuery } from '@apollo/client';
import { GET_REPOSITORIES } from '@/graphql/queries';
import { RepositoryExplorer } from '@/components/repository/RepositoryExplorer';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { 
  Plus,
  Search,
  Filter,
  Grid,
  List,
  FolderOpen,
  GitBranch,
  Clock,
  Code,
  FileText,
  Loader2
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { MainLayout } from '@/components/layout/MainLayout';
import Link from 'next/link';

export default function RepositoriesPage() {
  const [selectedRepository, setSelectedRepository] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [showFilters, setShowFilters] = useState(false);
  
  const { data, loading, error, refetch } = useQuery(GET_REPOSITORIES, {
    variables: {
      filter: searchTerm ? { name: searchTerm } : undefined
    },
    errorPolicy: 'all'
  });
  
  const repositories = data?.repositories || [];
  
  const handleRepositorySelect = (repositoryId: string) => {
    setSelectedRepository(repositoryId);
  };
  
  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'ready':
      case 'indexed':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300';
      case 'analyzing':
      case 'indexing':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300';
      case 'error':
      case 'failed':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300';
    }
  };
  
  if (selectedRepository) {
    return (
      <MainLayout>
        <div className="h-full">
          <div className="flex items-center justify-between mb-6">
            <Button
              variant="outline"
              onClick={() => setSelectedRepository(null)}
            >
              ← Back to Repositories
            </Button>
          </div>
          <RepositoryExplorer repositoryId={selectedRepository} />
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
              Repositories
            </h1>
            <p className="text-gray-600 dark:text-gray-400">
              Manage and explore your connected repositories
            </p>
          </div>
          
          <div className="flex items-center space-x-2">
            <Link href="/repositories/add">
              <Button variant="outline">
                <Plus className="h-4 w-4 mr-2" />
                Connect Repository
              </Button>
            </Link>
          </div>
        </div>
        
        {/* Search and Controls */}
        <div className="flex items-center justify-between space-x-4">
          <div className="flex-1 max-w-md">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Search repositories..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </div>
          
          <div className="flex items-center space-x-2">
            <Button
              variant={showFilters ? 'default' : 'outline'}
              size="sm"
              onClick={() => setShowFilters(!showFilters)}
            >
              <Filter className="h-4 w-4 mr-1" />
              Filters
            </Button>
            
            <div className="flex items-center border rounded-md">
              <Button
                variant={viewMode === 'grid' ? 'default' : 'ghost'}
                size="sm"
                onClick={() => setViewMode('grid')}
                className="rounded-r-none"
              >
                <Grid className="h-4 w-4" />
              </Button>
              <Button
                variant={viewMode === 'list' ? 'default' : 'ghost'}
                size="sm"
                onClick={() => setViewMode('list')}
                className="rounded-l-none"
              >
                <List className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>
        
        {/* Repository Grid/List */}
        {loading ? (
          <div className="flex items-center justify-center h-64">
            <div className="text-center">
              <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
              <p>Loading repositories...</p>
            </div>
          </div>
        ) : error ? (
          <div className="text-center py-12">
            <div className="text-red-600 mb-4">Failed to load repositories</div>
            <Button onClick={() => refetch()} variant="outline">
              Try Again
            </Button>
          </div>
        ) : repositories.length === 0 ? (
          <div className="text-center py-12">
            <FolderOpen className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
              No repositories found
            </h3>
            <p className="text-gray-600 dark:text-gray-400 mb-4">
              {searchTerm ? 'No repositories match your search.' : 'Connect your first repository to get started.'}
            </p>
            <Link href="/repositories/add">
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                Connect Repository
              </Button>
            </Link>
          </div>
        ) : (
          <div className={cn(
            viewMode === 'grid'
              ? 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6'
              : 'space-y-4'
          )}>
            {repositories.map((repo: any) => (
              <Card 
                key={repo.id} 
                className="p-6 cursor-pointer hover:shadow-lg transition-shadow"
                onClick={() => handleRepositorySelect(repo.id)}
              >
                <div className="space-y-4">
                  {/* Repository Header */}
                  <div className="flex items-start justify-between">
                    <div className="flex items-center space-x-3">
                      <div className="w-10 h-10 bg-blue-100 dark:bg-blue-900 rounded-lg flex items-center justify-center">
                        <FolderOpen className="w-5 h-5 text-blue-600 dark:text-blue-400" />
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
                    
                    <Badge className={getStatusColor(repo.status)}>
                      {repo.status}
                    </Badge>
                  </div>
                  
                  {/* Repository Stats */}
                  <div className="flex items-center space-x-4 text-sm text-gray-600 dark:text-gray-400">
                    {repo.language && (
                      <div className="flex items-center space-x-1">
                        <Code className="w-4 h-4" />
                        <span>{repo.language}</span>
                      </div>
                    )}
                    
                    {repo.statistics?.fileCount && (
                      <div className="flex items-center space-x-1">
                        <FileText className="w-4 h-4" />
                        <span>{repo.statistics.fileCount} files</span>
                      </div>
                    )}
                    
                    <div className="flex items-center space-x-1">
                      <Clock className="w-4 h-4" />
                      <span>{new Date(repo.updatedAt).toLocaleDateString()}</span>
                    </div>
                  </div>
                  
                  {/* Features */}
                  <div className="flex items-center space-x-2">
                    {repo.hasDocumentation && (
                      <Badge variant="secondary" className="text-xs">
                        📚 Documentation
                      </Badge>
                    )}
                    {repo.hasKnowledgeGraph && (
                      <Badge variant="secondary" className="text-xs">
                        🕸️ Knowledge Graph
                      </Badge>
                    )}
                    {repo.complexityScore && (
                      <Badge variant="outline" className="text-xs">
                        Complexity: {repo.complexityScore.toFixed(1)}
                      </Badge>
                    )}
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