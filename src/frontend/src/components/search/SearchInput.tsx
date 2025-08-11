'use client';

import React, { useState, useRef, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { 
  Search, 
  X, 
  Clock, 
  TrendingUp,
  ArrowUpRight,
  Loader2
} from 'lucide-react';
import { cn } from '@/lib/utils';

interface SearchInputProps {
  query: string;
  suggestions: string[];
  recentSearches: string[];
  onQueryChange: (query: string) => void;
  onSearch: (query: string) => void;
  onClear: () => void;
  isLoading?: boolean;
  placeholder?: string;
  className?: string;
}

export const SearchInput: React.FC<SearchInputProps> = ({
  query,
  suggestions,
  recentSearches,
  onQueryChange,
  onSearch,
  onClear,
  isLoading = false,
  placeholder = "Search repositories, files, functions, documentation...",
  className = ''
}) => {
  const [isFocused, setIsFocused] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Combine suggestions and recent searches
  const allSuggestions = React.useMemo(() => {
    const items: Array<{text: string, type: 'suggestion' | 'recent'}> = [];
    
    // Add query-based suggestions first
    suggestions.forEach(suggestion => {
      items.push({ text: suggestion, type: 'suggestion' });
    });
    
    // Add recent searches that don't match current suggestions
    if (query.length < 3) {
      recentSearches
        .filter(recent => !suggestions.includes(recent))
        .slice(0, 5)
        .forEach(recent => {
          items.push({ text: recent, type: 'recent' });
        });
    }
    
    return items;
  }, [suggestions, recentSearches, query]);

  const showDropdown = isFocused && (allSuggestions.length > 0 || query.length > 0);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim()) {
      onSearch(query.trim());
      setIsFocused(false);
      inputRef.current?.blur();
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!showDropdown) return;

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setSelectedIndex(prev => 
          prev < allSuggestions.length - 1 ? prev + 1 : 0
        );
        break;
      case 'ArrowUp':
        e.preventDefault();
        setSelectedIndex(prev => 
          prev > 0 ? prev - 1 : allSuggestions.length - 1
        );
        break;
      case 'Enter':
        e.preventDefault();
        if (selectedIndex >= 0 && allSuggestions[selectedIndex]) {
          const selectedText = allSuggestions[selectedIndex].text;
          onQueryChange(selectedText);
          onSearch(selectedText);
          setIsFocused(false);
          inputRef.current?.blur();
        } else if (query.trim()) {
          onSearch(query.trim());
          setIsFocused(false);
          inputRef.current?.blur();
        }
        break;
      case 'Escape':
        setIsFocused(false);
        inputRef.current?.blur();
        break;
    }
  };

  const handleSuggestionClick = (suggestion: string) => {
    onQueryChange(suggestion);
    onSearch(suggestion);
    setIsFocused(false);
    inputRef.current?.blur();
  };

  const handleClear = () => {
    onClear();
    setSelectedIndex(-1);
    inputRef.current?.focus();
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node) &&
        !inputRef.current?.contains(event.target as Node)
      ) {
        setIsFocused(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Reset selected index when suggestions change
  useEffect(() => {
    setSelectedIndex(-1);
  }, [allSuggestions]);

  return (
    <div className={cn('relative', className)}>
      <form onSubmit={handleSubmit} className="relative">
        {/* Search Icon */}
        <div className="absolute left-3 top-1/2 transform -translate-y-1/2 z-10">
          {isLoading ? (
            <Loader2 className="h-5 w-5 text-gray-400 animate-spin" />
          ) : (
            <Search className="h-5 w-5 text-gray-400" />
          )}
        </div>
        
        {/* Input Field */}
        <Input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => onQueryChange(e.target.value)}
          onFocus={() => setIsFocused(true)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          disabled={isLoading}
          className={cn(
            'pl-10 pr-20 h-12 text-lg',
            showDropdown && 'rounded-b-none border-b-0'
          )}
        />
        
        {/* Clear and Search Buttons */}
        <div className="absolute right-2 top-1/2 transform -translate-y-1/2 flex items-center space-x-1">
          {query && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={handleClear}
              disabled={isLoading}
              className="h-8 w-8 p-0"
            >
              <X className="h-4 w-4" />
            </Button>
          )}
          
          <Button
            type="submit"
            size="sm"
            disabled={!query.trim() || isLoading}
            className="h-8"
          >
            <Search className="h-4 w-4 mr-1" />
            <span className="hidden sm:inline">Search</span>
          </Button>
        </div>
      </form>
      
      {/* Suggestions Dropdown */}
      {showDropdown && (
        <div 
          ref={dropdownRef}
          className="absolute top-full left-0 right-0 z-50 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-b-md shadow-lg max-h-96 overflow-y-auto"
        >
          {allSuggestions.length === 0 && query.length > 0 ? (
            <div className="p-4 text-center text-gray-500 dark:text-gray-400">
              <Search className="h-6 w-6 mx-auto mb-2" />
              <p className="text-sm">
                No suggestions available. Press Enter to search for "{query}"
              </p>
            </div>
          ) : (
            <>
              {allSuggestions.map((item, index) => (
                <button
                  key={`${item.type}-${item.text}-${index}`}
                  type="button"
                  onClick={() => handleSuggestionClick(item.text)}
                  className={cn(
                    'w-full px-4 py-3 text-left hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center justify-between border-b border-gray-100 dark:border-gray-700 last:border-b-0',
                    selectedIndex === index && 'bg-blue-50 dark:bg-blue-900/20'
                  )}
                >
                  <div className="flex items-center space-x-3">
                    <div className="flex-shrink-0">
                      {item.type === 'recent' ? (
                        <Clock className="h-4 w-4 text-gray-400" />
                      ) : (
                        <TrendingUp className="h-4 w-4 text-blue-500" />
                      )}
                    </div>
                    
                    <div className="flex-1">
                      <div className="text-sm font-medium text-gray-900 dark:text-white">
                        {item.text}
                      </div>
                      {item.type === 'recent' && (
                        <div className="text-xs text-gray-500 dark:text-gray-400">
                          Recent search
                        </div>
                      )}
                    </div>
                  </div>
                  
                  <ArrowUpRight className="h-4 w-4 text-gray-400" />
                </button>
              ))}
              
              {/* Search current query option */}
              {query.length > 0 && !allSuggestions.some(s => s.text.toLowerCase() === query.toLowerCase()) && (
                <button
                  type="button"
                  onClick={() => handleSuggestionClick(query)}
                  className={cn(
                    'w-full px-4 py-3 text-left hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center justify-between border-t border-gray-200 dark:border-gray-700',
                    selectedIndex === allSuggestions.length && 'bg-blue-50 dark:bg-blue-900/20'
                  )}
                >
                  <div className="flex items-center space-x-3">
                    <Search className="h-4 w-4 text-gray-400" />
                    <div className="text-sm font-medium text-gray-900 dark:text-white">
                      Search for "{query}"
                    </div>
                  </div>
                  <Badge variant="outline">Enter</Badge>
                </button>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
};

export default SearchInput;