'use client';

import React, { useState, useEffect, useMemo } from 'react';
import { useLazyQuery } from '@apollo/client';
import { SEARCH_REPOSITORIES, GET_SEARCH_SUGGESTIONS } from '@/graphql/queries';
import { SearchInput } from './SearchInput';
import { SearchFilters } from './SearchFilters';
import { SearchResults } from './SearchResults';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { 
  Search, 
  Filter, 
  SortAsc, 
  SortDesc,
  History,
  TrendingUp,
  Zap,
  AlertCircle,
  Loader2
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useDebounce } from 'use-debounce';
import { toast } from '@/hooks/use-toast';

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

interface SearchFilters {
  repositories?: string[];
  languages?: string[];
  fileTypes?: string[];
  dateRange?: {
    from: string;
    to: string;
  };
  minScore?: number;
}

interface SearchInterfaceProps {
  onResultSelect?: (result: SearchResult) => void;
  initialQuery?: string;
  className?: string;
}

const RECENT_SEARCHES_KEY = 'archie-recent-searches';
const MAX_RECENT_SEARCHES = 10;

export const SearchInterface: React.FC<SearchInterfaceProps> = ({
  onResultSelect,
  initialQuery = '',
  className = ''
}) => {
  const [query, setQuery] = useState(initialQuery);
  const [filters, setFilters] = useState<SearchFilters>({});
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [recentSearches, setRecentSearches] = useState<string[]>([]);
  const [sortBy, setSortBy] = useState<'relevance' | 'date' | 'repository'>('relevance');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
  const [showFilters, setShowFilters] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [searchDuration, setSearchDuration] = useState(0);

  const [debouncedQuery] = useDebounce(query, 300);

  // GraphQL queries
  const [searchRepositories, { loading: searchLoading, error: searchError }] = useLazyQuery(
    SEARCH_REPOSITORIES,
    {
      errorPolicy: 'all',
      onCompleted: (data) => {
        if (data?.searchRepositories) {
          setResults(data.searchRepositories.results || []);
          setTotalCount(data.searchRepositories.totalCount || 0);
          setSearchDuration(data.searchRepositories.searchDuration || 0);
          setIsSearching(false);
        }
      },
      onError: (error) => {
        console.error('Search error:', error);
        setIsSearching(false);
        toast({
          title: 'Search failed',
          description: error.message,
          variant: 'destructive',
        });
      }
    }
  );

  const [getSuggestions] = useLazyQuery(
    GET_SEARCH_SUGGESTIONS,
    {
      errorPolicy: 'all',
      onCompleted: (data) => {
        if (data?.searchSuggestions) {
          setSuggestions([
            ...(data.searchSuggestions.suggestions || []),
            ...(data.searchSuggestions.popularQueries || [])
          ]);
        }
      }
    }
  );

  // Load recent searches on mount
  useEffect(() => {
    if (typeof window !== 'undefined') {
      const stored = localStorage.getItem(RECENT_SEARCHES_KEY);
      if (stored) {
        try {
          setRecentSearches(JSON.parse(stored));
        } catch (error) {
          console.warn('Failed to parse recent searches:', error);
        }
      }
    }
  }, []);

  // Get suggestions when query changes
  useEffect(() => {
    if (debouncedQuery.length > 2) {
      getSuggestions({
        variables: {
          query: debouncedQuery,
          count: 5
        }
      });
    } else {
      setSuggestions([]);
    }
  }, [debouncedQuery, getSuggestions]);

  // Perform search
  const handleSearch = async (searchQuery: string, searchFilters: SearchFilters = {}) => {
    if (!searchQuery.trim()) {
      setResults([]);
      setTotalCount(0);
      return;
    }

    setIsSearching(true);
    
    // Add to recent searches
    const updatedRecent = [searchQuery, ...recentSearches.filter(s => s !== searchQuery)]
      .slice(0, MAX_RECENT_SEARCHES);
    setRecentSearches(updatedRecent);
    
    if (typeof window !== 'undefined') {
      localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(updatedRecent));
    }

    // Execute search
    await searchRepositories({
      variables: {
        input: {
          query: searchQuery,
          filters: {
            repositories: searchFilters.repositories,
            languages: searchFilters.languages,
            fileTypes: searchFilters.fileTypes,
            dateRange: searchFilters.dateRange,
            minScore: searchFilters.minScore || 0.1
          },
          sortBy,
          sortOrder,
          limit: 50
        }
      }
    });
  };

  // Sort results
  const sortedResults = useMemo(() => {
    const sorted = [...results].sort((a, b) => {
      let comparison = 0;
      
      switch (sortBy) {
        case 'relevance':
          comparison = b.score - a.score;
          break;
        case 'date':
          const dateA = a.lastModified ? new Date(a.lastModified).getTime() : 0;
          const dateB = b.lastModified ? new Date(b.lastModified).getTime() : 0;
          comparison = dateB - dateA;
          break;
        case 'repository':
          comparison = a.repositoryName.localeCompare(b.repositoryName);
          break;
      }
      
      return sortOrder === 'asc' ? comparison : -comparison;
    });
    
    return sorted;
  }, [results, sortBy, sortOrder]);

  const handleQuerySubmit = (newQuery: string) => {
    setQuery(newQuery);
    handleSearch(newQuery, filters);
  };

  const handleFilterChange = (newFilters: SearchFilters) => {
    setFilters(newFilters);
    if (query.trim()) {
      handleSearch(query, newFilters);
    }
  };

  const clearSearch = () => {
    setQuery('');
    setResults([]);
    setTotalCount(0);
    setSuggestions([]);
  };

  const clearRecentSearches = () => {
    setRecentSearches([]);
    if (typeof window !== 'undefined') {
      localStorage.removeItem(RECENT_SEARCHES_KEY);
    }
  };

  return (
    <div className={cn('flex flex-col h-full', className)}>
      {/* Search Header */}
      <div className="border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
        <div className="p-6">
          <div className="flex items-center space-x-4 mb-4">
            <Search className="h-6 w-6 text-blue-600" />
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
              Search Repositories
            </h1>
          </div>
          
          {/* Search Input */}
          <SearchInput
            query={query}
            suggestions={suggestions}
            recentSearches={recentSearches}
            onQueryChange={setQuery}
            onSearch={handleQuerySubmit}
            onClear={clearSearch}
            isLoading={isSearching || searchLoading}
            className="mb-4"
          />
          
          {/* Controls Row */}
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <Button
                variant={showFilters ? "default" : "outline"}
                size="sm"
                onClick={() => setShowFilters(!showFilters)}
              >
                <Filter className="h-4 w-4 mr-1" />
                Filters
                {Object.keys(filters).length > 0 && (
                  <Badge variant="secondary" className="ml-2">
                    {Object.values(filters).flat().filter(Boolean).length}
                  </Badge>
                )}
              </Button>
              
              {recentSearches.length > 0 && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={clearRecentSearches}
                >
                  <History className="h-4 w-4 mr-1" />
                  Clear History
                </Button>
              )}
            </div>
            
            {/* Sort Controls */}
            {results.length > 0 && (
              <div className="flex items-center space-x-2">
                <span className="text-sm text-gray-600 dark:text-gray-400">Sort by:</span>
                <select
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value as any)}
                  className="text-sm border border-gray-300 rounded px-2 py-1 dark:bg-gray-700 dark:border-gray-600"
                >
                  <option value="relevance">Relevance</option>
                  <option value="date">Date</option>
                  <option value="repository">Repository</option>
                </select>
                
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')}
                >
                  {sortOrder === 'asc' ? (
                    <SortAsc className="h-4 w-4" />
                  ) : (
                    <SortDesc className="h-4 w-4" />
                  )}
                </Button>
              </div>
            )}
          </div>
        </div>
        
        {/* Filters Panel */}
        {showFilters && (
          <div className="border-t border-gray-200 dark:border-gray-700 p-4">
            <SearchFilters
              filters={filters}
              onFiltersChange={handleFilterChange}
            />
          </div>
        )}
      </div>

      {/* Search Results */}
      <div className="flex-1 overflow-auto">
        {searchError && (
          <div className="p-4">
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>
                Search failed: {searchError.message}
              </AlertDescription>
            </Alert>
          </div>
        )}
        
        {/* Results Header */}
        {(totalCount > 0 || isSearching) && (
          <div className="p-4 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                {isSearching ? (
                  <div className="flex items-center space-x-2">
                    <Loader2 className="h-4 w-4 animate-spin" />
                    <span className="text-sm text-gray-600 dark:text-gray-400">
                      Searching...
                    </span>
                  </div>
                ) : (
                  <div className="text-sm text-gray-600 dark:text-gray-400">
                    Found <strong>{totalCount.toLocaleString()}</strong> results
                    {searchDuration > 0 && (
                      <span className="ml-2 flex items-center space-x-1">
                        <Zap className="h-3 w-3" />
                        <span>{searchDuration}ms</span>
                      </span>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
        
        {/* Search Results */}
        <SearchResults
          results={sortedResults}
          isLoading={isSearching || searchLoading}
          onResultSelect={onResultSelect}
          searchTerm={query}
        />
        
        {/* Empty States */}
        {!isSearching && !searchLoading && totalCount === 0 && query && (
          <div className="flex items-center justify-center h-64">
            <div className="text-center">
              <Search className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                No results found
              </h3>
              <p className="text-gray-500 dark:text-gray-400 mb-4">
                Try adjusting your search terms or filters
              </p>
              <div className="space-x-2">
                <Button variant="outline" size="sm" onClick={clearSearch}>
                  Clear Search
                </Button>
                <Button 
                  variant="outline" 
                  size="sm" 
                  onClick={() => setShowFilters(!showFilters)}
                >
                  {showFilters ? 'Hide' : 'Show'} Filters
                </Button>
              </div>
            </div>
          </div>
        )}
        
        {/* Welcome State */}
        {!query && recentSearches.length === 0 && (
          <div className="flex items-center justify-center h-64">
            <div className="text-center">
              <TrendingUp className="h-12 w-12 text-blue-500 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                Search across all repositories
              </h3>
              <p className="text-gray-500 dark:text-gray-400">
                Find files, functions, classes, and documentation content
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchInterface;