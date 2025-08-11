'use client';

import React, { useMemo } from 'react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { 
  FileText, 
  Hash, 
  Clock, 
  ChevronRight,
  Search
} from 'lucide-react';
import { cn } from '@/lib/utils';

interface DocumentationSection {
  id: string;
  title: string;
  content: string;
  type: string;
  order: number;
  metadata?: {
    wordCount?: number;
    readingTime?: number;
    lastModified?: string;
  };
}

interface DocumentationTOCProps {
  sections: DocumentationSection[];
  selectedSection: string | null;
  onSectionSelect: (sectionId: string | null) => void;
  searchTerm?: string;
  className?: string;
}

const getSectionIcon = (type: string) => {
  switch (type.toLowerCase()) {
    case 'overview':
      return '📋';
    case 'getting-started':
      return '🚀';
    case 'api':
      return '🔌';
    case 'guide':
      return '📖';
    case 'tutorial':
      return '🎯';
    case 'reference':
      return '📚';
    case 'examples':
      return '📝';
    case 'troubleshooting':
      return '🔧';
    case 'faq':
      return '❓';
    default:
      return '📄';
  }
};

const highlightSearchTerm = (text: string, searchTerm: string): React.ReactNode => {
  if (!searchTerm.trim()) return text;
  
  const regex = new RegExp(`(${searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
  const parts = text.split(regex);
  
  return (
    <>
      {parts.map((part, index) => 
        regex.test(part) ? (
          <span key={index} className="bg-yellow-200 dark:bg-yellow-700 px-1 rounded">
            {part}
          </span>
        ) : (
          part
        )
      )}
    </>
  );
};

export const DocumentationTOC: React.FC<DocumentationTOCProps> = ({
  sections,
  selectedSection,
  onSectionSelect,
  searchTerm = '',
  className = ''
}) => {
  // Filter sections based on search term
  const filteredSections = useMemo(() => {
    if (!searchTerm.trim()) return sections;
    
    return sections.filter(section =>
      section.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      section.content.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [sections, searchTerm]);

  // Sort sections by order
  const sortedSections = useMemo(() => {
    return [...filteredSections].sort((a, b) => a.order - b.order);
  }, [filteredSections]);

  const totalReadingTime = useMemo(() => {
    return sortedSections.reduce((total, section) => 
      total + (section.metadata?.readingTime || 0), 0
    );
  }, [sortedSections]);

  if (sections.length === 0) {
    return (
      <div className={cn('p-4 text-center text-gray-500', className)}>
        <FileText className="h-8 w-8 mx-auto mb-2 text-gray-400" />
        <p>No sections available</p>
      </div>
    );
  }

  return (
    <div className={cn('flex flex-col h-full', className)}>
      {/* Header */}
      <div className="p-4 border-b border-gray-200 dark:border-gray-700">
        <h3 className="font-semibold text-gray-900 dark:text-white mb-2">
          Table of Contents
        </h3>
        
        <div className="flex items-center space-x-2 text-xs text-gray-500">
          <Badge variant="outline">
            {sortedSections.length} sections
          </Badge>
          {totalReadingTime > 0 && (
            <div className="flex items-center space-x-1">
              <Clock className="h-3 w-3" />
              <span>{totalReadingTime} min</span>
            </div>
          )}
        </div>
        
        {searchTerm && (
          <div className="mt-2 p-2 bg-blue-50 dark:bg-blue-900/20 rounded-md">
            <div className="flex items-center space-x-1 text-xs text-blue-600 dark:text-blue-400">
              <Search className="h-3 w-3" />
              <span>Filtered by: "{searchTerm}"</span>
            </div>
          </div>
        )}
      </div>

      {/* Sections List */}
      <div className="flex-1 overflow-auto">
        {sortedSections.length === 0 ? (
          <div className="p-4 text-center text-gray-500">
            <Search className="h-6 w-6 mx-auto mb-2 text-gray-400" />
            <p className="text-sm">No sections match your search</p>
          </div>
        ) : (
          <div className="py-2">
            {/* Overview/All Sections option */}
            <Button
              variant="ghost"
              className={cn(
                'w-full justify-start px-4 py-2 h-auto text-left',
                selectedSection === null && 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400'
              )}
              onClick={() => onSectionSelect(null)}
            >
              <div className="flex items-center space-x-3">
                <span className="text-lg">📝</span>
                <div className="flex-1">
                  <div className="font-medium text-sm">
                    All Sections
                  </div>
                  <div className="text-xs text-gray-500 dark:text-gray-400">
                    Complete documentation
                  </div>
                </div>
              </div>
            </Button>
            
            {/* Individual sections */}
            {sortedSections.map((section, index) => {
              const isSelected = selectedSection === section.id;
              
              return (
                <Button
                  key={section.id}
                  variant="ghost"
                  className={cn(
                    'w-full justify-start px-4 py-3 h-auto text-left',
                    isSelected && 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400'
                  )}
                  onClick={() => onSectionSelect(section.id)}
                >
                  <div className="flex items-start space-x-3 w-full">
                    <div className="flex items-center space-x-2 flex-shrink-0">
                      <span className="text-lg">
                        {getSectionIcon(section.type)}
                      </span>
                      {isSelected && (
                        <ChevronRight className="h-3 w-3 text-blue-600" />
                      )}
                    </div>
                    
                    <div className="flex-1 min-w-0">
                      <div className="font-medium text-sm truncate">
                        {highlightSearchTerm(section.title, searchTerm)}
                      </div>
                      
                      <div className="flex items-center space-x-2 mt-1">
                        <Badge variant="outline" className="text-xs">
                          {section.type}
                        </Badge>
                        
                        {section.metadata?.wordCount && (
                          <span className="text-xs text-gray-500">
                            {section.metadata.wordCount} words
                          </span>
                        )}
                        
                        {section.metadata?.readingTime && (
                          <div className="flex items-center space-x-1 text-xs text-gray-500">
                            <Clock className="h-3 w-3" />
                            <span>{section.metadata.readingTime}m</span>
                          </div>
                        )}
                      </div>
                      
                      {/* Section preview */}
                      {section.content && (
                        <div className="text-xs text-gray-400 mt-1 line-clamp-2">
                          {highlightSearchTerm(
                            section.content.substring(0, 100) + 
                            (section.content.length > 100 ? '...' : ''),
                            searchTerm
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                </Button>
              );
            })}
          </div>
        )}
      </div>
      
      {/* Footer */}
      {sortedSections.length > 0 && (
        <div className="p-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800">
          <div className="text-xs text-gray-500 text-center">
            {searchTerm ? (
              <>Showing {sortedSections.length} of {sections.length} sections</>
            ) : (
              <>Total: {sections.length} sections</>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default DocumentationTOC;