'use client';

import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Card } from '@/components/ui/card';
import { 
  X, 
  Calendar,
  Folder,
  Code,
  FileType,
  Star,
  Filter,
  RotateCcw
} from 'lucide-react';
import { cn } from '@/lib/utils';

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

interface SearchFiltersProps {
  filters: SearchFilters;
  onFiltersChange: (filters: SearchFilters) => void;
  className?: string;
}

// Common programming languages
const COMMON_LANGUAGES = [
  'JavaScript', 'TypeScript', 'Python', 'Java', 'C#', 'C++', 'Go', 'Rust',
  'PHP', 'Ruby', 'Swift', 'Kotlin', 'Scala', 'R', 'MATLAB', 'SQL'
];

// Common file types
const COMMON_FILE_TYPES = [
  'js', 'ts', 'jsx', 'tsx', 'py', 'java', 'cs', 'cpp', 'c', 'h',
  'go', 'rs', 'php', 'rb', 'swift', 'kt', 'scala', 'sql',
  'html', 'css', 'scss', 'less', 'json', 'xml', 'yaml', 'yml',
  'md', 'txt', 'csv', 'log'
];

export const SearchFilters: React.FC<SearchFiltersProps> = ({
  filters,
  onFiltersChange,
  className = ''
}) => {
  const [newRepository, setNewRepository] = useState('');
  const [newLanguage, setNewLanguage] = useState('');
  const [newFileType, setNewFileType] = useState('');

  const updateFilters = (updates: Partial<SearchFilters>) => {
    onFiltersChange({ ...filters, ...updates });
  };

  const addRepository = () => {
    if (newRepository.trim()) {
      const repositories = filters.repositories || [];
      if (!repositories.includes(newRepository.trim())) {
        updateFilters({
          repositories: [...repositories, newRepository.trim()]
        });
      }
      setNewRepository('');
    }
  };

  const removeRepository = (repo: string) => {
    updateFilters({
      repositories: (filters.repositories || []).filter(r => r !== repo)
    });
  };

  const addLanguage = (language: string) => {
    const languages = filters.languages || [];
    if (!languages.includes(language)) {
      updateFilters({
        languages: [...languages, language]
      });
    }
    setNewLanguage('');
  };

  const removeLanguage = (language: string) => {
    updateFilters({
      languages: (filters.languages || []).filter(l => l !== language)
    });
  };

  const addFileType = (fileType: string) => {
    const fileTypes = filters.fileTypes || [];
    const cleanType = fileType.replace(/^\./,''); // Remove leading dot if present
    if (!fileTypes.includes(cleanType)) {
      updateFilters({
        fileTypes: [...fileTypes, cleanType]
      });
    }
    setNewFileType('');
  };

  const removeFileType = (fileType: string) => {
    updateFilters({
      fileTypes: (filters.fileTypes || []).filter(f => f !== fileType)
    });
  };

  const updateDateRange = (field: 'from' | 'to', value: string) => {
    updateFilters({
      dateRange: {
        from: filters.dateRange?.from || '',
        to: filters.dateRange?.to || '',
        [field]: value
      }
    });
  };

  const clearDateRange = () => {
    updateFilters({ dateRange: undefined });
  };

  const clearAllFilters = () => {
    onFiltersChange({});
    setNewRepository('');
    setNewLanguage('');
    setNewFileType('');
  };

  const hasActiveFilters = Object.keys(filters).some(key => {
    const value = filters[key as keyof SearchFilters];
    if (Array.isArray(value)) return value.length > 0;
    if (typeof value === 'object' && value !== null) return Object.keys(value).length > 0;
    return value !== undefined && value !== null;
  });

  return (
    <Card className={cn('p-4', className)}>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <Filter className="h-4 w-4 text-gray-600" />
            <h3 className="text-sm font-medium text-gray-900 dark:text-white">
              Search Filters
            </h3>
            {hasActiveFilters && (
              <Badge variant="secondary">
                {[
                  ...(filters.repositories || []),
                  ...(filters.languages || []),
                  ...(filters.fileTypes || []),
                  filters.dateRange ? 1 : 0,
                  filters.minScore ? 1 : 0
                ].filter(Boolean).length} active
              </Badge>
            )}
          </div>
          
          {hasActiveFilters && (
            <Button
              variant="ghost"
              size="sm"
              onClick={clearAllFilters}
            >
              <RotateCcw className="h-4 w-4 mr-1" />
              Clear All
            </Button>
          )}
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Repositories Filter */}
          <div className="space-y-2">
            <div className="flex items-center space-x-2">
              <Folder className="h-4 w-4 text-gray-500" />
              <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                Repositories
              </label>
            </div>
            
            <div className="flex space-x-2">
              <Input
                placeholder="Repository name..."
                value={newRepository}
                onChange={(e) => setNewRepository(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && addRepository()}
                className="text-sm"
              />
              <Button
                type="button"
                size="sm"
                onClick={addRepository}
                disabled={!newRepository.trim()}
              >
                Add
              </Button>
            </div>
            
            {filters.repositories && filters.repositories.length > 0 && (
              <div className="flex flex-wrap gap-1">
                {filters.repositories.map((repo) => (
                  <Badge
                    key={repo}
                    variant="secondary"
                    className="text-xs"
                  >
                    {repo}
                    <button
                      onClick={() => removeRepository(repo)}
                      className="ml-1 hover:text-red-500"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))}
              </div>
            )}
          </div>
          
          {/* Languages Filter */}
          <div className="space-y-2">
            <div className="flex items-center space-x-2">
              <Code className="h-4 w-4 text-gray-500" />
              <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                Programming Languages
              </label>
            </div>
            
            <div className="flex space-x-2">
              <Input
                placeholder="Language name..."
                value={newLanguage}
                onChange={(e) => setNewLanguage(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && addLanguage(newLanguage)}
                className="text-sm"
              />
              <Button
                type="button"
                size="sm"
                onClick={() => addLanguage(newLanguage)}
                disabled={!newLanguage.trim()}
              >
                Add
              </Button>
            </div>
            
            {/* Common Languages */}
            <div className="flex flex-wrap gap-1">
              {COMMON_LANGUAGES.map((lang) => (
                <Button
                  key={lang}
                  variant="outline"
                  size="sm"
                  onClick={() => addLanguage(lang)}
                  disabled={(filters.languages || []).includes(lang)}
                  className="text-xs h-6"
                >
                  {lang}
                </Button>
              ))}
            </div>
            
            {filters.languages && filters.languages.length > 0 && (
              <div className="flex flex-wrap gap-1">
                {filters.languages.map((lang) => (
                  <Badge
                    key={lang}
                    variant="secondary"
                    className="text-xs"
                  >
                    {lang}
                    <button
                      onClick={() => removeLanguage(lang)}
                      className="ml-1 hover:text-red-500"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))}
              </div>
            )}
          </div>
          
          {/* File Types Filter */}
          <div className="space-y-2">
            <div className="flex items-center space-x-2">
              <FileType className="h-4 w-4 text-gray-500" />
              <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                File Types
              </label>
            </div>
            
            <div className="flex space-x-2">
              <Input
                placeholder="File extension..."
                value={newFileType}
                onChange={(e) => setNewFileType(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && addFileType(newFileType)}
                className="text-sm"
              />
              <Button
                type="button"
                size="sm"
                onClick={() => addFileType(newFileType)}
                disabled={!newFileType.trim()}
              >
                Add
              </Button>
            </div>
            
            {/* Common File Types */}
            <div className="flex flex-wrap gap-1">
              {COMMON_FILE_TYPES.slice(0, 12).map((type) => (
                <Button
                  key={type}
                  variant="outline"
                  size="sm"
                  onClick={() => addFileType(type)}
                  disabled={(filters.fileTypes || []).includes(type)}
                  className="text-xs h-6 font-mono"
                >
                  .{type}
                </Button>
              ))}
            </div>
            
            {filters.fileTypes && filters.fileTypes.length > 0 && (
              <div className="flex flex-wrap gap-1">
                {filters.fileTypes.map((type) => (
                  <Badge
                    key={type}
                    variant="secondary"
                    className="text-xs font-mono"
                  >
                    .{type}
                    <button
                      onClick={() => removeFileType(type)}
                      className="ml-1 hover:text-red-500"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))}
              </div>
            )}
          </div>
          
          {/* Date Range and Score Filter */}
          <div className="space-y-4">
            {/* Date Range */}
            <div className="space-y-2">
              <div className="flex items-center space-x-2">
                <Calendar className="h-4 w-4 text-gray-500" />
                <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Date Range
                </label>
                {filters.dateRange && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={clearDateRange}
                    className="h-6 w-6 p-0"
                  >
                    <X className="h-3 w-3" />
                  </Button>
                )}
              </div>
              
              <div className="grid grid-cols-2 gap-2">
                <Input
                  type="date"
                  value={filters.dateRange?.from || ''}
                  onChange={(e) => updateDateRange('from', e.target.value)}
                  className="text-sm"
                  placeholder="From"
                />
                <Input
                  type="date"
                  value={filters.dateRange?.to || ''}
                  onChange={(e) => updateDateRange('to', e.target.value)}
                  className="text-sm"
                  placeholder="To"
                />
              </div>
            </div>
            
            {/* Minimum Score */}
            <div className="space-y-2">
              <div className="flex items-center space-x-2">
                <Star className="h-4 w-4 text-gray-500" />
                <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Minimum Relevance Score
                </label>
              </div>
              
              <div className="flex items-center space-x-2">
                <Input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={filters.minScore || 0.1}
                  onChange={(e) => updateFilters({ minScore: parseFloat(e.target.value) })}
                  className="flex-1"
                />
                <span className="text-sm text-gray-600 dark:text-gray-400 min-w-[3rem]">
                  {((filters.minScore || 0.1) * 100).toFixed(0)}%
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Card>
  );
};

export default SearchFilters;