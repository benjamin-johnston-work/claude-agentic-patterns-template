'use client';

import React, { useState, useMemo } from 'react';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { 
  ExternalLink,
  FileText,
  Folder,
  Clock,
  Star,
  Eye,
  Code,
  Search,
  Loader2
} from 'lucide-react';
import { cn } from '@/lib/utils';

interface SearchResult {
  id: string;
  title: string;
  content: string;
  excerpt: string;
  repositoryId: string;
  repositoryName: string;
  filePath?: string;
  language?: string;
  score: number;
  highlights: string[];
  lastModified?: string;
}

interface SearchResultsProps {
  results: SearchResult[];
  isLoading: boolean;
  onResultSelect?: (result: SearchResult) => void;
  searchTerm?: string;
  viewMode?: 'list' | 'grid';
  className?: string;
}

const SearchResultSkeleton: React.FC = () => (
  <Card className="p-4 animate-pulse">
    <div className="space-y-3">
      <div className="flex items-center space-x-2">
        <div className="h-4 w-4 bg-gray-200 dark:bg-gray-700 rounded" />
        <div className="h-4 w-24 bg-gray-200 dark:bg-gray-700 rounded" />
        <div className="h-4 w-16 bg-gray-200 dark:bg-gray-700 rounded" />
      </div>
      <div className="h-5 w-3/4 bg-gray-200 dark:bg-gray-700 rounded" />
      <div className="space-y-2">
        <div className="h-3 w-full bg-gray-200 dark:bg-gray-700 rounded" />
        <div className="h-3 w-5/6 bg-gray-200 dark:bg-gray-700 rounded" />
        <div className="h-3 w-4/6 bg-gray-200 dark:bg-gray-700 rounded" />
      </div>
      <div className="flex items-center space-x-4 text-xs">
        <div className="h-3 w-12 bg-gray-200 dark:bg-gray-700 rounded" />
        <div className="h-3 w-16 bg-gray-200 dark:bg-gray-700 rounded" />
      </div>
    </div>
  </Card>
);

const getFileIcon = (fileName?: string, language?: string) => {
  if (!fileName && !language) return <FileText className="h-4 w-4" />;
  
  const ext = fileName?.split('.').pop()?.toLowerCase();
  const lang = language?.toLowerCase();
  
  // Icon mapping based on file extension or language
  if (lang === 'javascript' || ext === 'js') return '🟨';
  if (lang === 'typescript' || ext === 'ts' || ext === 'tsx') return '🔷';
  if (lang === 'python' || ext === 'py') return '🐍';
  if (lang === 'java' || ext === 'java') return '☕';
  if (lang === 'csharp' || ext === 'cs') return '💙';
  if (lang === 'go' || ext === 'go') return '🐹';
  if (lang === 'rust' || ext === 'rs') return '🦀';
  if (lang === 'php' || ext === 'php') return '🐘';
  if (lang === 'ruby' || ext === 'rb') return '💎';
  if (ext === 'html') return '🌐';
  if (ext === 'css' || ext === 'scss') return '🎨';
  if (ext === 'json') return '📋';
  if (ext === 'md') return '📝';
  
  return <FileText className="h-4 w-4 text-gray-500" />;
};

const highlightSearchTerm = (text: string, searchTerm: string): React.ReactNode => {
  if (!searchTerm.trim()) return text;
  
  const regex = new RegExp(`(${searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
  const parts = text.split(regex);
  
  return (
    <>
      {parts.map((part, index) => 
        regex.test(part) ? (
          <mark key={index} className="bg-yellow-200 dark:bg-yellow-700 px-1 rounded">
            {part}
          </mark>
        ) : (
          part
        )
      )}
    </>
  );
};

const SearchResultCard: React.FC<{
  result: SearchResult;
  onSelect?: (result: SearchResult) => void;
  searchTerm?: string;
  viewMode: 'list' | 'grid';
}> = ({ result, onSelect, searchTerm = '', viewMode }) => {
  const handleClick = () => {
    onSelect?.(result);
  };

  const scorePercentage = Math.round(result.score * 100);
  const isHighRelevance = result.score >= 0.8;
  const isMediumRelevance = result.score >= 0.5;

  return (
    <Card 
      className={cn(
        'p-4 hover:shadow-md transition-all duration-200 border-l-4',
        isHighRelevance ? 'border-l-green-500' : 
        isMediumRelevance ? 'border-l-yellow-500' : 'border-l-gray-300',
        onSelect && 'cursor-pointer hover:border-blue-500'
      )}
      onClick={handleClick}
    >
      <div className="space-y-3">
        {/* Header */}
        <div className="flex items-start justify-between">
          <div className="flex items-center space-x-2 min-w-0 flex-1">
            <span className="flex-shrink-0 text-lg">
              {getFileIcon(result.filePath, result.language)}
            </span>
            
            <div className="flex items-center space-x-2 min-w-0">
              <Badge variant="outline" className="text-xs flex-shrink-0">
                <Folder className="h-3 w-3 mr-1" />
                {result.repositoryName}
              </Badge>
              
              {result.language && (
                <Badge variant="secondary" className="text-xs flex-shrink-0">
                  <Code className="h-3 w-3 mr-1" />
                  {result.language}
                </Badge>
              )}
            </div>
          </div>
          
          <div className="flex items-center space-x-2 flex-shrink-0">
            <div className={cn(
              'flex items-center space-x-1 text-xs px-2 py-1 rounded-full',
              isHighRelevance ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300' :
              isMediumRelevance ? 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900 dark:text-yellow-300' :
              'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300'
            )}>
              <Star className="h-3 w-3" />
              <span>{scorePercentage}%</span>
            </div>
            
            {onSelect && (
              <Button variant="ghost" size="sm">
                <Eye className="h-4 w-4" />
              </Button>
            )}
          </div>
        </div>
        
        {/* Title */}
        <div>
          <h3 className={cn(
            'font-semibold text-gray-900 dark:text-white mb-1',
            viewMode === 'grid' ? 'text-sm' : 'text-base'
          )}>
            {highlightSearchTerm(result.title, searchTerm)}
          </h3>
          
          {result.filePath && (
            <p className="text-xs text-gray-500 dark:text-gray-400 font-mono">
              {result.filePath}
            </p>
          )}
        </div>
        
        {/* Content Preview */}
        <div className="text-sm text-gray-700 dark:text-gray-300">
          <p className={cn(
            'leading-relaxed',
            viewMode === 'grid' ? 'line-clamp-3' : 'line-clamp-4'
          )}>
            {highlightSearchTerm(result.excerpt || result.content.substring(0, 200) + '...', searchTerm)}
          </p>
        </div>
        
        {/* Highlights */}
        {result.highlights && result.highlights.length > 0 && (
          <div className="space-y-2">
            <p className="text-xs text-gray-500 dark:text-gray-400 font-medium">
              Matching content:
            </p>
            <div className="space-y-1">
              {result.highlights.slice(0, viewMode === 'grid' ? 2 : 3).map((highlight, index) => (
                <div 
                  key={index} 
                  className="text-xs bg-gray-50 dark:bg-gray-800 p-2 rounded border-l-2 border-blue-200 font-mono"
                >
                  {highlightSearchTerm(highlight, searchTerm)}
                </div>
              ))}
              {result.highlights.length > (viewMode === 'grid' ? 2 : 3) && (
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  +{result.highlights.length - (viewMode === 'grid' ? 2 : 3)} more matches
                </p>
              )}
            </div>
          </div>
        )}
        
        {/* Footer */}
        <div className="flex items-center justify-between pt-2 border-t border-gray-100 dark:border-gray-800">
          <div className="flex items-center space-x-4 text-xs text-gray-500 dark:text-gray-400">
            {result.lastModified && (
              <div className="flex items-center space-x-1">
                <Clock className="h-3 w-3" />
                <span>
                  {new Date(result.lastModified).toLocaleDateString()}
                </span>
              </div>
            )}
            
            <span>Score: {scorePercentage}%</span>
          </div>
          
          {onSelect && (
            <Button variant="ghost" size="sm" className="text-xs">
              <ExternalLink className="h-3 w-3 mr-1" />
              View
            </Button>
          )}
        </div>
      </div>
    </Card>
  );
};

export const SearchResults: React.FC<SearchResultsProps> = ({
  results,
  isLoading,
  onResultSelect,
  searchTerm = '',
  viewMode = 'list',
  className = ''
}) => {
  const [currentPage, setCurrentPage] = useState(1);
  const resultsPerPage = 10;
  
  const paginatedResults = useMemo(() => {
    const startIndex = (currentPage - 1) * resultsPerPage;
    const endIndex = startIndex + resultsPerPage;
    return results.slice(startIndex, endIndex);
  }, [results, currentPage, resultsPerPage]);
  
  const totalPages = Math.ceil(results.length / resultsPerPage);
  
  if (isLoading) {
    return (
      <div className={cn('p-6', className)}>
        <div className={cn(
          'space-y-4',
          viewMode === 'grid' && 'grid grid-cols-1 md:grid-cols-2 gap-4'
        )}>
          {Array.from({ length: 5 }).map((_, i) => (
            <SearchResultSkeleton key={i} />
          ))}
        </div>
      </div>
    );
  }
  
  if (results.length === 0) {
    return null; // Empty state handled by parent component
  }
  
  return (
    <div className={cn('p-6', className)}>
      {/* Results Grid/List */}
      <div className={cn(
        viewMode === 'grid' 
          ? 'grid grid-cols-1 lg:grid-cols-2 gap-4'
          : 'space-y-4'
      )}>
        {paginatedResults.map((result) => (
          <SearchResultCard
            key={result.id}
            result={result}
            onSelect={onResultSelect}
            searchTerm={searchTerm}
            viewMode={viewMode}
          />
        ))}
      </div>
      
      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-center space-x-2 mt-8">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
            disabled={currentPage === 1}
          >
            Previous
          </Button>
          
          <div className="flex items-center space-x-1">
            {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
              const pageNum = i + 1;
              return (
                <Button
                  key={pageNum}
                  variant={currentPage === pageNum ? "default" : "outline"}
                  size="sm"
                  onClick={() => setCurrentPage(pageNum)}
                >
                  {pageNum}
                </Button>
              );
            })}
            
            {totalPages > 5 && (
              <>
                <span className="px-2">...</span>
                <Button
                  variant={currentPage === totalPages ? "default" : "outline"}
                  size="sm"
                  onClick={() => setCurrentPage(totalPages)}
                >
                  {totalPages}
                </Button>
              </>
            )}
          </div>
          
          <Button
            variant="outline"
            size="sm"
            onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
            disabled={currentPage === totalPages}
          >
            Next
          </Button>
        </div>
      )}
      
      {/* Results Summary */}
      <div className="mt-6 text-center text-sm text-gray-500 dark:text-gray-400">
        Showing {(currentPage - 1) * resultsPerPage + 1}-{Math.min(currentPage * resultsPerPage, results.length)} of {results.length} results
      </div>
    </div>
  );
};

export default SearchResults;